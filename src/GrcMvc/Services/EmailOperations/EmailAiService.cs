using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using GrcMvc.Models.Entities.EmailOperations;
using GrcMvc.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.EmailOperations;

/// <summary>
/// AI-powered email classification and reply generation
/// </summary>
public interface IEmailAiService
{
    /// <summary>
    /// Classify an email based on subject and body
    /// </summary>
    Task<EmailClassificationResult> ClassifyEmailAsync(string subject, string body, string? brand = null);
    
    /// <summary>
    /// Generate a reply using AI
    /// </summary>
    Task<string> GenerateReplyAsync(EmailReplyContext context);
    
    /// <summary>
    /// Extract entities from email (customer name, order number, etc.)
    /// </summary>
    Task<Dictionary<string, object>> ExtractEntitiesAsync(string subject, string body);
    
    /// <summary>
    /// Suggest follow-up actions
    /// </summary>
    Task<List<SuggestedAction>> SuggestActionsAsync(string subject, string body, EmailClassification classification);
}

public class EmailClassificationResult
{
    public EmailClassification Classification { get; set; }
    public int Confidence { get; set; } // 0-100
    public EmailPriority SuggestedPriority { get; set; }
    public bool RequiresHumanReview { get; set; }
    public string? Reasoning { get; set; }
}

public class EmailReplyContext
{
    public string Brand { get; set; } = EmailBrands.Shahin;
    public string Language { get; set; } = "ar";
    public string OriginalSubject { get; set; } = string.Empty;
    public string OriginalBody { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public EmailClassification Classification { get; set; }
    public string? TemplateName { get; set; }
    public string? AdditionalInstructions { get; set; }
    public Dictionary<string, object>? ExtractedData { get; set; }
    public List<string>? PreviousReplies { get; set; }
}

public class SuggestedAction
{
    public EmailTaskType TaskType { get; set; }
    public string Description { get; set; } = string.Empty;
    public EmailPriority Priority { get; set; }
    public int? DueInHours { get; set; }
}

/// <summary>
/// Implementation using Claude AI
/// </summary>
public class EmailAiService : IEmailAiService
{
    private readonly IClaudeAgentService? _claudeService;
    private readonly ILogger<EmailAiService> _logger;

    public EmailAiService(
        IClaudeAgentService? claudeService,
        ILogger<EmailAiService> logger)
    {
        _claudeService = claudeService;
        _logger = logger;
    }

    public async Task<EmailClassificationResult> ClassifyEmailAsync(string subject, string body, string? brand = null)
    {
        // Check if Claude service is available
        if (_claudeService == null)
        {
            _logger.LogWarning("Claude AI service not available, using fallback classification");
            return FallbackClassification(subject, body);
        }

        var prompt = $@"أنت مساعد ذكي لتصنيف رسائل البريد الإلكتروني لشركة {brand ?? "شاهين"}.

صنّف الرسالة التالية إلى إحدى الفئات:
- TechnicalSupport: دعم فني، مشاكل تقنية
- BillingInquiry: استفسار عن الفواتير أو الدفع
- AccountIssue: مشكلة في الحساب
- FeatureRequest: طلب ميزة جديدة
- BugReport: بلاغ عن خطأ
- QuoteRequest: طلب عرض سعر
- DemoRequest: طلب عرض تجريبي
- PricingInquiry: استفسار عن الأسعار
- PartnershipInquiry: استفسار عن الشراكة
- ContractQuestion: سؤال عن العقود
- ComplianceQuery: استفسار عن الامتثال
- DocumentRequest: طلب مستندات
- JobApplication: طلب توظيف
- Complaint: شكوى (تحتاج مراجعة بشرية)
- Legal: قانوني (تحتاج مراجعة بشرية)
- AutoReply: رد آلي
- Spam: بريد غير مرغوب

الموضوع: {subject}

المحتوى:
{body}

أجب بـ JSON فقط بهذا الشكل:
{{
  ""classification"": ""اسم_التصنيف"",
  ""confidence"": 85,
  ""priority"": ""Normal"",
  ""requiresHumanReview"": false,
  ""reasoning"": ""السبب باختصار""
}}";

        try
        {
            var chatResponse = await _claudeService.ChatAsync(prompt, null, "email_classification");
            var response = chatResponse.Response ?? "";
            
            // Parse JSON response
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var classificationStr = root.GetProperty("classification").GetString() ?? "Unclassified";
                var classification = Enum.TryParse<EmailClassification>(classificationStr, true, out var c) 
                    ? c : EmailClassification.Unclassified;

                var priorityStr = root.TryGetProperty("priority", out var p) ? p.GetString() ?? "Normal" : "Normal";
                var priority = Enum.TryParse<EmailPriority>(priorityStr, true, out var pr) 
                    ? pr : EmailPriority.Normal;

                return new EmailClassificationResult
                {
                    Classification = classification,
                    Confidence = root.TryGetProperty("confidence", out var conf) ? conf.GetInt32() : 50,
                    SuggestedPriority = priority,
                    RequiresHumanReview = root.TryGetProperty("requiresHumanReview", out var hr) && hr.GetBoolean(),
                    Reasoning = root.TryGetProperty("reasoning", out var r) ? r.GetString() : null
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to classify email");
        }

        return new EmailClassificationResult
        {
            Classification = EmailClassification.Unclassified,
            Confidence = 0,
            SuggestedPriority = EmailPriority.Normal,
            RequiresHumanReview = true,
            Reasoning = "فشل التصنيف التلقائي"
        };
    }

    public async Task<string> GenerateReplyAsync(EmailReplyContext context)
    {
        // Check if Claude service is available
        if (_claudeService == null)
        {
            _logger.LogWarning("Claude AI service not available, using fallback reply");
            return FallbackReply(context);
        }

        var brandInfo = context.Brand switch
        {
            EmailBrands.DoganConsult => "Dogan Consult - مكتب استشارات متخصص في الحوكمة والمخاطر والامتثال",
            _ => "شاهين - منصة الحوكمة والمخاطر والامتثال الذكية"
        };

        var prompt = $@"أنت مساعد خدمة عملاء لـ {brandInfo}.

اكتب رداً احترافياً على الرسالة التالية باللغة {(context.Language == "ar" ? "العربية" : "الإنجليزية")}.

الرسالة الأصلية:
الموضوع: {context.OriginalSubject}
المرسل: {context.SenderName}
المحتوى:
{context.OriginalBody}

التصنيف: {context.Classification}

{(context.AdditionalInstructions != null ? $"تعليمات إضافية: {context.AdditionalInstructions}" : "")}

قواعد الرد:
1. كن ودوداً ومحترفاً
2. اعترف بمشكلة/استفسار العميل
3. قدم معلومات مفيدة أو الخطوات التالية
4. اختم بعرض المساعدة الإضافية
5. لا تعد بما لا يمكنك الوفاء به
6. إذا كان الأمر يحتاج تصعيد، وضّح ذلك

اكتب الرد فقط (بدون تحية أو توقيع - سيضاف تلقائياً):";

        try
        {
            var chatResponse = await _claudeService.ChatAsync(prompt, null, "email_reply_generation");
            return chatResponse.Response?.Trim() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate reply");
            return context.Language == "ar" 
                ? "شكراً لتواصلك معنا. سيقوم فريقنا بالرد عليك في أقرب وقت."
                : "Thank you for contacting us. Our team will respond to you shortly.";
        }
    }

    public async Task<Dictionary<string, object>> ExtractEntitiesAsync(string subject, string body)
    {
        // Check if Claude service is available
        if (_claudeService == null)
        {
            _logger.LogWarning("Claude AI service not available, skipping entity extraction");
            return new Dictionary<string, object>();
        }

        var prompt = $@"استخرج المعلومات المهمة من الرسالة التالية:

الموضوع: {subject}
المحتوى:
{body}

أجب بـ JSON يحتوي على المعلومات المستخرجة:
- customerName: اسم العميل إن وجد
- companyName: اسم الشركة إن وجد
- phoneNumber: رقم الهاتف إن وجد
- orderNumber: رقم الطلب إن وجد
- ticketNumber: رقم التذكرة إن وجد
- urgency: مستوى الاستعجال (low/medium/high)
- sentiment: المشاعر (positive/neutral/negative)
- keyTopics: قائمة بالمواضيع الرئيسية

أجب بـ JSON فقط:";

        try
        {
            var chatResponse = await _claudeService.ChatAsync(prompt, null, "email_entity_extraction");
            var response = chatResponse.Response ?? "";
            
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart);
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract entities");
        }

        return new Dictionary<string, object>();
    }

    public async Task<List<SuggestedAction>> SuggestActionsAsync(string subject, string body, EmailClassification classification)
    {
        var actions = new List<SuggestedAction>();

        // Rule-based suggestions based on classification
        switch (classification)
        {
            case EmailClassification.TechnicalSupport:
            case EmailClassification.BugReport:
                actions.Add(new SuggestedAction
                {
                    TaskType = EmailTaskType.FollowUp,
                    Description = "متابعة الحل التقني",
                    Priority = EmailPriority.High,
                    DueInHours = 24
                });
                break;

            case EmailClassification.QuoteRequest:
            case EmailClassification.DemoRequest:
                actions.Add(new SuggestedAction
                {
                    TaskType = EmailTaskType.ScheduleMeeting,
                    Description = "جدولة عرض تجريبي",
                    Priority = EmailPriority.High,
                    DueInHours = 4
                });
                break;

            case EmailClassification.Complaint:
                actions.Add(new SuggestedAction
                {
                    TaskType = EmailTaskType.Escalate,
                    Description = "تصعيد الشكوى للإدارة",
                    Priority = EmailPriority.Urgent,
                    DueInHours = 2
                });
                break;

            case EmailClassification.BillingInquiry:
                actions.Add(new SuggestedAction
                {
                    TaskType = EmailTaskType.ReviewDraft,
                    Description = "مراجعة الفاتورة والرد",
                    Priority = EmailPriority.Normal,
                    DueInHours = 24
                });
                break;

            default:
                actions.Add(new SuggestedAction
                {
                    TaskType = EmailTaskType.SendReply,
                    Description = "الرد على الاستفسار",
                    Priority = EmailPriority.Normal,
                    DueInHours = 48
                });
                break;
        }

        return actions;
    }

    // Fallback methods when Claude AI is not available
    private EmailClassificationResult FallbackClassification(string subject, string body)
    {
        var classification = EmailClassification.Unclassified;
        var priority = EmailPriority.Normal;

        // Simple keyword-based classification
        var text = (subject + " " + body).ToLower();
        
        if (text.Contains("bug") || text.Contains("error") || text.Contains("خطأ") || text.Contains("مشكلة"))
        {
            classification = EmailClassification.BugReport;
            priority = EmailPriority.High;
        }
        else if (text.Contains("price") || text.Contains("quote") || text.Contains("سعر") || text.Contains("عرض"))
        {
            classification = EmailClassification.QuoteRequest;
            priority = EmailPriority.High;
        }
        else if (text.Contains("demo") || text.Contains("تجريبي") || text.Contains("عرض توضيحي"))
        {
            classification = EmailClassification.DemoRequest;
            priority = EmailPriority.High;
        }
        else if (text.Contains("support") || text.Contains("help") || text.Contains("دعم") || text.Contains("مساعدة"))
        {
            classification = EmailClassification.TechnicalSupport;
            priority = EmailPriority.Normal;
        }
        else if (text.Contains("bill") || text.Contains("invoice") || text.Contains("payment") || text.Contains("فاتورة") || text.Contains("دفع"))
        {
            classification = EmailClassification.BillingInquiry;
            priority = EmailPriority.Normal;
        }
        else if (text.Contains("complaint") || text.Contains("شكوى") || text.Contains("اعتراض"))
        {
            classification = EmailClassification.Complaint;
            priority = EmailPriority.Urgent;
        }

        return new EmailClassificationResult
        {
            Classification = classification,
            Confidence = 60,
            SuggestedPriority = priority,
            RequiresHumanReview = classification == EmailClassification.Unclassified || classification == EmailClassification.Complaint,
            Reasoning = "تصنيف تلقائي بسيط (Claude AI غير متوفر)"
        };
    }

    private string FallbackReply(EmailReplyContext context)
    {
        var isArabic = context.Language == "ar";
        
        return context.Classification switch
        {
            EmailClassification.TechnicalSupport => isArabic
                ? "شكراً لتواصلك معنا. تم استلام طلب الدعم الفني الخاص بك وسيقوم فريقنا المتخصص بالرد عليك في أقرب وقت ممكن."
                : "Thank you for contacting us. Your technical support request has been received and our specialized team will respond to you as soon as possible.",
            
            EmailClassification.BillingInquiry => isArabic
                ? "شكراً لاستفسارك. تم استلام طلبك المتعلق بالفواتير وسيقوم قسم المحاسبة بالرد عليك خلال 24 ساعة."
                : "Thank you for your inquiry. Your billing request has been received and our accounting department will respond within 24 hours.",
            
            EmailClassification.QuoteRequest => isArabic
                ? "شكراً لاهتمامك بخدماتنا. سيقوم فريق المبيعات بالتواصل معك قريباً لتقديم عرض سعر مفصل."
                : "Thank you for your interest in our services. Our sales team will contact you soon with a detailed quote.",
            
            EmailClassification.DemoRequest => isArabic
                ? "شكراً لطلبك عرضاً توضيحياً. سيتواصل معك أحد ممثلينا لجدولة موعد مناسب."
                : "Thank you for requesting a demo. One of our representatives will contact you to schedule a convenient time.",
            
            EmailClassification.Complaint => isArabic
                ? "نعتذر عن أي إزعاج. تم تصعيد شكواك إلى الإدارة وسنتواصل معك في أقرب وقت لحل المشكلة."
                : "We apologize for any inconvenience. Your complaint has been escalated to management and we will contact you soon to resolve the issue.",
            
            _ => isArabic
                ? "شكراً لتواصلك معنا. تم استلام رسالتك وسيقوم فريقنا بالرد عليك قريباً."
                : "Thank you for contacting us. Your message has been received and our team will respond to you soon."
        };
    }
}
