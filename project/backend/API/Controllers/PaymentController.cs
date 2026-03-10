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
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService) { _paymentService = paymentService; }

        // POST api/payment — Customer pays for accepted quote
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
        {
            try
            {
                int customerId = GetUserId();
                var paymentResponse = await _paymentService.ProcessPaymentAsync(customerId, dto);
                return Ok(paymentResponse);
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

        // GET api/payment/my — Customer views payment history & invoices
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyPayments()
        {
            int customerId = GetUserId();
            var payments = await _paymentService.GetMyPaymentsAsync(customerId);
            return Ok(payments);
        }

        // GET api/payment/agent — Agent views commissions earned
        [HttpGet("agent")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetAgentCommissions()
        {
            int agentId = GetUserId();
            var commissions = await _paymentService.GetAgentCommissionsAsync(agentId);
            return Ok(commissions);
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}