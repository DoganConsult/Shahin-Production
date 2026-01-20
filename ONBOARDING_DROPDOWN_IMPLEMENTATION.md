# Onboarding Wizard Dropdown Implementation

## Overview

All onboarding wizard questions now use **database-driven dropdown menus** instead of free text inputs. Options are stored in the `ReferenceData` table, and AI recommendations guide users to select the most appropriate values.

## Key Features

1. **Database-Driven Options**: All dropdown values come from the `ReferenceData` table
2. **AI Recommendations**: Context-aware suggestions based on:
   - Organization type
   - Industry sector
   - Country of incorporation
   - Common practices
3. **Bilingual Support**: All options support English and Arabic labels
4. **Step Progress Tracking**: Tenant admin can search/query step progress via API
5. **Progressive Reflection**: Step completion is immediately saved and reflected in the next step

## Database Schema

### ReferenceData Table

```sql
CREATE TABLE "ReferenceData" (
    "Id" uuid PRIMARY KEY,
    "Category" varchar(100) NOT NULL,  -- e.g., "OrganizationType", "IndustrySector"
    "Value" varchar(255) NOT NULL,     -- e.g., "enterprise", "financial_services"
    "LabelEn" varchar(255) NOT NULL,   -- English display label
    "LabelAr" varchar(255),            -- Arabic display label
    "DescriptionEn" varchar(1000),      -- English help text
    "DescriptionAr" varchar(1000),     -- Arabic help text
    "SortOrder" integer DEFAULT 0,
    "IsActive" boolean DEFAULT true,
    "IsCommon" boolean DEFAULT false,  -- For AI recommendations
    "IndustryContext" varchar(100),     -- Industry-specific context
    "OrganizationTypeContext" varchar(50), -- Org type-specific context
    "MetadataJson" text,               -- Additional metadata
    "CreatedAt" timestamp with time zone,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text
);
```

### Indexes

- `IX_ReferenceData_Category` - Fast lookup by category
- `IX_ReferenceData_Category_Value` - Unique constraint per category+value
- `IX_ReferenceData_IsActive` - Filter active options
- `IX_ReferenceData_IsCommon` - Filter common/recommended options

## Services

### IOnboardingReferenceDataService

Fetches dropdown options from the database:

```csharp
Task<List<ReferenceDataOptionDto>> GetOptionsAsync(string category, string? language = "en");
Task<List<ReferenceDataOptionDto>> GetCommonOptionsAsync(string category, string? language = "en");
Task<Dictionary<string, List<ReferenceDataOptionDto>>> GetOptionsForCategoriesAsync(
    List<string> categories, string? language = "en");
```

### IOnboardingAIRecommendationService

Provides AI-powered recommendations:

```csharp
Task<OnboardingFieldRecommendationDto> GetRecommendationsAsync(
    string fieldName,
    string section,
    OnboardingContextDto? context = null,
    string? language = "en");
```

**Recommendation Logic:**
- Base confidence: 50%
- +20% if marked as "common" in database
- +30% if matches industry/organization type patterns
- +25% if country-specific (e.g., Saudi Arabia → NCA, SAMA)
- Top 3-5 recommendations returned, sorted by confidence

## Controller Updates

### OnboardingWizardController

**StepA (Example):**
- Loads dropdown options for all Step A fields
- Gets AI recommendations based on current context
- Passes options and recommendations to view via `ViewData`

**New API Endpoint:**
```csharp
[HttpGet("Api/Progress/{tenantId:guid}")]
public async Task<IActionResult> GetStepProgress(Guid tenantId)
```

Returns:
- Current step number and name
- Progress percentage
- Completed sections
- Step details (completed, current, locked status)

## Step Progress Tracking

### MarkStepCompleted Method

When a step is completed:
1. Section letter added to `CompletedSectionsJson`
2. `ProgressPercent` updated: `(completedSections.Count / 12.0) * 100`
3. `CurrentStep` updated to next step
4. `LastStepSavedAt` timestamp updated
5. Progress logged with `[ONBOARDING_PROGRESS]` marker

### Progressive Reflection

- Progress is **immediately saved** to database
- Next step **automatically reflects** completed status
- Step navigation **enforces** sequential completion
- API endpoint allows **real-time progress queries**

## Seed Data

Initial reference data includes:

1. **CountryOfIncorporation**: SA, AE, KW, QA, BH, OM, US, GB
2. **OrganizationType**: enterprise, SME, government, regulated_financial, fintech, telecom, other
3. **IndustrySector**: financial_services, telecom, government, healthcare, energy, retail, technology, manufacturing, education, other
4. **PrimaryDriver**: regulator_exam, internal_audit, external_audit, certification, customer_due_diligence, board_reporting
5. **PrimaryLanguage**: bilingual, english, arabic
6. **DefaultTimezone**: Asia/Riyadh, Asia/Dubai, Asia/Kuwait, UTC
7. **DomainVerificationMethod**: admin_email, dns_txt
8. **DesiredMaturity**: Foundation, AssuranceOps, ContinuousAssurance

## Migration

Run migration to create table:
```bash
dotnet ef migrations add AddReferenceDataTable
dotnet ef database update
```

Seed initial data:
```bash
psql -d grc_db -f scripts/seed-reference-data.sql
```

## View Updates (TODO)

Views need to be updated to:
1. Replace text inputs with `<select>` dropdowns
2. Display AI recommendations prominently
3. Show confidence scores and explanations
4. Highlight strongly recommended options
5. Support bilingual labels

## Next Steps

1. ✅ Create ReferenceData entity and table
2. ✅ Create services (ReferenceData, AI Recommendations)
3. ✅ Update controller to load options and recommendations
4. ✅ Add step progress API endpoint
5. ⏳ Update all wizard step views (StepA, StepB, ..., StepL)
6. ⏳ Create migration and seed data
7. ⏳ Test dropdown population and AI recommendations
8. ⏳ Verify step progress tracking and reflection

## Testing

1. **Database Options**: Verify all categories return correct options
2. **AI Recommendations**: Test with different organization types/industries
3. **Step Progress**: Complete Step A, verify Step B shows progress
4. **API Endpoint**: Query progress API, verify JSON response
5. **Bilingual**: Switch language, verify Arabic labels appear

---

**Status**: Backend implementation complete. Frontend views pending update.
