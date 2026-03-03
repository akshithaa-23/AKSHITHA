namespace Application.DTOs
{
    public class AddEmployeeDto
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string NomineeName { get; set; } = string.Empty;
        public string NomineeRelationship { get; set; } = string.Empty;
        public string NomineePhone { get; set; } = string.Empty;
        public DateTime CoverageStartDate { get; set; } = DateTime.UtcNow;
    }

    public class UpdateEmployeeDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string NomineeName { get; set; } = string.Empty;
        public string NomineeRelationship { get; set; } = string.Empty;
        public string NomineePhone { get; set; } = string.Empty;
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CoverageStartDate { get; set; }
        public string NomineeName { get; set; } = string.Empty;
        public string NomineeRelationship { get; set; } = string.Empty;
        public string NomineePhone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }




}