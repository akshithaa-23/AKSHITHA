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
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/employee/my-company - Customer gets their employees
        [HttpGet("my-company")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyEmployees()
        {
            int customerId = GetUserId();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null)
                return NotFound(new { message = "No company registered yet" });

            var employees = await _context.Employees
                .Where(e => e.CompanyId == company.Id)
                .OrderBy(e => e.FullName)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FullName,
                    Email = e.Email,
                    Gender = e.Gender,
                    Salary = e.Salary,
                    IsActive = e.IsActive,
                    CoverageStartDate = e.CoverageStartDate,
                    NomineeName = e.NomineeName,
                    NomineeRelationship = e.NomineeRelationship,
                    NomineePhone = e.NomineePhone,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(employees);
        }

        // POST api/employee - Customer adds employee to their company
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeDto dto)
        {
            int customerId = GetUserId();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null)
                return BadRequest(new { message = "Please register your company first" });

            // Check duplicate employee code
            var exists = await _context.Employees
                .AnyAsync(e => e.CompanyId == company.Id && e.EmployeeCode == dto.EmployeeCode);
            if (exists)
                return BadRequest(new { message = "Employee code already exists in your company" });

            var employee = new Employee
            {
                EmployeeCode = dto.EmployeeCode,
                FullName = dto.FullName,
                Email = dto.Email,
                Gender = dto.Gender,
                Salary = dto.Salary,
                CompanyId = company.Id,
                IsActive = true,
                CoverageStartDate = dto.CoverageStartDate,
                NomineeName = dto.NomineeName,
                NomineeRelationship = dto.NomineeRelationship,
                NomineePhone = dto.NomineePhone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee added successfully", employeeId = employee.Id });
        }

        // PUT api/employee/{id} - Customer updates employee
        [HttpPut("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
            int customerId = GetUserId();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null) return NotFound(new { message = "Company not found" });

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == company.Id);

            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            employee.FullName = dto.FullName;
            employee.Email = dto.Email;
            employee.Gender = dto.Gender;
            employee.Salary = dto.Salary;
            employee.NomineeName = dto.NomineeName;
            employee.NomineeRelationship = dto.NomineeRelationship;
            employee.NomineePhone = dto.NomineePhone;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee updated successfully" });
        }

        // PUT api/employee/{id}/deactivate - Customer deactivates employee
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeactivateEmployee(int id)
        {
            int customerId = GetUserId();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null) return NotFound(new { message = "Company not found" });

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == company.Id);

            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            employee.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee deactivated" });
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}