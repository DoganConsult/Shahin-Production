-- Seed Reference Data for Onboarding Wizard Dropdowns
-- All options are stored in database, not hardcoded in JSON or scripts
-- Run this after creating the ReferenceData table

-- ============================================================================
-- CountryOfIncorporation Options
-- ============================================================================
INSERT INTO "ReferenceData" ("Id", "Category", "Value", "LabelEn", "LabelAr", "DescriptionEn", "DescriptionAr", "SortOrder", "IsActive", "IsCommon", "CreatedAt", "CreatedBy")
VALUES
    (gen_random_uuid(), 'CountryOfIncorporation', 'SA', 'Saudi Arabia', 'المملكة العربية السعودية', 'Kingdom of Saudi Arabia', 'المملكة العربية السعودية', 1, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'CountryOfIncorporation', 'AE', 'United Arab Emirates', 'الإمارات العربية المتحدة', 'United Arab Emirates', 'الإمارات العربية المتحدة', 2, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'CountryOfIncorporation', 'KW', 'Kuwait', 'الكويت', 'State of Kuwait', 'دولة الكويت', 3, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'CountryOfIncorporation', 'QA', 'Qatar', 'قطر', 'State of Qatar', 'دولة قطر', 4, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'CountryOfIncorporation', 'BH', 'Bahrain', 'البحرين', 'Kingdom of Bahrain', 'مملكة البحرين', 5, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'CountryOfIncorporation', 'OM', 'Oman', 'عمان', 'Sultanate of Oman', 'سلطنة عمان', 6, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'CountryOfIncorporation', 'US', 'United States', 'الولايات المتحدة', 'United States of America', 'الولايات المتحدة الأمريكية', 7, true, false, NOW(), 'system'),
    (gen_random_uuid(), 'CountryOfIncorporation', 'GB', 'United Kingdom', 'المملكة المتحدة', 'United Kingdom', 'المملكة المتحدة', 8, true, false, NOW(), 'system');

-- ============================================================================
-- OrganizationType Options
-- ============================================================================
INSERT INTO "ReferenceData" ("Id", "Category", "Value", "LabelEn", "LabelAr", "DescriptionEn", "DescriptionAr", "SortOrder", "IsActive", "IsCommon", "CreatedAt", "CreatedBy")
VALUES
    (gen_random_uuid(), 'OrganizationType', 'enterprise', 'Enterprise', 'مؤسسة كبيرة', 'Large enterprise organization (1000+ employees)', 'مؤسسة كبيرة (أكثر من 1000 موظف)', 1, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'OrganizationType', 'SME', 'SME', 'منشأة صغيرة ومتوسطة', 'Small and Medium Enterprise', 'منشأة صغيرة ومتوسطة', 2, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'OrganizationType', 'government', 'Government', 'حكومي', 'Government entity or agency', 'كيان أو وكالة حكومية', 3, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'OrganizationType', 'regulated_financial', 'Regulated Financial Institution', 'مؤسسة مالية منظمة', 'Bank, insurance, or other regulated financial institution', 'بنك أو تأمين أو مؤسسة مالية منظمة أخرى', 4, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'OrganizationType', 'fintech', 'Fintech', 'التكنولوجيا المالية', 'Financial technology company', 'شركة تكنولوجيا مالية', 5, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'OrganizationType', 'telecom', 'Telecommunications', 'الاتصالات', 'Telecommunications provider', 'مزود خدمات الاتصالات', 6, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'OrganizationType', 'other', 'Other', 'أخرى', 'Other organization type', 'نوع منظمة آخر', 7, true, false, NOW(), 'system');

-- ============================================================================
-- IndustrySector Options
-- ============================================================================
INSERT INTO "ReferenceData" ("Id", "Category", "Value", "LabelEn", "LabelAr", "DescriptionEn", "DescriptionAr", "SortOrder", "IsActive", "IsCommon", "CreatedAt", "CreatedBy")
VALUES
    (gen_random_uuid(), 'IndustrySector', 'financial_services', 'Financial Services', 'الخدمات المالية', 'Banking, insurance, investment services', 'الخدمات المصرفية والتأمين والاستثمار', 1, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'IndustrySector', 'telecom', 'Telecommunications', 'الاتصالات', 'Telecommunications and IT services', 'الاتصالات وخدمات تكنولوجيا المعلومات', 2, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'IndustrySector', 'government', 'Government', 'حكومي', 'Government and public sector', 'القطاع الحكومي والعام', 3, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'IndustrySector', 'healthcare', 'Healthcare', 'الرعاية الصحية', 'Healthcare and medical services', 'الرعاية الصحية والخدمات الطبية', 4, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'IndustrySector', 'energy', 'Energy', 'الطاقة', 'Oil, gas, and renewable energy', 'النفط والغاز والطاقة المتجددة', 5, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'IndustrySector', 'retail', 'Retail', 'التجزئة', 'Retail and e-commerce', 'التجزئة والتجارة الإلكترونية', 6, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'IndustrySector', 'technology', 'Technology', 'التكنولوجيا', 'Software and technology services', 'البرمجيات وخدمات التكنولوجيا', 7, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'IndustrySector', 'manufacturing', 'Manufacturing', 'التصنيع', 'Manufacturing and industrial', 'التصنيع والصناعة', 8, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'IndustrySector', 'education', 'Education', 'التعليم', 'Education and training', 'التعليم والتدريب', 9, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'IndustrySector', 'other', 'Other', 'أخرى', 'Other industry sector', 'قطاع صناعي آخر', 10, true, false, NOW(), 'system');

-- ============================================================================
-- PrimaryDriver Options
-- ============================================================================
INSERT INTO "ReferenceData" ("Id", "Category", "Value", "LabelEn", "LabelAr", "DescriptionEn", "DescriptionAr", "SortOrder", "IsActive", "IsCommon", "CreatedAt", "CreatedBy")
VALUES
    (gen_random_uuid(), 'PrimaryDriver', 'regulator_exam', 'Regulator Examination', 'فحص المنظم', 'Preparing for regulatory examination or audit', 'التحضير لفحص أو تدقيق المنظم', 1, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'PrimaryDriver', 'internal_audit', 'Internal Audit', 'التدقيق الداخلي', 'Internal audit requirements', 'متطلبات التدقيق الداخلي', 2, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'PrimaryDriver', 'external_audit', 'External Audit', 'التدقيق الخارجي', 'External audit or certification', 'التدقيق الخارجي أو الشهادة', 3, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'PrimaryDriver', 'certification', 'Certification', 'الشهادة', 'ISO, SOC 2, or other certification', 'ISO أو SOC 2 أو شهادة أخرى', 4, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'PrimaryDriver', 'customer_due_diligence', 'Customer Due Diligence', 'العناية الواجبة للعملاء', 'Customer or partner due diligence requirements', 'متطلبات العناية الواجبة للعملاء أو الشركاء', 5, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'PrimaryDriver', 'board_reporting', 'Board Reporting', 'تقارير مجلس الإدارة', 'Board of directors reporting requirements', 'متطلبات تقارير مجلس الإدارة', 6, true, true, NOW(), 'system');

-- ============================================================================
-- PrimaryLanguage Options
-- ============================================================================
INSERT INTO "ReferenceData" ("Id", "Category", "Value", "LabelEn", "LabelAr", "DescriptionEn", "DescriptionAr", "SortOrder", "IsActive", "IsCommon", "CreatedAt", "CreatedBy")
VALUES
    (gen_random_uuid(), 'PrimaryLanguage', 'bilingual', 'Bilingual (English/Arabic)', 'ثنائي اللغة (إنجليزي/عربي)', 'Both English and Arabic', 'الإنجليزية والعربية معاً', 1, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'PrimaryLanguage', 'english', 'English', 'الإنجليزية', 'English only', 'الإنجليزية فقط', 2, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'PrimaryLanguage', 'arabic', 'Arabic', 'العربية', 'Arabic only', 'العربية فقط', 3, true, true, NOW(), 'system');

-- ============================================================================
-- DefaultTimezone Options (Common timezones)
-- ============================================================================
INSERT INTO "ReferenceData" ("Id", "Category", "Value", "LabelEn", "LabelAr", "DescriptionEn", "DescriptionAr", "SortOrder", "IsActive", "IsCommon", "CreatedAt", "CreatedBy")
VALUES
    (gen_random_uuid(), 'DefaultTimezone', 'Asia/Riyadh', 'Riyadh (UTC+3)', 'الرياض (UTC+3)', 'Saudi Arabia Standard Time', 'توقيت المملكة العربية السعودية', 1, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'DefaultTimezone', 'Asia/Dubai', 'Dubai (UTC+4)', 'دبي (UTC+4)', 'United Arab Emirates Standard Time', 'توقيت الإمارات العربية المتحدة', 2, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'DefaultTimezone', 'Asia/Kuwait', 'Kuwait (UTC+3)', 'الكويت (UTC+3)', 'Kuwait Standard Time', 'توقيت الكويت', 3, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'DefaultTimezone', 'UTC', 'UTC (UTC+0)', 'UTC (UTC+0)', 'Coordinated Universal Time', 'التوقيت العالمي المنسق', 4, true, false, NOW(), 'system');

-- ============================================================================
-- DomainVerificationMethod Options
-- ============================================================================
INSERT INTO "ReferenceData" ("Id", "Category", "Value", "LabelEn", "LabelAr", "DescriptionEn", "DescriptionAr", "SortOrder", "IsActive", "IsCommon", "CreatedAt", "CreatedBy")
VALUES
    (gen_random_uuid(), 'DomainVerificationMethod', 'admin_email', 'Admin Email Confirmation', 'تأكيد بريد المسؤول', 'Verify domain ownership via admin email', 'التحقق من ملكية النطاق عبر بريد المسؤول', 1, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'DomainVerificationMethod', 'dns_txt', 'DNS TXT Record', 'سجل DNS TXT', 'Verify domain ownership via DNS TXT record', 'التحقق من ملكية النطاق عبر سجل DNS TXT', 2, true, true, NOW(), 'system');

-- ============================================================================
-- DesiredMaturity Options
-- ============================================================================
INSERT INTO "ReferenceData" ("Id", "Category", "Value", "LabelEn", "LabelAr", "DescriptionEn", "DescriptionAr", "SortOrder", "IsActive", "IsCommon", "CreatedAt", "CreatedBy")
VALUES
    (gen_random_uuid(), 'DesiredMaturity', 'Foundation', 'Foundation', 'الأساس', 'Basic GRC foundation and compliance tracking', 'أساس GRC الأساسي وتتبع الامتثال', 1, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'DesiredMaturity', 'AssuranceOps', 'Assurance Operations', 'عمليات الضمان', 'Operational assurance and continuous monitoring', 'الضمان التشغيلي والمراقبة المستمرة', 2, true, true, NOW(), 'system'),
    (gen_random_uuid(), 'DesiredMaturity', 'ContinuousAssurance', 'Continuous Assurance', 'الضمان المستمر', 'Advanced continuous assurance and automation', 'الضمان المستمر المتقدم والأتمتة', 3, true, true, NOW(), 'system');
