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
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET api/employee/my-company - Customer gets their employees
        [HttpGet("my-company")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyEmployees()
        {
            try
            {
                int customerId = GetUserId();
                var employees = await _employeeService.GetMyEmployeesAsync(customerId);
                return Ok(employees);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        // POST api/employee - Customer adds employee to their company
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeDto dto)
        {
            try
            {
                int customerId = GetUserId();
                var employeeId = await _employeeService.AddEmployeeAsync(customerId, dto);
                return Ok(new { message = "Employee added successfully", employeeId = employeeId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        // PUT api/employee/{id} - Customer updates employee
        [HttpPut("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
             try
            {
                int customerId = GetUserId();
                await _employeeService.UpdateEmployeeAsync(customerId, id, dto);
                return Ok(new { message = "Employee updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        // PUT api/employee/{id}/deactivate - Customer deactivates employee
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeactivateEmployee(int id)
        {
             try
            {
                int customerId = GetUserId();
                await _employeeService.DeactivateEmployeeAsync(customerId, id);
                return Ok(new { message = "Employee deactivated" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}