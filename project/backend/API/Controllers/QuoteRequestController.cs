using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuoteRequestController : ControllerBase
    {
        private readonly IQuoteRequestService _quoteRequestService;

        public QuoteRequestController(IQuoteRequestService quoteRequestService)
        {
            _quoteRequestService = quoteRequestService;
        }

        [HttpPost("recommendation")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RequestRecommendation([FromBody] QuoteRequestDto dto)
        {
            try
            {
                var result = await _quoteRequestService.RequestRecommendationAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("direct-buy")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DirectBuy([FromBody] DirectBuyRequestDto dto)
        {
            try
            {
                var result = await _quoteRequestService.DirectBuyAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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

        [HttpGet("customer")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyRequests()
        {
            var requests = await _quoteRequestService.GetMyRequestsAsync(GetUserId());
            return Ok(requests);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _quoteRequestService.GetAllAsync();
            return Ok(requests);
        }

        [HttpGet("agent")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetAgentRequests()
        {
            var requests = await _quoteRequestService.GetAgentRequestsAsync(GetUserId());
            return Ok(requests);
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}