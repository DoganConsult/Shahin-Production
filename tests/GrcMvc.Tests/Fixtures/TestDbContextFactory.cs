using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GrcMvc.Data;
using GrcMvc.Models.Entities;
using GrcMvc.Models.Entities.Catalogs;
using GrcMvc.Models.Entities.Marketing;

namespace GrcMvc.Tests.Fixtures;

/// <summary>
/// Factory for creating in-memory database contexts for testing
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates a new in-memory GrcDbContext for testing
    /// </summary>
    public static GrcDbContext Create(string? databaseName = null)
    {
        var dbName = databaseName ?? $"TestDb_{Guid.NewGuid()}";

        var options = new DbContextOptionsBuilder<GrcDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new GrcDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Creates a seeded database context with sample data
    /// </summary>
    public static GrcDbContext CreateWithSeedData(string? databaseName = null)
    {
        var context = Create(databaseName);
        SeedTestData(context);
        return context;
    }

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    public static void SeedTestData(GrcDbContext context)
    {
        var tenantId = Guid.NewGuid();

        // Seed Tenant
        var tenant = new Tenant
        {
            Id = tenantId,
            OrganizationName = "Test Tenant",
            TenantSlug = "test",
            AdminEmail = "admin@test.com",
            Email = "admin@test.com",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        context.Tenants.Add(tenant);

        // Seed RegulatorCatalogs
        var regulators = new List<RegulatorCatalog>
        {
            new() { Id = Guid.NewGuid(), Code = "NCA", NameEn = "National Cybersecurity Authority", NameAr = "الهيئة الوطنية للأمن السيبراني", RegionType = "saudi", IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "SAMA", NameEn = "Saudi Arabian Monetary Authority", NameAr = "مؤسسة النقد العربي السعودي", RegionType = "saudi", IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "CITC", NameEn = "Communications and IT Commission", NameAr = "هيئة الاتصالات وتقنية المعلومات", RegionType = "saudi", IsActive = true }
        };
        context.RegulatorCatalogs.AddRange(regulators);

        // Seed FrameworkCatalogs
        var frameworks = new List<FrameworkCatalog>
        {
            new() { Id = Guid.NewGuid(), Code = "ECC", TitleEn = "Essential Cybersecurity Controls", TitleAr = "ضوابط الأمن السيبراني الأساسية", Version = "2.0", IsActive = true, RegulatorId = regulators[0].Id },
            new() { Id = Guid.NewGuid(), Code = "CSF", TitleEn = "Cybersecurity Framework", TitleAr = "إطار الأمن السيبراني", Version = "1.0", IsActive = true, RegulatorId = regulators[1].Id },
            new() { Id = Guid.NewGuid(), Code = "PDPL", TitleEn = "Personal Data Protection Law", TitleAr = "نظام حماية البيانات الشخصية", Version = "1.0", IsActive = true, RegulatorId = regulators[2].Id }
        };
        context.FrameworkCatalogs.AddRange(frameworks);

        // Seed ControlCatalogs
        var controls = new List<ControlCatalog>
        {
            new() { Id = Guid.NewGuid(), ControlId = "ECC-1-1", ControlNumber = "1-1", TitleEn = "Cybersecurity Strategy", TitleAr = "استراتيجية الأمن السيبراني", FrameworkId = frameworks[0].Id, IsActive = true },
            new() { Id = Guid.NewGuid(), ControlId = "ECC-1-2", ControlNumber = "1-2", TitleEn = "Cybersecurity Roles", TitleAr = "أدوار الأمن السيبراني", FrameworkId = frameworks[0].Id, IsActive = true },
            new() { Id = Guid.NewGuid(), ControlId = "CSF-1-1", ControlNumber = "1-1", TitleEn = "Risk Assessment", TitleAr = "تقييم المخاطر", FrameworkId = frameworks[1].Id, IsActive = true }
        };
        context.ControlCatalogs.AddRange(controls);

        // Seed Testimonials
        var testimonials = new List<Testimonial>
        {
            new() { Id = Guid.NewGuid(), Quote = "Great platform!", QuoteAr = "منصة رائعة!", AuthorName = "Test User", AuthorTitle = "CTO", CompanyName = "Test Co", IsActive = true, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Quote = "Excellent service!", QuoteAr = "خدمة ممتازة!", AuthorName = "Another User", AuthorTitle = "CEO", CompanyName = "Another Co", IsActive = true, DisplayOrder = 2 }
        };
        context.Testimonials.AddRange(testimonials);

        // Seed Risks
        var risks = new List<Risk>
        {
            new() { Id = Guid.NewGuid(), Name = "Data Breach Risk", Description = "Risk of unauthorized data access", TenantId = tenantId, Status = "Open", Impact = 5, Likelihood = 3, CreatedDate = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Compliance Risk", Description = "Risk of non-compliance", TenantId = tenantId, Status = "Open", Impact = 4, Likelihood = 2, CreatedDate = DateTime.UtcNow }
        };
        context.Risks.AddRange(risks);

        // Seed Controls (tenant-specific)
        var tenantControls = new List<Control>
        {
            new() { Id = Guid.NewGuid(), ControlId = "CTRL-001", Name = "Access Control", Description = "User access management", TenantId = tenantId, Status = "Implemented", CreatedDate = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ControlId = "CTRL-002", Name = "Encryption", Description = "Data encryption at rest and transit", TenantId = tenantId, Status = "Planned", CreatedDate = DateTime.UtcNow }
        };
        context.Controls.AddRange(tenantControls);

        // Seed Policies
        var policies = new List<Policy>
        {
            new() { Id = Guid.NewGuid(), Title = "Information Security Policy", PolicyNumber = "POL-001", Content = "Security policy content", TenantId = tenantId, Status = "Active", CreatedDate = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Data Protection Policy", PolicyNumber = "POL-002", Content = "Data protection content", TenantId = tenantId, Status = "Draft", CreatedDate = DateTime.UtcNow }
        };
        context.Policies.AddRange(policies);

        // Seed Evidences
        var evidences = new List<Evidence>
        {
            new() { Id = Guid.NewGuid(), Title = "Access Log Review", EvidenceNumber = "EV-001", Description = "Monthly access log review", TenantId = tenantId, VerificationStatus = "Verified", CreatedDate = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Penetration Test Report", EvidenceNumber = "EV-002", Description = "Annual penetration test", TenantId = tenantId, VerificationStatus = "Pending", CreatedDate = DateTime.UtcNow }
        };
        context.Evidences.AddRange(evidences);

        // Seed Audits
        var audits = new List<Audit>
        {
            new() { Id = Guid.NewGuid(), Title = "Annual Security Audit", AuditNumber = "AUD-001", Scope = "Full IT infrastructure", TenantId = tenantId, Status = "Completed", CreatedDate = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Compliance Audit", AuditNumber = "AUD-002", Scope = "Regulatory compliance", TenantId = tenantId, Status = "In Progress", CreatedDate = DateTime.UtcNow }
        };
        context.Audits.AddRange(audits);

        // Seed Assessments
        var assessments = new List<Assessment>
        {
            new() { Id = Guid.NewGuid(), Name = "ECC Gap Assessment", Description = "NCA ECC compliance gap assessment", TenantId = tenantId, Status = "Active", CreatedDate = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Risk Assessment Q1", Description = "Quarterly risk assessment", TenantId = tenantId, Status = "Completed", CreatedDate = DateTime.UtcNow }
        };
        context.Assessments.AddRange(assessments);

        // Seed PricingPlans
        var pricingPlans = new List<PricingPlan>
        {
            new() { Id = Guid.NewGuid(), Name = "Starter", NameAr = "مبتدئ", Price = 999, Period = "month", IsActive = true, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "Professional", NameAr = "احترافي", Price = 2999, Period = "month", IsActive = true, IsPopular = true, DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "Enterprise", NameAr = "مؤسسي", Price = 0, Period = "custom", IsActive = true, DisplayOrder = 3 }
        };
        context.PricingPlans.AddRange(pricingPlans);

        // Seed FAQs
        var faqs = new List<Faq>
        {
            new() { Id = Guid.NewGuid(), Question = "What is Shahin?", QuestionAr = "ما هي شاهين؟", Answer = "Shahin is a GRC platform", AnswerAr = "شاهين هي منصة GRC", Category = "General", IsActive = true, DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), Question = "How to start trial?", QuestionAr = "كيف أبدأ التجربة؟", Answer = "Click free trial button", AnswerAr = "انقر على زر التجربة المجانية", Category = "Getting Started", IsActive = true, DisplayOrder = 2 }
        };
        context.Faqs.AddRange(faqs);

        context.SaveChanges();
    }

    /// <summary>
    /// Gets the test tenant ID from seeded data
    /// </summary>
    public static Guid GetTestTenantId(GrcDbContext context)
    {
        return context.Tenants.First().Id;
    }
}

/// <summary>
/// Test data builder for creating specific test scenarios
/// </summary>
public class TestDataBuilder
{
    private readonly GrcDbContext _context;
    private readonly Guid _tenantId;

    public TestDataBuilder(GrcDbContext context)
    {
        _context = context;
        _tenantId = context.Tenants.FirstOrDefault()?.Id ?? Guid.NewGuid();
    }

    public TestDataBuilder WithRisk(string name, string status = "Open", int impact = 3, int likelihood = 3)
    {
        _context.Risks.Add(new Risk
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Description for {name}",
            TenantId = _tenantId,
            Status = status,
            Impact = impact,
            Likelihood = likelihood,
            CreatedDate = DateTime.UtcNow
        });
        return this;
    }

    public TestDataBuilder WithControl(string name, string controlId, string status = "Planned")
    {
        _context.Controls.Add(new Control
        {
            Id = Guid.NewGuid(),
            ControlId = controlId,
            Name = name,
            Description = $"Description for {name}",
            TenantId = _tenantId,
            Status = status,
            CreatedDate = DateTime.UtcNow
        });
        return this;
    }

    public TestDataBuilder WithTrialSignup(string email, string fullName, string company)
    {
        var nameParts = fullName.Split(' ', 2);
        _context.TrialSignups.Add(new TrialSignup
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = nameParts[0],
            LastName = nameParts.Length > 1 ? nameParts[1] : "",
            CompanyName = company,
            Status = "pending",
            Source = "website",
            CreatedDate = DateTime.UtcNow
        });
        return this;
    }

    public TestDataBuilder Build()
    {
        _context.SaveChanges();
        return this;
    }
}
