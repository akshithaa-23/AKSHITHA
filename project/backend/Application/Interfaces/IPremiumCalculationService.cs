using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPremiumCalculationService
    {
        decimal GetIndustryFactor(string industryType, string? customIndustry);
        decimal GetGeographyFactor(string location, string locationCategory);
        decimal GetPlanRiskFactor(int policyId);
    }
}
