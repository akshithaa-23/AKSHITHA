using Application.DTOs;

namespace Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetMyEmployeesAsync(int customerId);
        Task<int> AddEmployeeAsync(int customerId, AddEmployeeDto dto);
        Task UpdateEmployeeAsync(int customerId, int employeeId, UpdateEmployeeDto dto);
        Task DeactivateEmployeeAsync(int customerId, int employeeId);
    }
}
