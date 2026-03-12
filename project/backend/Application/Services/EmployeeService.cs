using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IAppDbContext _context;

        public EmployeeService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeDto>> GetMyEmployeesAsync(int customerId)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null)
            {
                throw new InvalidOperationException("No company registered yet");
            }

            var employees = await _context.Employees
                .Include(e => e.Claims)
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
                    DateOfBirth = e.DateOfBirth,
                    EmployeeJoinDate = e.EmployeeJoinDate,
                    Age = DateTime.UtcNow.Year - e.DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < e.DateOfBirth.DayOfYear ? 1 : 0),
                    NomineeName = e.NomineeName,
                    NomineeRelationship = e.NomineeRelationship,
                    NomineePhone = e.NomineePhone,
                    HasPendingClaim = e.Claims != null && e.Claims.Any(c => c.Status == "Pending"),
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return employees;
        }

        public async Task<int> AddEmployeeAsync(int customerId, AddEmployeeDto dto)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null)
            {
                 throw new InvalidOperationException("Please register your company first");
            }

            // Check duplicate employee code
            var exists = await _context.Employees
                .AnyAsync(e => e.CompanyId == company.Id && e.EmployeeCode == dto.EmployeeCode);
                
            if (exists)
            {
                throw new InvalidOperationException("Employee code already exists in your company");
            }

            var employee = new Employee
            {
                EmployeeCode = dto.EmployeeCode,
                FullName = dto.FullName,
                Email = dto.Email,
                Gender = dto.Gender,
                Salary = dto.Salary,
                CompanyId = company.Id,
                IsActive = true,
                DateOfBirth = dto.DateOfBirth,
                EmployeeJoinDate = dto.EmployeeJoinDate,
                NomineeName = dto.NomineeName,
                NomineeRelationship = dto.NomineeRelationship,
                NomineePhone = dto.NomineePhone,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return employee.Id;
        }

        public async Task UpdateEmployeeAsync(int customerId, int employeeId, UpdateEmployeeDto dto)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null)
            {
                throw new KeyNotFoundException("Company not found");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.CompanyId == company.Id);

            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found");
            }

            employee.FullName = dto.FullName;
            employee.Email = dto.Email;
            employee.Gender = dto.Gender;
            employee.Salary = dto.Salary;
            employee.NomineeName = dto.NomineeName;
            employee.NomineeRelationship = dto.NomineeRelationship;
            employee.NomineePhone = dto.NomineePhone;

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateEmployeeAsync(int customerId, int employeeId)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (company == null)
            {
                throw new KeyNotFoundException("Company not found");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId && e.CompanyId == company.Id);

            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found");
            }

            employee.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
