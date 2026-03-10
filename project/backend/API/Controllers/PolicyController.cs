using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _policyService;
        public PolicyController(IPolicyService policyService) { _policyService = policyService; }

        // GET api/policy - Active policies
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var policies = await _policyService.GetAllActiveAsync();
            return Ok(policies);
        }

        // GET api/policy/all - Admin only
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAdmin()
        {
            var policies = await _policyService.GetAllAdminAsync();
            return Ok(policies);
        }

        // GET api/policy/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _policyService.GetByIdAsync(id);
            if (p == null) return NotFound(new { message = "Policy not found" });

            return Ok(p);
        }

        // POST api/policy - Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePolicyDto dto)
        {
            try
            {
                var policyId = await _policyService.CreateAsync(dto);
                return Ok(new { message = "Policy created successfully", policyId = policyId });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // PUT api/policy/{id} - Admin only
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePolicyDto dto)
        {
            try
            {
                await _policyService.UpdateAsync(id, dto);
                return Ok(new { message = "Policy updated successfully" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // DELETE api/policy/{id} - Admin soft delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _policyService.DeleteAsync(id);
                return Ok(new { message = "Policy deactivated successfully" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}