using Application.Interfaces;
using System;
using System.Linq;

namespace Application.Services
{
    public class PremiumCalculationService : IPremiumCalculationService
    {
        public decimal GetIndustryFactor(string industryType, string? customIndustry)
        {
            return industryType switch
            {
                "Technology / IT"       => 1.00m,
                "Finance / Banking"     => 1.03m,
                "Education"             => 1.03m,
                "Healthcare"            => 1.05m,
                "Retail / Trade"        => 1.07m,
                "Manufacturing"         => 1.12m,
                "Logistics / Transport" => 1.15m,
                "Construction"          => 1.20m,
                _                       => 1.07m  // Other
            };
        }

        public decimal GetGeographyFactor(string location, string locationCategory)
        {
            var tier1 = new[] { "Mumbai", "Delhi", "Bengaluru", "Chennai", "Hyderabad", "Pune", "Kolkata" };
            var tier2 = new[] { "Ahmedabad", "Visakhapatnam", "Lucknow", "Coimbatore", "Nagpur", "Kochi", "Bhubaneswar" };
            var tier3 = new[] { "Warangal", "Tirupati", "Nashik", "Madurai", "Mysuru", "Mangaluru", "Hubballi" };

            if (tier1.Contains(location)) return 1.10m;
            if (tier2.Contains(location)) return 1.05m;
            if (tier3.Contains(location)) return 1.00m;

            return locationCategory switch
            {
                "Metropolitan" => 1.10m,
                "Urban"        => 1.05m,
                "Semi-Urban"   => 1.00m,
                _              => 1.00m
            };
        }

        public decimal GetPlanRiskFactor(int policyId)
        {
            return policyId switch
            {
                1 => 1.00m,  // Essential Base
                2 => 1.05m,  // Essential Plus
                3 => 1.10m,  // Essential Pro
                4 => 1.03m,  // Enhanced Base
                5 => 1.08m,  // Enhanced Plus
                6 => 1.13m,  // Enhanced Pro
                7 => 1.05m,  // Enterprise Base
                8 => 1.10m,  // Enterprise Plus
                9 => 1.15m,  // Enterprise Pro
                _ => 1.00m
            };
        }
    }
}
