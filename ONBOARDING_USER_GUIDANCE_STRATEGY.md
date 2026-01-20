# Onboarding User Guidance Strategy

## Problem Statement
**Most organization admins don't know the answers to all 96 onboarding questions.** How do we ensure they're guided properly?

## 🎯 Current Guidance Features

### 1. **Floating AI Assistant** ✅
- **Location**: Available on all onboarding pages
- **Features**:
  - Real-time AI assistance
  - Multiple AI providers (Shahin AI, Claude, Azure Bot, Copilot)
  - Bilingual support (Arabic/English)
  - Context-aware help

**How to Use:**
- Users can click the floating AI assistant icon
- Ask questions like: "What is my organization type?" or "What frameworks apply to me?"
- Get instant AI-powered guidance

### 2. **Placeholders & Examples** ✅
- **Location**: All form fields
- **Features**:
  - Placeholder text with examples
  - Localized help text
  - Format hints

**Example:**
```html
<input placeholder="e.g., Saudi Telecom Company" />
<input placeholder="e.g., 40" />
```

### 3. **"Why We Ask" Tooltips** ✅
- **Location**: Referenced in views (`WhyWeAskTooltips`)
- **Status**: Partially implemented
- **Purpose**: Explain why each question is important

---

## 🚀 Enhanced Guidance Strategy (Recommended)

### **1. Contextual Help Icons** (Add to Every Field)

**Implementation:**
```html
<label>
    Organization Type
    <i class="fas fa-question-circle text-info" 
       data-bs-toggle="tooltip" 
       data-bs-placement="right"
       title="Select the legal structure of your organization. This helps determine applicable regulations.">
    </i>
</label>
```

**Benefits:**
- Quick help without leaving the page
- Explains what the field means
- Provides context

---

### **2. "I Need Help" Options** (Critical Addition)

**For Dropdowns:**
```html
<select required>
    <option value="">-- Select (Required) --</option>
    <option value="LLC">Limited Liability Company</option>
    <option value="PJSC">Public Joint Stock Company</option>
    <!-- ... -->
</select>
<div class="mt-2">
    <button type="button" class="btn btn-outline-primary btn-sm" onclick="getHelpForField('organizationType')">
        <i class="fas fa-question-circle"></i> I'm not sure - Help me determine
    </button>
</div>
```

**For Text Fields:**
```html
<input type="text" required placeholder="Enter value (Required)" />
<div class="mt-2">
    <button type="button" class="btn btn-outline-primary btn-sm" onclick="getHelpForField('fieldName')">
        <i class="fas fa-question-circle"></i> I need help with this
    </button>
</div>
```

**When "I Need Help" is Clicked:**
- **Show AI assistant automatically** - with context about the field
- **Provide guided questions** - step-by-step to determine answer
- **Suggest values based on industry** - intelligent defaults
- **Show examples** - real-world examples
- **Explain why required** - business reason
- **Cannot proceed** until field is completed (no skipping)

---

### **3. Progressive Disclosure with Required Validation**

**Strategy:**
- Show all questions in logical groups
- **All questions are required** - no optional fields
- Group related questions together
- Show progress indicator
- **Block progression** until current section is complete

**Implementation:**
```html
<div class="question-group">
    <h5>Organization Identity <span class="badge bg-danger">All Required</span></h5>
    <div class="progress mb-3">
        <div class="progress-bar" style="width: 60%">3 of 5 questions completed</div>
    </div>
    <!-- All questions required -->
</div>

<!-- Next section disabled until current is complete -->
<div class="question-group" id="next-section" style="opacity: 0.5; pointer-events: none;">
    <h5>Assurance Objectives <span class="badge bg-danger">All Required</span></h5>
    <div class="alert alert-warning">
        <i class="fas fa-lock"></i> Complete previous section to continue
    </div>
</div>
```

**Validation:**
- **Check all fields** in current section before allowing next
- **Show specific missing fields** - highlight what's incomplete
- **Provide help** for each incomplete field
- **Cannot proceed** until all fields completed

---

### **4. Smart Defaults Based on Industry**

**Implementation:**
```javascript
// When user selects industry sector
function onIndustryChange(industry) {
    // Auto-fill likely answers
    if (industry === "Banking") {
        setDefaultFrameworks(["SAMA CSF", "NCA ECC", "Basel III"]);
        setDefaultRegulators(["SAMA", "NCA"]);
        setDefaultControls("Banking Controls");
    }
    // Show confirmation: "We've pre-filled some answers based on your industry. Review and adjust."
}
```

**Benefits:**
- Reduces questions users need to answer
- Provides intelligent defaults
- Users can adjust if needed

---

### **5. Step-by-Step Guidance Messages**

**Add at Top of Each Step:**
```html
<div class="alert alert-info">
    <h6><i class="fas fa-lightbulb"></i> Quick Tips for This Section</h6>
    <ul>
        <li>You can find your organization type in your commercial registration</li>
        <li>If unsure about industry sector, select the closest match</li>
        <li>You can update these answers later in Organization Settings</li>
    </ul>
    <button class="btn btn-sm btn-outline-info" onclick="openAIAssistant()">
        <i class="fas fa-robot"></i> Ask AI Assistant for Help
    </button>
</div>
```

---

### **6. Field-Level AI Suggestions**

**Implementation:**
```javascript
// When user focuses on a field
function onFieldFocus(fieldName) {
    // Show AI suggestion button
    showAISuggestionButton(fieldName);
}

// When user clicks "Get AI Suggestion"
async function getAISuggestion(fieldName, context) {
    const suggestion = await callAIAssistant({
        question: `What should I enter for ${fieldName}?`,
        context: context,
        organizationType: getOrganizationType()
    });
    
    // Show suggestion with "Use This" button
    showSuggestion(fieldName, suggestion);
}
```

**Example:**
- User focuses on "Primary Regulators" field
- AI button appears: "🤖 Get AI Suggestion"
- AI analyzes organization type, industry, location
- Suggests: "Based on your profile, likely regulators: SAMA, NCA"
- User can click "Use This" or adjust

---

### **7. Validation with Helpful Error Messages**

**Instead of:**
```
❌ "This field is required"
```

**Show:**
```
❌ "Organization type is required to determine applicable regulations.
   💡 Tip: Check your commercial registration document or ask your legal team.
   🤖 Need help? Click the AI assistant icon."
```

---

### **8. Required Field Validation with Guidance**

**Implementation:**
```html
<div class="form-group">
    <label>
        Complex Field <span class="text-danger">*</span>
        <i class="fas fa-question-circle text-info" 
           data-bs-toggle="tooltip" 
           title="This field is required. Click 'I need help' if you're unsure.">
        </i>
    </label>
    <input type="text" required />
    <div class="d-flex justify-content-between mt-2">
        <button type="button" class="btn btn-link text-danger" disabled>
            <i class="fas fa-lock"></i> This field is required - cannot skip
        </button>
        <button type="button" class="btn btn-primary" onclick="getHelp('fieldName')">
            <i class="fas fa-question-circle"></i> I need help with this
        </button>
    </div>
    <div class="alert alert-info mt-2" id="help-for-fieldName" style="display: none;">
        <strong>💡 Guidance:</strong> 
        <p>This field is required because it determines [explanation].</p>
        <p>You can find this information by [specific guidance].</p>
        <button class="btn btn-sm btn-primary" onclick="openAIAssistant('fieldName')">
            <i class="fas fa-robot"></i> Ask AI Assistant
        </button>
    </div>
</div>
```

**When Field is Empty:**
- **Block form submission** - show validation error
- **Show guidance immediately** - explain why it's required
- **Provide specific help** - where to find the answer
- **Open AI assistant** - if user clicks "I need help"
- **Cannot proceed** until field is completed

---

### **9. Contextual Examples & Templates**

**Add Example Section:**
```html
<div class="card bg-light mb-3">
    <div class="card-header">
        <h6><i class="fas fa-book"></i> Example</h6>
    </div>
    <div class="card-body">
        <p><strong>For a Saudi Bank:</strong></p>
        <ul>
            <li>Organization Type: <code>Public Joint Stock Company</code></li>
            <li>Primary Regulators: <code>SAMA, NCA</code></li>
            <li>Frameworks: <code>SAMA CSF, NCA ECC, Basel III</code></li>
        </ul>
        <button class="btn btn-sm btn-outline-primary" onclick="useAsTemplate('banking')">
            Use this as a template
        </button>
    </div>
</div>
```

---

### **10. Multi-Person Collaboration**

**Allow Admin to Invite Helpers:**
```html
<div class="alert alert-warning">
    <h6><i class="fas fa-users"></i> Need Help from Your Team?</h6>
    <p>You can invite team members to help complete specific sections:</p>
    <ul>
        <li><strong>Legal Team</strong> → Organization Identity (Section A)</li>
        <li><strong>IT Team</strong> → Technology Stack (Section F)</li>
        <li><strong>Compliance Team</strong> → Regulatory Applicability (Section C)</li>
    </ul>
    <button class="btn btn-primary" onclick="inviteHelpers()">
        <i class="fas fa-user-plus"></i> Invite Team Members to Help
    </button>
</div>
```

---

## 📋 Implementation Priority

### **Phase 1: Critical (Implement First) - REQUIRED**
1. ✅ **All Fields Required Validation** - Block progress if incomplete
2. ✅ **"I Need Help" Buttons** - On every field (no skip option)
3. ✅ **Enhanced AI Assistant Integration** - Make it prominent and contextual
4. ✅ **Helpful Error Messages** - Guide users when validation fails
5. ✅ **Section-by-Section Validation** - Cannot proceed until current section complete

### **Phase 2: High Value (Implement Next)**
6. ✅ **Contextual Help Icons** - Quick tooltips on every field
7. ✅ **Smart Defaults** - Auto-fill based on industry (user must confirm)
8. ✅ **Step-by-Step Guidance** - Tips at top of each section
9. ✅ **Field-Level AI Suggestions** - AI help per field
10. ✅ **Progress Indicators** - Show completion status per section

### **Phase 3: Enhanced Guidance (Future)**
11. ✅ **Examples & Templates** - Industry-specific examples
12. ✅ **Multi-Person Collaboration** - Invite team members to help (but admin must complete)
13. ✅ **Guided Question Flow** - AI asks clarifying questions to determine answers

---

## 🎯 User Experience Flow

### **Scenario: User Doesn't Know Answer**

```
User sees question: "What is your primary regulator?" [Required]
    ↓
User tries to proceed without answering
    ↓
Validation Error: "This field is required. You cannot proceed without completing it."
    ↓
User clicks: "I need help with this"
    ↓
AI Assistant opens automatically with context
    ↓
AI asks guided questions:
    "What industry are you in?" → User: "Banking"
    "What country are you operating in?" → User: "Saudi Arabia"
    ↓
AI suggests: "Based on banking industry in Saudi Arabia, 
              your primary regulators are: SAMA, NCA"
    ↓
User can:
    ✅ Click "Use This" → Auto-fills field, user confirms
    ✅ Click "Tell me more" → Get detailed explanation
    ✅ Ask follow-up questions → AI provides more guidance
    ✅ Manually enter different value → After getting guidance
    ❌ Cannot skip → Must complete to proceed
    ↓
Field completed → User can proceed to next question
```

### **Scenario: User Tries to Skip Section**

```
User completes 4 of 5 questions in Section A
    ↓
User clicks "Next" button
    ↓
Validation runs: "2 fields incomplete"
    ↓
Error message shows:
    ❌ "Cannot proceed - All fields are required"
    📋 Missing fields:
       - Organization Type (Required)
       - Country of Incorporation (Required)
    ↓
Each missing field shows:
    - Red border/highlight
    - "This field is required" message
    - "I need help" button
    ↓
User must complete ALL fields before proceeding
```

---

## 💡 Key Principles

### **1. All Fields Required - No Skipping Allowed** ⚠️
- **ALL fields must be completed** - no exceptions
- **Block progress** if any field is incomplete
- **Guide users properly** so they CAN answer every question
- **No "Skip for now"** - must complete all 96 questions
- **Purpose**: Ensure complete, accurate organizational profile

### **2. Provide Multiple Help Channels**
- AI Assistant (primary)
- Tooltips (quick help)
- Examples (visual guidance)
- "I Don't Know" options (explicit)

### **3. Make Help Discoverable**
- Visible help icons
- Prominent AI assistant button
- Clear "Need Help?" buttons
- Contextual suggestions

### **4. Progressive Enhancement**
- Start simple (essential questions)
- Add complexity gradually
- Allow refinement later
- Don't overwhelm users

### **5. Learn from Usage**
- Track which fields users skip most
- Identify confusing questions
- Improve guidance based on data
- A/B test different help approaches

---

## ✅ Summary

**To ensure users are guided properly to complete ALL fields (NO SKIPPING):**

1. ✅ **All Fields Required** - Block progress if incomplete (no exceptions)
2. ✅ **Make AI Assistant prominent** - Always accessible, context-aware
3. ✅ **Add "I Need Help" buttons** - On every field (not "skip" option)
4. ✅ **Provide smart defaults** - Based on industry/context (user must confirm)
5. ✅ **Show helpful examples** - Visual guidance for every field
6. ✅ **Contextual help icons** - Quick tooltips explaining each field
7. ✅ **Section-by-section validation** - Cannot proceed until current section complete
8. ✅ **Guided question flow** - AI asks clarifying questions to help determine answers
9. ✅ **Helpful error messages** - Explain why field is required and how to find answer
10. ✅ **Progress indicators** - Show completion status, highlight missing fields

**Key Principle**: 
- ❌ **NO SKIPPING** - All 96 questions must be completed
- ✅ **PROPER GUIDANCE** - Help users find/understand every answer
- ✅ **BLOCK PROGRESS** - Until all fields in current section are complete
- ✅ **AI-POWERED HELP** - Intelligent assistance for every question

**Result**: Users are properly guided to complete ALL fields - no skipping, but with excellent support! 🎯
