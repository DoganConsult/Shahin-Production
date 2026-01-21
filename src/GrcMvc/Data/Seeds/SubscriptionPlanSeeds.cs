using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrcMvc.Data.Seeds
{
    /// <summary>
    /// Seeds subscription plans for the GRC platform.
    /// MVP, Professional, Enterprise tiers.
    /// </summary>
    public static class SubscriptionPlanSeeds
    {
        public static async Task SeedAsync(GrcDbContext context)
        {
            // Check if already seeded
            if (await context.SubscriptionPlans.AnyAsync())
            {
                return;
            }

            var plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "MVP",
                    Code = "MVP",
                    Description = "الخطة الأساسية للشركات الصغيرة | Basic plan for small businesses",
                    MonthlyPrice = 999,
                    AnnualPrice = 9990, // 2 months free
                    MaxUsers = 5,
                    MaxAssessments = 10,
                    MaxPolicies = 20,
                    HasAdvancedReporting = false,
                    HasApiAccess = false,
                    HasPrioritySupport = false,
                    Features = JsonSerializer.Serialize(new List<string>
                    {
                        "5 مستخدمين | 5 Users",
                        "10 تقييمات شهرياً | 10 Assessments/month",
                        "20 سياسة | 20 Policies",
                        "إطار تنظيمي واحد | 1 Framework",
                        "دعم بالبريد الإلكتروني | Email Support",
                        "تقارير أساسية | Basic Reports",
                        "نسخ احتياطي يومي | Daily Backup"
                    }),
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTime.UtcNow
                },
                new SubscriptionPlan
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Professional",
                    Code = "PRO",
                    Description = "للمؤسسات المتوسطة مع احتياجات امتثال متعددة | For mid-size organizations with multiple compliance needs",
                    MonthlyPrice = 2999,
                    AnnualPrice = 29990, // 2 months free
                    MaxUsers = 25,
                    MaxAssessments = 50,
                    MaxPolicies = 100,
                    HasAdvancedReporting = true,
                    HasApiAccess = true,
                    HasPrioritySupport = false,
                    Features = JsonSerializer.Serialize(new List<string>
                    {
                        "25 مستخدم | 25 Users",
                        "50 تقييم شهرياً | 50 Assessments/month",
                        "100 سياسة | 100 Policies",
                        "5 أطر تنظيمية | 5 Frameworks",
                        "تقارير متقدمة | Advanced Reports",
                        "وصول API | API Access",
                        "دعم بالهاتف | Phone Support",
                        "تكامل مع SSO | SSO Integration",
                        "نسخ احتياطي كل ساعة | Hourly Backup"
                    }),
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedDate = DateTime.UtcNow
                },
                new SubscriptionPlan
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Enterprise",
                    Code = "ENT",
                    Description = "للمؤسسات الكبيرة مع متطلبات امتثال معقدة | For large enterprises with complex compliance requirements",
                    MonthlyPrice = 9999,
                    AnnualPrice = 99990, // 2 months free
                    MaxUsers = 999,
                    MaxAssessments = 999,
                    MaxPolicies = 999,
                    HasAdvancedReporting = true,
                    HasApiAccess = true,
                    HasPrioritySupport = true,
                    Features = JsonSerializer.Serialize(new List<string>
                    {
                        "مستخدمون غير محدودين | Unlimited Users",
                        "تقييمات غير محدودة | Unlimited Assessments",
                        "سياسات غير محدودة | Unlimited Policies",
                        "جميع الأطر التنظيمية | All Frameworks",
                        "ذكاء اصطناعي متقدم | Advanced AI",
                        "لوحة تحكم مخصصة | Custom Dashboard",
                        "تكامل ERP | ERP Integration",
                        "دعم أولوية 24/7 | 24/7 Priority Support",
                        "مدير حساب مخصص | Dedicated Account Manager",
                        "تدريب في الموقع | On-site Training",
                        "SLA مخصص | Custom SLA"
                    }),
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedDate = DateTime.UtcNow
                }
            };

            await context.SubscriptionPlans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
        }
    }
}
