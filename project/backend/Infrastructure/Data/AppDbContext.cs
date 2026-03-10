using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<User> Users { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyPolicy> CompanyPolicies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<QuoteRequest> QuoteRequests { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<RecommendationPolicy> RecommendationPolicies { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AgentCommission> AgentCommissions { get; set; }
        public DbSet<Claim> Claims { get; set; }   // NEW

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            // Policy
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.Property(p => p.HealthCoverage).HasPrecision(18, 2);
                entity.Property(p => p.MaxLifeCoverageLimit).HasPrecision(18, 2);
                entity.Property(p => p.AccidentCoverage).HasPrecision(18, 2);
                entity.Property(p => p.PremiumPerEmployee).HasPrecision(18, 2);
            });

            // Company 
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasOne(c => c.Customer).WithMany()
                      .HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.Agent).WithMany()
                      .HasForeignKey(c => c.AgentId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
                // NEW: Claims Manager FK
                entity.HasOne(c => c.ClaimsManager).WithMany()
                  .HasForeignKey(c => c.ClaimsManagerId).OnDelete(DeleteBehavior.NoAction).IsRequired(false);
            });

            



            // CompanyPolicy
            modelBuilder.Entity<CompanyPolicy>(entity =>
            {
                entity.Property(cp => cp.TotalPremium).HasPrecision(18, 2);
                entity.HasOne(cp => cp.Company).WithMany(c => c.CompanyPolicies)
                      .HasForeignKey(cp => cp.CompanyId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(cp => cp.Policy).WithMany(p => p.CompanyPolicies)
                      .HasForeignKey(cp => cp.PolicyId).OnDelete(DeleteBehavior.Restrict);
            });

            // Employee — new fields: HealthCoverageRemaining, AccidentClaimRaised
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Salary).HasPrecision(18, 2);
                entity.Property(e => e.HealthCoverageRemaining).HasPrecision(18, 2);
                entity.HasOne(e => e.Company).WithMany(c => c.Employees)
                      .HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Cascade);
            });

            // QuoteRequest
            modelBuilder.Entity<QuoteRequest>(entity =>
            {
                entity.HasOne(q => q.Customer).WithMany()
                      .HasForeignKey(q => q.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(q => q.AssignedAgent).WithMany()
                      .HasForeignKey(q => q.AssignedAgentId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
                entity.HasOne(q => q.Policy).WithMany()
                      .HasForeignKey(q => q.PolicyId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
                entity.HasOne(q => q.Recommendation).WithOne(r => r.QuoteRequest)
                      .HasForeignKey<Recommendation>(r => r.QuoteRequestId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(q => q.Quote).WithOne(qt => qt.QuoteRequest)
                      .HasForeignKey<Quote>(qt => qt.QuoteRequestId).OnDelete(DeleteBehavior.Cascade);
            });

            // Recommendation
            modelBuilder.Entity<Recommendation>(entity =>
            {
                entity.HasOne(r => r.Agent).WithMany()
                      .HasForeignKey(r => r.AgentId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(r => r.Customer).WithMany()
                      .HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });

            // RecommendationPolicy
            modelBuilder.Entity<RecommendationPolicy>(entity =>
            {
                entity.HasOne(rp => rp.Recommendation).WithMany(r => r.RecommendationPolicies)
                      .HasForeignKey(rp => rp.RecommendationId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(rp => rp.Policy).WithMany()
                      .HasForeignKey(rp => rp.PolicyId).OnDelete(DeleteBehavior.Restrict);
            });

            // Quote
            modelBuilder.Entity<Quote>(entity =>
            {
                entity.Property(q => q.PremiumPerEmployee).HasPrecision(18, 2);
                entity.Property(q => q.TotalPremium).HasPrecision(18, 2);
                entity.HasOne(q => q.Agent).WithMany()
                      .HasForeignKey(q => q.AgentId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(q => q.Customer).WithMany()
                      .HasForeignKey(q => q.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(q => q.Policy).WithMany()
                      .HasForeignKey(q => q.PolicyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(q => q.Payment).WithOne(p => p.Quote)
                      .HasForeignKey<Payment>(p => p.QuoteId).OnDelete(DeleteBehavior.Restrict);
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.AmountPaid).HasPrecision(18, 2);
                entity.HasOne(p => p.Customer).WithMany()
                      .HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Policy).WithMany()
                      .HasForeignKey(p => p.PolicyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.AgentCommission).WithOne(ac => ac.Payment)
                      .HasForeignKey<AgentCommission>(ac => ac.PaymentId).OnDelete(DeleteBehavior.Cascade);
            });

            // AgentCommission
            modelBuilder.Entity<AgentCommission>(entity =>
            {
                entity.Property(ac => ac.CommissionRate).HasPrecision(5, 2);
                entity.Property(ac => ac.CommissionAmount).HasPrecision(18, 2);
                entity.HasOne(ac => ac.Agent).WithMany()
                      .HasForeignKey(ac => ac.AgentId).OnDelete(DeleteBehavior.Restrict);
            });

            // ── NEW: Claim ────────────────────────────────────────────────────
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.Property(c => c.ClaimAmount).HasPrecision(18, 2);
                entity.Property(c => c.AccidentPercentage).HasPrecision(5, 2);

                entity.HasOne(c => c.CompanyPolicy).WithMany()
                      .HasForeignKey(c => c.CompanyPolicyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.Employee).WithMany(e => e.Claims)
                      .HasForeignKey(c => c.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.Customer).WithMany()
                      .HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.ClaimsManager).WithMany()
                      .HasForeignKey(c => c.ClaimsManagerId).OnDelete(DeleteBehavior.Restrict);
            });

          

            // Seed Admin — FIXED hash so it never changes between migrations
            // This hash is for password: Admin@123
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FullName = "System Admin",
                Email = "admin@groupinsurance.com",
                PasswordHash = "$2a$11$UUu/cFfejo9bEL/Z00yDwO3lHVu6kaoAOd.ybxnANFItUiMAG1xvO",
                Role = Domain.Enums.UserRole.Admin,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // Seed 9 Policies
            modelBuilder.Entity<Policy>().HasData(
    new Policy { Id = 1, Name = "Essential Base", HealthCoverage = 200000, LifeCoverageMultiplier = null, MaxLifeCoverageLimit = null, AccidentCoverage = null, PremiumPerEmployee = 3000, MinEmployees = 10, DurationYears = 1, IsPopular = false, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
    new Policy { Id = 2, Name = "Essential Plus", HealthCoverage = 300000, LifeCoverageMultiplier = 2, MaxLifeCoverageLimit = 1000000, AccidentCoverage = null, PremiumPerEmployee = 5000, MinEmployees = 10, DurationYears = 1, IsPopular = false, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
    new Policy { Id = 3, Name = "Essential Pro", HealthCoverage = 400000, LifeCoverageMultiplier = 3, MaxLifeCoverageLimit = 1500000, AccidentCoverage = 200000, PremiumPerEmployee = 7000, MinEmployees = 10, DurationYears = 1, IsPopular = false, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

    new Policy { Id = 4, Name = "Enhanced Base", HealthCoverage = 300000, LifeCoverageMultiplier = null, MaxLifeCoverageLimit = null, AccidentCoverage = null, PremiumPerEmployee = 6500, MinEmployees = 80, DurationYears = 1, IsPopular = false, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
    new Policy { Id = 5, Name = "Enhanced Plus", HealthCoverage = 400000, LifeCoverageMultiplier = 3, MaxLifeCoverageLimit = 4000000, AccidentCoverage = null, PremiumPerEmployee = 9500, MinEmployees = 80, DurationYears = 1, IsPopular = true, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
    new Policy { Id = 6, Name = "Enhanced Pro", HealthCoverage = 500000, LifeCoverageMultiplier = 4, MaxLifeCoverageLimit = 5000000, AccidentCoverage = 500000, PremiumPerEmployee = 12500, MinEmployees = 80, DurationYears = 1, IsPopular = false, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

    new Policy { Id = 7, Name = "Enterprise Base", HealthCoverage = 500000, LifeCoverageMultiplier = null, MaxLifeCoverageLimit = null, AccidentCoverage = null, PremiumPerEmployee = 12000, MinEmployees = 200, DurationYears = 1, IsPopular = false, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
    new Policy { Id = 8, Name = "Enterprise Plus", HealthCoverage = 700000, LifeCoverageMultiplier = 4, MaxLifeCoverageLimit = 10000000, AccidentCoverage = null, PremiumPerEmployee = 16000, MinEmployees = 200, DurationYears = 1, IsPopular = false, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
    new Policy { Id = 9, Name = "Enterprise Pro", HealthCoverage = 1000000, LifeCoverageMultiplier = 5, MaxLifeCoverageLimit = 15000000, AccidentCoverage = 1000000, PremiumPerEmployee = 21000, MinEmployees = 200, DurationYears = 1, IsPopular = false, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
);
        }
    }
}