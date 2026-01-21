using GrcMvc.Data;
using GrcMvc.Exceptions;
using GrcMvc.Models.Entities;
using GrcMvc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// AI-powered support agent service for user assistance
/// </summary>
public class SupportAgentService : ISupportAgentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SupportAgentService> _logger;

    // Knowledge base for common questions
    private static readonly Dictionary<string, string> OnboardingFAQ = new()
    {
        ["tos"] = "Our Terms of Service outline the rules and guidelines for using the GRC platform. By using our services, you agree to comply with data protection regulations, maintain confidentiality of your organization's data, and use the platform in accordance with Saudi Arabian laws and regulations.",
        ["privacy"] = "Our Privacy Policy explains how we collect, use, and protect your personal data in compliance with PDPL (Personal Data Protection Law). We implement strong encryption, access controls, and data minimization practices.",
        ["frameworks"] = "The system supports major KSA regulatory frameworks including NCA ECC (Essential Cybersecurity Controls), SAMA CSF (Cybersecurity Framework), PDPL (Personal Data Protection Law), and more. Based on your organization profile, applicable frameworks are automatically determined.",
        ["onboarding"] = "The onboarding process consists of 4 steps: 1) Registration - Create your organization account, 2) Organization Profile - Answer questions about your business, 3) Review Scope - See applicable compliance frameworks, 4) Create Plan - Generate your GRC compliance plan.",
        ["assessment"] = "Assessments are automatically generated based on your applicable frameworks. Each assessment contains controls from the relevant regulatory framework, and you can assign team members to evaluate and provide evidence for each control.",
        ["roles"] = "The system supports multiple roles: Compliance Officer, Control Owner, Risk Manager, DPO (Data Protection Officer), Security Officer, Auditor, and GRC Manager. Each role has specific permissions and responsibilities.",
        ["evidence"] = "Evidence can be uploaded as documents, screenshots, or automated integrations. Each piece of evidence goes through a review workflow before being accepted as proof of compliance.",
        ["help"] = "I'm here to help! You can ask me about Terms of Service, Privacy Policy, onboarding steps, regulatory frameworks, assessments, roles, or any other feature of the GRC system."
    };

    public SupportAgentService(IUnitOfWork unitOfWork, ILogger<SupportAgentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SupportConversation> StartConversationAsync(
        Guid? tenantId, string? userId, string? sessionId, string? category = null)
    {
        var conversation = new SupportConversation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            SessionId = sessionId ?? Guid.NewGuid().ToString(),
            Category = category ?? "General",
            Status = "Active",
            IsAgentHandled = true,
            StartedAt = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = userId ?? "System"
        };

        await _unitOfWork.SupportConversations.AddAsync(conversation);
        await _unitOfWork.SaveChangesAsync();

        // Add welcome message
        await SendMessageAsync(conversation.Id, GetWelcomeMessage(), "Agent", "AI-Assistant");

        _logger.LogInformation("Started support conversation {ConversationId}", conversation.Id);
        return conversation;
    }

    public async Task<SupportMessage> SendMessageAsync(
        Guid conversationId, string content, string senderType = "User", string? senderId = null)
    {
        var message = new SupportMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Content = content,
            SenderType = senderType,
            SenderId = senderId,
            MessageType = "Text",
            SentAt = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = senderId ?? "System"
        };

        await _unitOfWork.SupportMessages.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        return message;
    }

    public async Task<SupportMessage> GetAgentResponseAsync(Guid conversationId, string userMessage)
    {
        // Analyze user message and generate response
        var response = GenerateAIResponse(userMessage);

        var agentMessage = await SendMessageAsync(conversationId, response, "Agent", "AI-Assistant");

        _logger.LogInformation("Generated agent response for conversation {ConversationId}", conversationId);
        return agentMessage;
    }

    public async Task<IEnumerable<SupportMessage>> GetConversationMessagesAsync(Guid conversationId)
    {
        return await _unitOfWork.SupportMessages
            .Query()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<SupportConversation> EscalateToHumanAsync(Guid conversationId, string reason)
    {
        var conversation = await _unitOfWork.SupportConversations.GetByIdAsync(conversationId);
        if (conversation == null)
            throw new EntityNotFoundException("Conversation", conversationId);

        conversation.Status = "Escalated";
        conversation.IsAgentHandled = false;
        conversation.Priority = "High";
        conversation.ModifiedDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        // Add system message about escalation
        await SendMessageAsync(conversationId,
            $"تم تحويل المحادثة إلى فريق الدعم البشري. سبب التحويل: {reason}. سيتواصل معك أحد ممثلي الدعم قريباً.",
            "System", "System");

        _logger.LogInformation("Escalated conversation {ConversationId} to human agent", conversationId);
        return conversation;
    }

    public async Task<SupportConversation> ResolveConversationAsync(
        Guid conversationId, int? satisfactionRating = null, string? feedback = null)
    {
        var conversation = await _unitOfWork.SupportConversations.GetByIdAsync(conversationId);
        if (conversation == null)
            throw new EntityNotFoundException("Conversation", conversationId);

        conversation.Status = "Resolved";
        conversation.ResolvedAt = DateTime.UtcNow;
        conversation.SatisfactionRating = satisfactionRating;
        conversation.Feedback = feedback;
        conversation.ModifiedDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Resolved conversation {ConversationId}", conversationId);
        return conversation;
    }

    public Task<string> GetQuickHelpAsync(string question, string context = "onboarding")
    {
        var response = GenerateAIResponse(question);
        return Task.FromResult(response);
    }

    #region Private Helper Methods

    private string GetWelcomeMessage()
    {
        return @"مرحباً بك في نظام إدارة الحوكمة والمخاطر والامتثال! 👋

أنا مساعدك الذكي، وأنا هنا لمساعدتك في:
• فهم شروط الخدمة وسياسة الخصوصية
• إكمال عملية التسجيل
• التعرف على الأطر التنظيمية المطبقة
• الإجابة على أي استفسارات

كيف يمكنني مساعدتك اليوم؟

---

Welcome to the GRC Management System! 👋

I'm your AI assistant, here to help you with:
• Understanding Terms of Service and Privacy Policy
• Completing the registration process
• Learning about applicable regulatory frameworks
• Answering any questions

How can I help you today?";
    }

    private string GenerateAIResponse(string userMessage)
    {
        var messageLower = userMessage.ToLower();

        // Check for FAQ matches
        foreach (var faq in OnboardingFAQ)
        {
            if (messageLower.Contains(faq.Key))
            {
                return faq.Value;
            }
        }

        // Terms of Service specific
        if (messageLower.Contains("term") || messageLower.Contains("شروط") || messageLower.Contains("service"))
        {
            return @"📋 **Terms of Service Summary**

Our Terms of Service cover:

1. **Account Responsibilities**: You are responsible for maintaining the security of your account credentials and all activities under your account.

2. **Data Protection**: You must ensure that any data you enter complies with applicable data protection laws, including PDPL.

3. **Acceptable Use**: The platform must be used for legitimate GRC purposes only. Prohibited activities include unauthorized access attempts and data misuse.

4. **Intellectual Property**: All platform content and features are protected by intellectual property laws.

5. **Service Availability**: We strive for 99.9% uptime but may perform scheduled maintenance.

6. **Termination**: Either party may terminate the agreement with 30 days notice.

Do you have any specific questions about our Terms of Service?";
        }

        // Privacy Policy specific
        if (messageLower.Contains("privacy") || messageLower.Contains("خصوصية") || messageLower.Contains("data") || messageLower.Contains("بيانات"))
        {
            return @"🔒 **Privacy Policy Summary**

We are committed to protecting your privacy in compliance with PDPL:

1. **Data Collection**: We collect only necessary data for providing GRC services - organization profile, user accounts, compliance evidence.

2. **Data Processing**: Your data is processed in Saudi Arabia and stored on secure, encrypted servers.

3. **Data Sharing**: We do not sell your data. Third-party sharing is limited to service providers under strict data processing agreements.

4. **Your Rights**: You have the right to access, correct, delete, and port your data as per PDPL requirements.

5. **Security**: We implement encryption, access controls, and regular security audits.

6. **Retention**: Data is retained as long as your account is active, plus 7 years for regulatory compliance.

Would you like more details about any specific aspect?";
        }

        // Onboarding help
        if (messageLower.Contains("onboard") || messageLower.Contains("register") || messageLower.Contains("تسجيل") || messageLower.Contains("sign"))
        {
            return @"📝 **Onboarding Guide**

The registration process is simple and takes about 15 minutes:

**Step 1: Registration**
- Enter your organization name
- Provide administrator email
- Select subscription tier
- Choose your country

**Step 2: Organization Profile**
- Answer questions about your organization type
- Specify your sector (Banking, Healthcare, etc.)
- Indicate data types you handle
- Select hosting model and size

**Step 3: Review Scope**
- System automatically determines applicable frameworks
- Review NCA ECC, SAMA CSF, PDPL requirements
- Understand why each framework applies

**Step 4: Create Plan**
- Generate your compliance roadmap
- Set timelines and milestones
- Assign team members

Need help with any specific step?";
        }

        // Default response
        return @"شكراً لتواصلك!

I understand you're asking about: " + userMessage + @"

Here's what I can help you with:
• **Terms of Service** - Type 'terms' or 'شروط'
• **Privacy Policy** - Type 'privacy' or 'خصوصية'
• **Onboarding Steps** - Type 'onboarding' or 'تسجيل'
• **Frameworks** - Type 'frameworks' or 'أطر'
• **Roles** - Type 'roles' or 'أدوار'
• **Escalate to Human** - Type 'human' or 'بشري'

Or feel free to ask any specific question!";
    }

    #endregion
}
