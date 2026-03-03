using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuoteController : ControllerBase
    {
        private readonly AppDbContext _context;
        public QuoteController(AppDbContext context) { _context = context; }

        // POST api/quote — Agent sends calculated quote
        [HttpPost]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> SendQuote([FromBody] SendQuoteDto dto)
        {
            int agentId = GetUserId();

            var quoteRequest = await _context.QuoteRequests.FindAsync(dto.QuoteRequestId);
            if (quoteRequest == null)
                return NotFound(new { message = "Quote request not found" });

            if (quoteRequest.AssignedAgentId != agentId)
                return Forbid();

            var policy = await _context.Policies.FindAsync(dto.PolicyId);
            if (policy == null)
                return NotFound(new { message = "Policy not found" });

            // Business rule: employee count must meet policy minimum
            if (dto.EmployeeCount < policy.MinEmployees)
                return BadRequest(new
                {
                    message = $"Minimum {policy.MinEmployees} employees required for this policy"
                });

            decimal totalPremium = dto.EmployeeCount * policy.PremiumPerEmployee;

            var quote = new Quote
            {
                QuoteRequestId = dto.QuoteRequestId,
                AgentId = agentId,
                CustomerId = quoteRequest.CustomerId,
                PolicyId = dto.PolicyId,
                EmployeeCount = dto.EmployeeCount,
                PremiumPerEmployee = policy.PremiumPerEmployee,
                TotalPremium = totalPremium,
                Status = "Pending",
                ValidUntil = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };

            quoteRequest.Status = "QuoteSent";
            quoteRequest.PolicyId = dto.PolicyId;

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Quote sent successfully",
                quoteId = quote.Id,
                totalPremium,
                validUntil = quote.ValidUntil
            });
        }

        // GET api/quote/my — Customer views their quotes
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyQuotes()
        {
            int customerId = GetUserId();

            var quotes = await _context.Quotes
                .Include(q => q.Policy)
                .Include(q => q.Agent)
                .Include(q => q.QuoteRequest)
                .Where(q => q.CustomerId == customerId)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuoteResponseDto
                {
                    Id = q.Id,
                    QuoteRequestId = q.QuoteRequestId,
                    PolicyId = q.PolicyId,
                    PolicyName = q.Policy.Name,
                    EmployeeCount = q.EmployeeCount,
                    PremiumPerEmployee = q.PremiumPerEmployee,
                    TotalPremium = q.TotalPremium,
                    Status = q.Status,
                    AgentName = q.Agent.FullName,
                    ValidUntil = q.ValidUntil,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();

            return Ok(quotes);
        }

        // GET api/quote/agent — Agent views quotes they sent
        [HttpGet("agent")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetAgentQuotes()
        {
            int agentId = GetUserId();

            var quotes = await _context.Quotes
                .Include(q => q.Policy)
                .Include(q => q.Customer)
                .Where(q => q.AgentId == agentId)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuoteResponseDto
                {
                    Id = q.Id,
                    QuoteRequestId = q.QuoteRequestId,
                    PolicyId = q.PolicyId,
                    PolicyName = q.Policy.Name,
                    EmployeeCount = q.EmployeeCount,
                    PremiumPerEmployee = q.PremiumPerEmployee,
                    TotalPremium = q.TotalPremium,
                    Status = q.Status,
                    AgentName = q.Customer.FullName,
                    ValidUntil = q.ValidUntil,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();

            return Ok(quotes);
        }

        // PUT api/quote/{id}/accept — Customer accepts quote
        [HttpPut("{id}/accept")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Accept(int id)
        {
            int customerId = GetUserId();
            var quote = await _context.Quotes
                .FirstOrDefaultAsync(q => q.Id == id && q.CustomerId == customerId);

            if (quote == null) return NotFound(new { message = "Quote not found" });
            if (quote.Status != "Pending") return BadRequest(new { message = "Quote already processed" });

            quote.Status = "Accepted";
            await _context.QuoteRequests
                .Where(q => q.Id == quote.QuoteRequestId)
                .ExecuteUpdateAsync(s => s.SetProperty(q => q.Status, "Accepted"));

            await _context.SaveChangesAsync();
            return Ok(new { message = "Quote accepted. Proceed to payment." });
        }

        // PUT api/quote/{id}/reject — Customer rejects quote
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Reject(int id)
        {
            int customerId = GetUserId();
            var quote = await _context.Quotes
                .FirstOrDefaultAsync(q => q.Id == id && q.CustomerId == customerId);

            if (quote == null) return NotFound(new { message = "Quote not found" });

            quote.Status = "Rejected";
            await _context.QuoteRequests
                .Where(q => q.Id == quote.QuoteRequestId)
                .ExecuteUpdateAsync(s => s.SetProperty(q => q.Status, "Rejected"));

            await _context.SaveChangesAsync();
            return Ok(new { message = "Quote rejected" });
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}