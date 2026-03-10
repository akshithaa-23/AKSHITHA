using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        public CompanyController(ICompanyService companyService) { _companyService = companyService; }

        // GET api/company/my — Customer views their own company profile
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyCompany()
        {
            int customerId = GetUserId();
            var company = await _companyService.GetMyCompanyAsync(customerId);

            if (company == null)
                return NotFound(new { message = "No company registered yet" });

            return Ok(company);
        }

        // GET api/company/all — Admin: all companies with policy + agent info
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetAllAsync();
            return Ok(companies);
        }

        // GET api/company/by-agent — Admin: which agent handles which customers
        [HttpGet("by-agent")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByAgent()
        {
            var agents = await _companyService.GetByAgentAsync();
            return Ok(agents);
        }

        // GET api/company/policy-summary — Admin: policy purchase summary
        [HttpGet("policy-summary")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPolicySummary()
        {
            var summary = await _companyService.GetPolicySummaryAsync();
            return Ok(summary);
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}