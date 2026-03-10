using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        [HttpGet("allowed-types")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetAllowedClaimTypes()
        {
            try
            {
                var result = await _claimService.GetAllowedClaimTypesAsync(GetUserId());
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("health")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RaiseHealthClaim([FromBody] RaiseHealthClaimDto dto)
        {
            try
            {
                var result = await _claimService.RaiseHealthClaimAsync(GetUserId(), dto);
                var isAutoRejected = result.GetType().GetProperty("autoRejected")?.GetValue(result, null) as bool?;
                if (isAutoRejected == true) return BadRequest(result);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("term-life")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RaiseTermLifeClaim([FromBody] RaiseTermLifeClaimDto dto)
        {
            try
            {
                var result = await _claimService.RaiseTermLifeClaimAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("accident")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RaiseAccidentClaim([FromBody] RaiseAccidentClaimDto dto)
        {
            try
            {
                var result = await _claimService.RaiseAccidentClaimAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyClaims()
        {
            var claims = await _claimService.GetMyClaimsAsync(GetUserId());
            return Ok(claims);
        }

        [HttpGet("manager")]
        [Authorize(Roles = "ClaimsManager")]
        public async Task<IActionResult> GetManagerClaims()
        {
            var claims = await _claimService.GetManagerClaimsAsync(GetUserId());
            return Ok(claims);
        }

        [HttpPut("{id}/process")]
        [Authorize(Roles = "ClaimsManager")]
        public async Task<IActionResult> ProcessClaim(int id, [FromBody] ProcessClaimDto dto)
        {
            try
            {
                var result = await _claimService.ProcessClaimAsync(GetUserId(), id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllClaims()
        {
            var claims = await _claimService.GetAllClaimsAsync();
            return Ok(claims);
        }

        [HttpPost("upload-document")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UploadClaimDocument(IFormFile file)
        {
            try
            {
                var fileUrl = await _claimService.UploadClaimDocumentAsync(file, Request.Scheme, Request.Host.Value);
                return Ok(new { fileUrl });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}