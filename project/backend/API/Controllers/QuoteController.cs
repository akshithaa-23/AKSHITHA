using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuoteController : ControllerBase
    {
        private readonly IQuoteService _quoteService;
        public QuoteController(IQuoteService quoteService) { _quoteService = quoteService; }

        // POST api/quote — Agent sends calculated quote
        [HttpPost]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> SendQuote([FromBody] SendQuoteDto dto)
        {
            try
            {
                int agentId = GetUserId();
                var result = await _quoteService.SendQuoteAsync(agentId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/quote/my — Customer views their quotes
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyQuotes()
        {
            int customerId = GetUserId();
            var quotes = await _quoteService.GetMyQuotesAsync(customerId);
            return Ok(quotes);
        }

        // GET api/quote/agent — Agent views quotes they sent
        [HttpGet("agent")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetAgentQuotes()
        {
            int agentId = GetUserId();
            var quotes = await _quoteService.GetAgentQuotesAsync(agentId);
            return Ok(quotes);
        }

        // PUT api/quote/{id}/accept — Customer accepts quote
        [HttpPut("{id}/accept")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Accept(int id)
        {
            try
            {
                int customerId = GetUserId();
                var message = await _quoteService.AcceptQuoteAsync(customerId, id);
                return Ok(new { message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT api/quote/{id}/reject — Customer rejects quote
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                int customerId = GetUserId();
                var message = await _quoteService.RejectQuoteAsync(customerId, id);
                return Ok(new { message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}