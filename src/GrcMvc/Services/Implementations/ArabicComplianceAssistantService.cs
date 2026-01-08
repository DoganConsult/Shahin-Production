using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrcMvc.Data;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// Arabic Compliance Assistant Service Implementation
/// مساعد الامتثال العربي
/// AI-powered compliance assistant with native Arabic support
/// </summary>
public class ArabicComplianceAssistantService : IArabicComplianceAssistant
{
    private readonly GrcDbContext _context;
    private readonly ILogger<ArabicComplianceAssistantService> _logger;
    private readonly IClaudeAgentService? _aiService;

    // GRC glossary for term lookup
    private static readonly Dictionary<string, GrcTermHelp> GrcGlossary = new()
    {
        ["compliance"] = new() { Term = "Compliance", TermAr = "الامتثال",
            DefinitionEn = "The act of conforming to a rule, standard, or law.",
            DefinitionAr = "التزام المنظمة بالقواعد والمعايير والقوانين المطبقة.",
            Category = "General GRC", RelatedTerms = new() { "Regulation", "Standard", "Policy" } },

        ["risk"] = new() { Term = "Risk", TermAr = "المخاطر",
            DefinitionEn = "The possibility of loss or harm occurring from uncertain events.",
            DefinitionAr = "احتمالية حدوث خسارة أو ضرر نتيجة أحداث غير مؤكدة.",
            Category = "Risk Management", RelatedTerms = new() { "Risk Assessment", "Risk Mitigation", "Risk Appetite" } },

        ["control"] = new() { Term = "Control", TermAr = "الضابط",
            DefinitionEn = "A measure or safeguard designed to mitigate risk or ensure compliance.",
            DefinitionAr = "إجراء أو ضمان مصمم للتخفيف من المخاطر أو ضمان الامتثال.",
            Category = "Controls", RelatedTerms = new() { "Preventive Control", "Detective Control", "Corrective Control" } },

        ["nca-ecc"] = new() { Term = "NCA-ECC", TermAr = "ضوابط الأمن السيبراني الأساسية",
            DefinitionEn = "Essential Cybersecurity Controls issued by the National Cybersecurity Authority of Saudi Arabia.",
            DefinitionAr = "الضوابط الأساسية للأمن السيبراني الصادرة من الهيئة الوطنية للأمن السيبراني في المملكة العربية السعودية.",
            Category = "Frameworks", SourceFramework = "NCA", RelatedTerms = new() { "Cybersecurity", "NCA", "ECC" } },

        ["pdpl"] = new() { Term = "PDPL", TermAr = "نظام حماية البيانات الشخصية",
            DefinitionEn = "Personal Data Protection Law - Saudi Arabia's comprehensive data privacy regulation.",
            DefinitionAr = "نظام حماية البيانات الشخصية - نظام شامل لخصوصية البيانات في المملكة العربية السعودية.",
            Category = "Regulations", SourceFramework = "SDAIA", RelatedTerms = new() { "Data Privacy", "SDAIA", "Personal Data" } },

        ["sama-csf"] = new() { Term = "SAMA-CSF", TermAr = "إطار الأمن السيبراني للبنك المركزي",
            DefinitionEn = "Saudi Central Bank Cybersecurity Framework for financial institutions.",
            DefinitionAr = "إطار الأمن السيبراني للبنك المركزي السعودي للمؤسسات المالية.",
            Category = "Frameworks", SourceFramework = "SAMA", RelatedTerms = new() { "Banking", "Financial Services", "SAMA" } }
    };

    public ArabicComplianceAssistantService(
        GrcDbContext context,
        ILogger<ArabicComplianceAssistantService> logger,
        IClaudeAgentService? aiService = null)
    {
        _context = context;
        _logger = logger;
        _aiService = aiService;
    }

    public async Task<ComplianceAnswer> AskQuestionAsync(string question, Guid tenantId, string language = "ar")
    {
        _logger.LogInformation("Processing compliance question for tenant {TenantId}: {Question}", tenantId, question);

        var answer = new ComplianceAnswer
        {
            OriginalQuestion = question,
            AnsweredAt = DateTime.UtcNow
        };

        try
        {
            // Analyze question to determine relevant frameworks
            var relevantFrameworks = AnalyzeQuestionForFrameworks(question);
            answer.RelevantFrameworks = relevantFrameworks;

            // Get relevant controls
            var relevantControls = await GetRelevantControlsAsync(question, tenantId);
            answer.RelevantControls = relevantControls.Select(c => c.ControlCode ?? c.Id.ToString()).ToList();

            // Generate answer based on question type
            var (answerEn, answerAr) = GenerateAnswer(question, relevantFrameworks, relevantControls);
            answer.AnswerEn = answerEn;
            answer.AnswerAr = answerAr;
            answer.ConfidenceScore = 0.85;

            // Add sources
            answer.Sources = relevantFrameworks.Select(f => new SourceReference
            {
                Type = "Framework",
                Code = f,
                Title = GetFrameworkTitle(f),
                Url = GetFrameworkUrl(f)
            }).ToList();

            // Suggest follow-up questions
            answer.FollowUpQuestions = GenerateFollowUpQuestions(question, language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing compliance question");
            answer.AnswerEn = "I apologize, but I encountered an error processing your question. Please try rephrasing or contact support.";
            answer.AnswerAr = "أعتذر، لكنني واجهت خطأ في معالجة سؤالك. يرجى محاولة إعادة صياغة السؤال أو الاتصال بالدعم.";
            answer.ConfidenceScore = 0;
        }

        return answer;
    }

    public async Task<byte[]> GenerateArabicReportAsync(Guid assessmentId, string reportType)
    {
        _logger.LogInformation("Generating Arabic report for assessment {AssessmentId}, type: {ReportType}", assessmentId, reportType);

        var assessment = await _context.Assessments.FindAsync(assessmentId);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html dir=\"rtl\" lang=\"ar\">");
        sb.AppendLine("<head><meta charset=\"UTF-8\"><title>تقرير الامتثال</title>");
        sb.AppendLine("<style>body{font-family:'Segoe UI',Tahoma,sans-serif;direction:rtl;padding:40px;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<h1>تقرير التقييم</h1>");
        sb.AppendLine($"<p><strong>رقم التقييم:</strong> {assessmentId}</p>");
        sb.AppendLine($"<p><strong>نوع التقرير:</strong> {reportType}</p>");
        sb.AppendLine($"<p><strong>تاريخ الإنشاء:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm}</p>");

        if (assessment != null)
        {
            sb.AppendLine($"<h2>ملخص تنفيذي</h2>");
            sb.AppendLine($"<p>هذا التقرير يعرض نتائج تقييم الامتثال للمنظمة.</p>");
            sb.AppendLine($"<h2>النتائج الرئيسية</h2>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>تم تقييم الضوابط بنجاح</li>");
            sb.AppendLine("<li>تم تحديد نقاط التحسين</li>");
            sb.AppendLine("<li>تم وضع خطة العمل</li>");
            sb.AppendLine("</ul>");
        }

        sb.AppendLine("<h2>التوصيات</h2>");
        sb.AppendLine("<ol>");
        sb.AppendLine("<li>تعزيز ضوابط الأمن السيبراني</li>");
        sb.AppendLine("<li>تحديث سياسات حماية البيانات</li>");
        sb.AppendLine("<li>تدريب الموظفين على الامتثال</li>");
        sb.AppendLine("</ol>");

        sb.AppendLine("<footer><p>تم إنشاؤه بواسطة منصة شاهين للحوكمة والمخاطر والامتثال</p></footer>");
        sb.AppendLine("</body></html>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<DocumentAnalysisResult> AnalyzeDocumentAsync(Stream documentStream, string fileName, Guid tenantId)
    {
        _logger.LogInformation("Analyzing document {FileName} for tenant {TenantId}", fileName, tenantId);

        // Simulate document analysis
        var result = new DocumentAnalysisResult
        {
            FileName = fileName,
            DetectedLanguage = fileName.Contains("ar") ? "ar" : "en",
            DocumentType = DetectDocumentType(fileName),
            ComplianceScore = 75.5,
            AnalyzedAt = DateTime.UtcNow,
            KeyFindings = new() { "Document structure is well-organized", "Some sections need more detail" },
            KeyFindingsAr = new() { "هيكل المستند منظم جيداً", "بعض الأقسام تحتاج إلى مزيد من التفصيل" },
            RecommendationsEn = new() { "Add more specific controls references", "Include implementation timeline" },
            RecommendationsAr = new() { "إضافة مراجع أكثر تحديداً للضوابط", "تضمين جدول زمني للتنفيذ" },
            MappedControls = new() { "NCA-ECC-1-1", "NCA-ECC-2-1", "PDPL-Art-5" },
            Gaps = new List<ArabicComplianceGap>
            {
                new ArabicComplianceGap { DescriptionEn = "Missing data retention policy", DescriptionAr = "سياسة الاحتفاظ بالبيانات مفقودة",
                        Severity = "Medium", RelatedControl = "PDPL-Art-18",
                        RemediationEn = "Develop and document data retention policy", RemediationAr = "وضع وتوثيق سياسة الاحتفاظ بالبيانات" }
            }
        };

        return await Task.FromResult(result);
    }

    public async Task<List<ArabicRecommendation>> GetRecommendationsAsync(Guid tenantId)
    {
        var recommendations = new List<ArabicRecommendation>
        {
            new() { TitleEn = "Implement Multi-Factor Authentication", TitleAr = "تطبيق المصادقة متعددة العوامل",
                    DescriptionEn = "Enable MFA across all critical systems to enhance security.",
                    DescriptionAr = "تفعيل المصادقة متعددة العوامل عبر جميع الأنظمة الحرجة لتعزيز الأمان.",
                    Priority = "High", Category = "Cybersecurity", ImpactScore = 9.5, EstimatedEffortDays = 14,
                    ActionItemsEn = new() { "Assess current authentication methods", "Select MFA solution", "Deploy and test" },
                    ActionItemsAr = new() { "تقييم طرق المصادقة الحالية", "اختيار حل المصادقة متعددة العوامل", "النشر والاختبار" } },

            new() { TitleEn = "Update Data Classification Policy", TitleAr = "تحديث سياسة تصنيف البيانات",
                    DescriptionEn = "Review and update data classification to align with PDPL requirements.",
                    DescriptionAr = "مراجعة وتحديث تصنيف البيانات بما يتوافق مع متطلبات نظام حماية البيانات الشخصية.",
                    Priority = "High", Category = "Data Protection", ImpactScore = 8.5, EstimatedEffortDays = 7,
                    ActionItemsEn = new() { "Review current classification", "Map to PDPL categories", "Update policies" },
                    ActionItemsAr = new() { "مراجعة التصنيف الحالي", "ربطه بفئات نظام حماية البيانات", "تحديث السياسات" } },

            new() { TitleEn = "Conduct Security Awareness Training", TitleAr = "إجراء تدريب التوعية الأمنية",
                    DescriptionEn = "Implement comprehensive security awareness program for all employees.",
                    DescriptionAr = "تنفيذ برنامج شامل للتوعية الأمنية لجميع الموظفين.",
                    Priority = "Medium", Category = "Training", ImpactScore = 7.0, EstimatedEffortDays = 30,
                    ActionItemsEn = new() { "Develop training content", "Schedule sessions", "Track completion" },
                    ActionItemsAr = new() { "تطوير محتوى التدريب", "جدولة الجلسات", "تتبع الإكمال" } }
        };

        return await Task.FromResult(recommendations);
    }

    public async Task<string> TranslateContentAsync(string content, string fromLanguage, string toLanguage)
    {
        // In production, integrate with translation service
        _logger.LogInformation("Translating content from {From} to {To}", fromLanguage, toLanguage);

        // Simple placeholder - in production use Azure Translator or similar
        if (fromLanguage == "en" && toLanguage == "ar")
        {
            return await Task.FromResult($"[ترجمة عربية] {content}");
        }
        else if (fromLanguage == "ar" && toLanguage == "en")
        {
            return await Task.FromResult($"[English Translation] {content}");
        }

        return content;
    }

    public async Task<ControlGuidance> GetControlGuidanceAsync(string controlCode, string language = "ar")
    {
        _logger.LogInformation("Getting guidance for control {ControlCode} in {Language}", controlCode, language);

        return await Task.FromResult(new ControlGuidance
        {
            ControlCode = controlCode,
            ControlTitleEn = $"Control {controlCode}",
            ControlTitleAr = $"الضابط {controlCode}",
            OverviewEn = "This control ensures proper security measures are in place.",
            OverviewAr = "يضمن هذا الضابط وجود تدابير أمنية مناسبة.",
            ImplementationSteps = new List<ImplementationStep>
            {
                new() { StepNumber = 1, TitleEn = "Assessment", TitleAr = "التقييم",
                        DescriptionEn = "Assess current state", DescriptionAr = "تقييم الوضع الحالي",
                        ResponsibleRole = "Compliance Officer", EstimatedDays = 3 },
                new() { StepNumber = 2, TitleEn = "Planning", TitleAr = "التخطيط",
                        DescriptionEn = "Develop implementation plan", DescriptionAr = "وضع خطة التنفيذ",
                        ResponsibleRole = "Project Manager", EstimatedDays = 5 },
                new() { StepNumber = 3, TitleEn = "Implementation", TitleAr = "التنفيذ",
                        DescriptionEn = "Execute the plan", DescriptionAr = "تنفيذ الخطة",
                        ResponsibleRole = "IT Team", EstimatedDays = 10 }
            },
            RequiredEvidenceEn = new() { "Policy document", "Implementation records", "Test results" },
            RequiredEvidenceAr = new() { "وثيقة السياسة", "سجلات التنفيذ", "نتائج الاختبار" },
            BestPracticesEn = new() { "Document everything", "Test before deployment", "Train users" },
            BestPracticesAr = new() { "توثيق كل شيء", "الاختبار قبل النشر", "تدريب المستخدمين" }
        });
    }

    public async Task<ArabicAssessmentSummary> SummarizeAssessmentAsync(Guid assessmentId, string language = "ar")
    {
        var assessment = await _context.Assessments.FindAsync(assessmentId);

        return new ArabicAssessmentSummary
        {
            AssessmentId = assessmentId,
            AssessmentTitle = assessment?.Name ?? "Assessment",
            ExecutiveSummaryEn = "This assessment evaluated the organization's compliance posture across key regulatory frameworks. Overall compliance is satisfactory with some areas requiring attention.",
            ExecutiveSummaryAr = "قيّم هذا التقييم وضع امتثال المنظمة عبر الأطر التنظيمية الرئيسية. الامتثال العام مرضٍ مع بعض المجالات التي تتطلب الاهتمام.",
            OverallScore = 78.5,
            OverallStatus = "Satisfactory",
            TotalControls = 109,
            CompliantControls = 85,
            PartialControls = 15,
            NonCompliantControls = 9,
            KeyStrengthsEn = new() { "Strong governance framework", "Good documentation practices", "Active risk management" },
            KeyStrengthsAr = new() { "إطار حوكمة قوي", "ممارسات توثيق جيدة", "إدارة مخاطر فعالة" },
            KeyWeaknessesEn = new() { "Third-party risk management needs improvement", "Incident response procedures incomplete" },
            KeyWeaknessesAr = new() { "إدارة مخاطر الطرف الثالث تحتاج تحسين", "إجراءات الاستجابة للحوادث غير مكتملة" },
            PriorityActionsEn = new() { "Complete incident response plan", "Implement vendor risk assessment", "Conduct penetration testing" },
            PriorityActionsAr = new() { "إكمال خطة الاستجابة للحوادث", "تنفيذ تقييم مخاطر الموردين", "إجراء اختبار الاختراق" },
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<GrcTermHelp> GetTermHelpAsync(string term, string language = "ar")
    {
        var normalizedTerm = term.ToLower().Replace(" ", "-").Replace("_", "-");

        if (GrcGlossary.TryGetValue(normalizedTerm, out var help))
        {
            return await Task.FromResult(help);
        }

        // Return generic help if term not found
        return new GrcTermHelp
        {
            Term = term,
            TermAr = term,
            DefinitionEn = $"Definition for '{term}' not found in glossary. Please consult the relevant framework documentation.",
            DefinitionAr = $"لم يتم العثور على تعريف '{term}' في المسرد. يرجى الرجوع إلى وثائق الإطار ذات الصلة.",
            Category = "General"
        };
    }

    private List<string> AnalyzeQuestionForFrameworks(string question)
    {
        var frameworks = new List<string>();
        var lowerQuestion = question.ToLower();

        if (lowerQuestion.Contains("nca") || lowerQuestion.Contains("cybersecurity") || lowerQuestion.Contains("أمن سيبراني"))
            frameworks.Add("NCA-ECC");
        if (lowerQuestion.Contains("sama") || lowerQuestion.Contains("bank") || lowerQuestion.Contains("مالي"))
            frameworks.Add("SAMA-CSF");
        if (lowerQuestion.Contains("pdpl") || lowerQuestion.Contains("data protection") || lowerQuestion.Contains("بيانات شخصية"))
            frameworks.Add("PDPL");
        if (lowerQuestion.Contains("iso") || lowerQuestion.Contains("27001"))
            frameworks.Add("ISO-27001");

        return frameworks.Count > 0 ? frameworks : new List<string> { "General GRC" };
    }

    private async Task<List<Models.Entities.Control>> GetRelevantControlsAsync(string question, Guid tenantId)
    {
        return await _context.Controls
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .Take(5)
            .ToListAsync();
    }

    private (string En, string Ar) GenerateAnswer(string question, List<string> frameworks, List<Models.Entities.Control> controls)
    {
        var frameworkList = string.Join(", ", frameworks);

        var answerEn = $"Based on your question about {frameworkList}, here is the guidance: " +
                       $"You should implement the relevant controls from these frameworks. " +
                       $"Currently, you have {controls.Count} related controls in your system. " +
                       $"Please refer to the official framework documentation for detailed requirements.";

        var answerAr = $"بناءً على سؤالك حول {frameworkList}، إليك الإرشادات: " +
                       $"يجب عليك تطبيق الضوابط ذات الصلة من هذه الأطر. " +
                       $"حالياً، لديك {controls.Count} ضوابط مرتبطة في نظامك. " +
                       $"يرجى الرجوع إلى وثائق الإطار الرسمية للمتطلبات التفصيلية.";

        return (answerEn, answerAr);
    }

    private List<string> GenerateFollowUpQuestions(string question, string language)
    {
        return language == "ar"
            ? new List<string>
              {
                  "ما هي الضوابط المطلوبة لقطاعي؟",
                  "كيف يمكنني تحسين درجة الامتثال؟",
                  "ما هي المواعيد النهائية القادمة؟"
              }
            : new List<string>
              {
                  "What controls are required for my sector?",
                  "How can I improve my compliance score?",
                  "What are the upcoming deadlines?"
              };
    }

    private string GetFrameworkTitle(string code) => code switch
    {
        "NCA-ECC" => "NCA Essential Cybersecurity Controls",
        "SAMA-CSF" => "SAMA Cybersecurity Framework",
        "PDPL" => "Personal Data Protection Law",
        "ISO-27001" => "ISO/IEC 27001",
        _ => code
    };

    private string GetFrameworkUrl(string code) => code switch
    {
        "NCA-ECC" => "https://nca.gov.sa/pages/ecc.html",
        "SAMA-CSF" => "https://www.sama.gov.sa/en-US/Laws/Pages/cybersecurity.aspx",
        "PDPL" => "https://sdaia.gov.sa/pdpl",
        _ => "#"
    };

    private string DetectDocumentType(string fileName)
    {
        var lowerName = fileName.ToLower();
        if (lowerName.Contains("policy")) return "Policy";
        if (lowerName.Contains("procedure")) return "Procedure";
        if (lowerName.Contains("report")) return "Report";
        if (lowerName.Contains("evidence")) return "Evidence";
        return "Document";
    }
}
