'use client';

import { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import {
  Building2,
  Users,
  Shield,
  Server,
  Globe,
  CheckCircle,
  ArrowRight,
  ArrowLeft,
  Briefcase,
  MapPin,
  FileText,
  Layers,
  Lock,
  Database,
  Cloud,
  Network,
  UserCog,
  Target,
  AlertTriangle,
  Calendar,
  Mail,
  Phone,
  ChevronDown,
} from 'lucide-react';

// =============================================================================
// ONBOARDING QUESTIONNAIRE DATA STRUCTURE
// Based on GRC experience mapping requirements
// =============================================================================

interface QuestionOption {
  value: string;
  label: string;
  labelAr: string;
}

interface Question {
  id: string;
  type: 'text' | 'select' | 'multiselect' | 'number' | 'textarea' | 'toggle';
  label: string;
  labelAr: string;
  placeholder?: string;
  placeholderAr?: string;
  options?: QuestionOption[];
  required?: boolean;
  helpText?: string;
  helpTextAr?: string;
}

interface QuestionSection {
  id: string;
  title: string;
  titleAr: string;
  description: string;
  descriptionAr: string;
  icon: typeof Building2;
  questions: Question[];
}

// =============================================================================
// QUESTIONNAIRE SECTIONS & QUESTIONS
// =============================================================================

const questionnaireSections: QuestionSection[] = [
  // SECTION 1: Organization Profile
  {
    id: 'org-profile',
    title: 'Organization Profile',
    titleAr: 'ملف المنظمة',
    description: 'Tell us about your organization to customize your compliance experience',
    descriptionAr: 'أخبرنا عن منظمتك لتخصيص تجربة الامتثال الخاصة بك',
    icon: Building2,
    questions: [
      {
        id: 'org_name',
        type: 'text',
        label: 'Organization Name',
        labelAr: 'اسم المنظمة',
        placeholder: 'Enter your organization name',
        placeholderAr: 'أدخل اسم المنظمة',
        required: true,
      },
      {
        id: 'org_name_ar',
        type: 'text',
        label: 'Organization Name (Arabic)',
        labelAr: 'اسم المنظمة بالعربية',
        placeholder: 'أدخل اسم المنظمة بالعربية',
        placeholderAr: 'أدخل اسم المنظمة بالعربية',
      },
      {
        id: 'industry',
        type: 'select',
        label: 'Industry Sector',
        labelAr: 'القطاع الصناعي',
        required: true,
        options: [
          { value: 'financial', label: 'Financial Services', labelAr: 'الخدمات المالية' },
          { value: 'healthcare', label: 'Healthcare', labelAr: 'الرعاية الصحية' },
          { value: 'government', label: 'Government', labelAr: 'الحكومة' },
          { value: 'energy', label: 'Energy & Utilities', labelAr: 'الطاقة والمرافق' },
          { value: 'retail', label: 'Retail & Commerce', labelAr: 'التجزئة والتجارة' },
          { value: 'telecom', label: 'Telecom & Technology', labelAr: 'الاتصالات والتقنية' },
          { value: 'manufacturing', label: 'Manufacturing', labelAr: 'التصنيع' },
          { value: 'education', label: 'Education', labelAr: 'التعليم' },
          { value: 'real-estate', label: 'Real Estate', labelAr: 'العقارات' },
          { value: 'other', label: 'Other', labelAr: 'أخرى' },
        ],
      },
      {
        id: 'employee_count',
        type: 'select',
        label: 'Number of Employees',
        labelAr: 'عدد الموظفين',
        required: true,
        options: [
          { value: '1-50', label: '1-50', labelAr: '١-٥٠' },
          { value: '51-200', label: '51-200', labelAr: '٥١-٢٠٠' },
          { value: '201-500', label: '201-500', labelAr: '٢٠١-٥٠٠' },
          { value: '501-1000', label: '501-1,000', labelAr: '٥٠١-١٠٠٠' },
          { value: '1001-5000', label: '1,001-5,000', labelAr: '١٠٠١-٥٠٠٠' },
          { value: '5000+', label: '5,000+', labelAr: '+٥٠٠٠' },
        ],
      },
      {
        id: 'headquarters',
        type: 'select',
        label: 'Headquarters Location',
        labelAr: 'موقع المقر الرئيسي',
        required: true,
        options: [
          { value: 'riyadh', label: 'Riyadh', labelAr: 'الرياض' },
          { value: 'jeddah', label: 'Jeddah', labelAr: 'جدة' },
          { value: 'dammam', label: 'Dammam / Eastern Province', labelAr: 'الدمام / المنطقة الشرقية' },
          { value: 'makkah', label: 'Makkah', labelAr: 'مكة المكرمة' },
          { value: 'madinah', label: 'Madinah', labelAr: 'المدينة المنورة' },
          { value: 'other-ksa', label: 'Other KSA', labelAr: 'مناطق أخرى في المملكة' },
          { value: 'gcc', label: 'GCC (Outside KSA)', labelAr: 'دول الخليج (خارج المملكة)' },
          { value: 'mena', label: 'MENA (Outside GCC)', labelAr: 'منطقة الشرق الأوسط' },
          { value: 'international', label: 'International', labelAr: 'دولي' },
        ],
      },
      {
        id: 'operations_countries',
        type: 'multiselect',
        label: 'Countries of Operation',
        labelAr: 'دول العمليات',
        helpText: 'Select all countries where you have operations',
        helpTextAr: 'اختر جميع الدول التي لديك عمليات فيها',
        options: [
          { value: 'sa', label: 'Saudi Arabia', labelAr: 'المملكة العربية السعودية' },
          { value: 'ae', label: 'UAE', labelAr: 'الإمارات' },
          { value: 'bh', label: 'Bahrain', labelAr: 'البحرين' },
          { value: 'kw', label: 'Kuwait', labelAr: 'الكويت' },
          { value: 'qa', label: 'Qatar', labelAr: 'قطر' },
          { value: 'om', label: 'Oman', labelAr: 'عمان' },
          { value: 'eg', label: 'Egypt', labelAr: 'مصر' },
          { value: 'jo', label: 'Jordan', labelAr: 'الأردن' },
          { value: 'eu', label: 'European Union', labelAr: 'الاتحاد الأوروبي' },
          { value: 'us', label: 'United States', labelAr: 'الولايات المتحدة' },
          { value: 'other', label: 'Other', labelAr: 'أخرى' },
        ],
      },
    ],
  },

  // SECTION 2: Regulatory Requirements
  {
    id: 'regulatory',
    title: 'Regulatory Requirements',
    titleAr: 'المتطلبات التنظيمية',
    description: 'Which frameworks and regulations apply to your organization?',
    descriptionAr: 'ما هي الأطر واللوائح التي تنطبق على منظمتك؟',
    icon: Shield,
    questions: [
      {
        id: 'primary_regulator',
        type: 'multiselect',
        label: 'Primary Regulators',
        labelAr: 'الجهات التنظيمية الرئيسية',
        required: true,
        helpText: 'Select all regulators that govern your operations',
        helpTextAr: 'اختر جميع الجهات التنظيمية التي تحكم عملياتك',
        options: [
          { value: 'nca', label: 'NCA (National Cybersecurity Authority)', labelAr: 'الهيئة الوطنية للأمن السيبراني' },
          { value: 'sama', label: 'SAMA (Saudi Central Bank)', labelAr: 'البنك المركزي السعودي' },
          { value: 'cma', label: 'CMA (Capital Market Authority)', labelAr: 'هيئة السوق المالية' },
          { value: 'citc', label: 'CITC (Communications & IT Commission)', labelAr: 'هيئة الاتصالات وتقنية المعلومات' },
          { value: 'moh', label: 'Ministry of Health', labelAr: 'وزارة الصحة' },
          { value: 'sdaia', label: 'SDAIA (Saudi Data & AI Authority)', labelAr: 'هيئة البيانات والذكاء الاصطناعي' },
          { value: 'mcit', label: 'MCIT (Ministry of Communications & IT)', labelAr: 'وزارة الاتصالات وتقنية المعلومات' },
          { value: 'zatca', label: 'ZATCA (Zakat, Tax & Customs)', labelAr: 'هيئة الزكاة والضريبة والجمارك' },
          { value: 'csc', label: 'CSC (Cloud Service Certification)', labelAr: 'شهادة الخدمات السحابية' },
          { value: 'other', label: 'Other', labelAr: 'أخرى' },
        ],
      },
      {
        id: 'required_frameworks',
        type: 'multiselect',
        label: 'Required Compliance Frameworks',
        labelAr: 'أطر الامتثال المطلوبة',
        required: true,
        options: [
          { value: 'nca-ecc', label: 'NCA-ECC (Essential Cybersecurity Controls)', labelAr: 'الضوابط الأساسية للأمن السيبراني' },
          { value: 'nca-ccc', label: 'NCA-CCC (Critical Systems Controls)', labelAr: 'ضوابط الأنظمة الحساسة' },
          { value: 'pdpl', label: 'PDPL (Personal Data Protection Law)', labelAr: 'نظام حماية البيانات الشخصية' },
          { value: 'sama-csf', label: 'SAMA Cybersecurity Framework', labelAr: 'إطار الأمن السيبراني لساما' },
          { value: 'sama-bcm', label: 'SAMA BCM Framework', labelAr: 'إطار استمرارية الأعمال لساما' },
          { value: 'iso27001', label: 'ISO 27001:2022', labelAr: 'آيزو ٢٧٠٠١' },
          { value: 'iso22301', label: 'ISO 22301 (BCM)', labelAr: 'آيزو ٢٢٣٠١' },
          { value: 'soc2', label: 'SOC 2 Type II', labelAr: 'سوك ٢' },
          { value: 'pci-dss', label: 'PCI-DSS', labelAr: 'بي سي آي' },
          { value: 'hipaa', label: 'HIPAA', labelAr: 'هيبا' },
          { value: 'gdpr', label: 'GDPR', labelAr: 'اللائحة العامة لحماية البيانات' },
          { value: 'csc', label: 'CSC Cloud Tier', labelAr: 'مستوى الخدمات السحابية' },
        ],
      },
      {
        id: 'compliance_maturity',
        type: 'select',
        label: 'Current Compliance Maturity',
        labelAr: 'مستوى نضج الامتثال الحالي',
        required: true,
        helpText: 'Where is your organization today?',
        helpTextAr: 'أين تقف منظمتك حاليًا؟',
        options: [
          { value: 'initial', label: 'Initial - Just starting compliance journey', labelAr: 'مبتدئ - بداية رحلة الامتثال' },
          { value: 'developing', label: 'Developing - Some controls in place', labelAr: 'متطور - بعض الضوابط موجودة' },
          { value: 'defined', label: 'Defined - Documented policies & procedures', labelAr: 'محدد - سياسات وإجراءات موثقة' },
          { value: 'managed', label: 'Managed - Regular monitoring & review', labelAr: 'مُدار - مراقبة ومراجعة منتظمة' },
          { value: 'optimized', label: 'Optimized - Continuous improvement', labelAr: 'محسّن - تحسين مستمر' },
        ],
      },
      {
        id: 'audit_frequency',
        type: 'select',
        label: 'Expected Audit Frequency',
        labelAr: 'تكرار التدقيق المتوقع',
        options: [
          { value: 'monthly', label: 'Monthly', labelAr: 'شهري' },
          { value: 'quarterly', label: 'Quarterly', labelAr: 'ربع سنوي' },
          { value: 'semi-annual', label: 'Semi-Annual', labelAr: 'نصف سنوي' },
          { value: 'annual', label: 'Annual', labelAr: 'سنوي' },
          { value: 'ad-hoc', label: 'Ad-hoc / On-demand', labelAr: 'عند الحاجة' },
        ],
      },
      {
        id: 'upcoming_audits',
        type: 'textarea',
        label: 'Upcoming Audits / Deadlines',
        labelAr: 'التدقيقات القادمة / المواعيد النهائية',
        placeholder: 'e.g., NCA-ECC audit in Q2 2026, ISO 27001 recertification in Dec 2026',
        placeholderAr: 'مثال: تدقيق الضوابط الأساسية في الربع الثاني ٢٠٢٦',
        helpText: 'Help us prioritize your compliance roadmap',
        helpTextAr: 'ساعدنا في ترتيب أولويات خارطة الامتثال',
      },
    ],
  },

  // SECTION 3: IT Assets & Infrastructure
  {
    id: 'assets',
    title: 'IT Assets & Infrastructure',
    titleAr: 'الأصول والبنية التحتية',
    description: 'Help us understand your technology landscape for accurate control mapping',
    descriptionAr: 'ساعدنا في فهم المشهد التقني لديك لربط الضوابط بدقة',
    icon: Server,
    questions: [
      {
        id: 'infrastructure_type',
        type: 'multiselect',
        label: 'Infrastructure Type',
        labelAr: 'نوع البنية التحتية',
        required: true,
        options: [
          { value: 'on-premise', label: 'On-Premise Data Center', labelAr: 'مركز بيانات محلي' },
          { value: 'private-cloud', label: 'Private Cloud', labelAr: 'سحابة خاصة' },
          { value: 'public-cloud', label: 'Public Cloud (AWS/Azure/GCP)', labelAr: 'سحابة عامة' },
          { value: 'hybrid', label: 'Hybrid Cloud', labelAr: 'سحابة هجينة' },
          { value: 'saas', label: 'SaaS-Only', labelAr: 'خدمات سحابية فقط' },
          { value: 'colocation', label: 'Colocation', labelAr: 'استضافة مشتركة' },
        ],
      },
      {
        id: 'cloud_providers',
        type: 'multiselect',
        label: 'Cloud Service Providers',
        labelAr: 'مزودي الخدمات السحابية',
        options: [
          { value: 'aws', label: 'AWS', labelAr: 'أمازون' },
          { value: 'azure', label: 'Microsoft Azure', labelAr: 'مايكروسوفت أزور' },
          { value: 'gcp', label: 'Google Cloud', labelAr: 'جوجل كلاود' },
          { value: 'alibaba', label: 'Alibaba Cloud', labelAr: 'علي بابا كلاود' },
          { value: 'oracle', label: 'Oracle Cloud', labelAr: 'أوراكل كلاود' },
          { value: 'stc', label: 'STC Cloud', labelAr: 'سحابة STC' },
          { value: 'mobily', label: 'Mobily Cloud', labelAr: 'سحابة موبايلي' },
          { value: 'other', label: 'Other', labelAr: 'أخرى' },
        ],
      },
      {
        id: 'critical_systems_count',
        type: 'select',
        label: 'Number of Critical Systems',
        labelAr: 'عدد الأنظمة الحساسة',
        helpText: 'Systems that are essential for business operations',
        helpTextAr: 'الأنظمة الضرورية لاستمرار الأعمال',
        options: [
          { value: '1-5', label: '1-5', labelAr: '١-٥' },
          { value: '6-15', label: '6-15', labelAr: '٦-١٥' },
          { value: '16-30', label: '16-30', labelAr: '١٦-٣٠' },
          { value: '31-50', label: '31-50', labelAr: '٣١-٥٠' },
          { value: '50+', label: '50+', labelAr: '+٥٠' },
        ],
      },
      {
        id: 'data_classification',
        type: 'multiselect',
        label: 'Data Types Processed',
        labelAr: 'أنواع البيانات المعالجة',
        required: true,
        options: [
          { value: 'pii', label: 'Personal Identifiable Information (PII)', labelAr: 'بيانات شخصية' },
          { value: 'financial', label: 'Financial / Payment Data', labelAr: 'بيانات مالية' },
          { value: 'health', label: 'Health Records (PHI)', labelAr: 'سجلات صحية' },
          { value: 'government', label: 'Government / Classified', labelAr: 'بيانات حكومية' },
          { value: 'ip', label: 'Intellectual Property', labelAr: 'ملكية فكرية' },
          { value: 'customer', label: 'Customer Data', labelAr: 'بيانات العملاء' },
          { value: 'employee', label: 'Employee Data', labelAr: 'بيانات الموظفين' },
        ],
      },
      {
        id: 'erp_systems',
        type: 'multiselect',
        label: 'ERP / Business Systems',
        labelAr: 'أنظمة تخطيط الموارد',
        helpText: 'For evidence automation integration',
        helpTextAr: 'لأتمتة جمع الأدلة',
        options: [
          { value: 'sap', label: 'SAP', labelAr: 'ساب' },
          { value: 'oracle-erp', label: 'Oracle ERP', labelAr: 'أوراكل' },
          { value: 'dynamics365', label: 'Microsoft Dynamics 365', labelAr: 'دايناميكس ٣٦٥' },
          { value: 'odoo', label: 'Odoo', labelAr: 'أودو' },
          { value: 'erpnext', label: 'ERPNext', labelAr: 'إي آر بي نكست' },
          { value: 'custom', label: 'Custom / In-house', labelAr: 'نظام مخصص' },
          { value: 'none', label: 'None', labelAr: 'لا يوجد' },
        ],
      },
      {
        id: 'security_tools',
        type: 'multiselect',
        label: 'Existing Security Tools',
        labelAr: 'أدوات الأمان الموجودة',
        helpText: 'For integration and evidence collection',
        helpTextAr: 'للتكامل وجمع الأدلة',
        options: [
          { value: 'siem', label: 'SIEM (Splunk, QRadar, etc.)', labelAr: 'نظام إدارة الأحداث' },
          { value: 'edr', label: 'EDR/XDR', labelAr: 'حماية النقاط الطرفية' },
          { value: 'vuln-scanner', label: 'Vulnerability Scanner', labelAr: 'ماسح الثغرات' },
          { value: 'iam', label: 'IAM / PAM', labelAr: 'إدارة الهوية' },
          { value: 'dlp', label: 'DLP', labelAr: 'منع تسريب البيانات' },
          { value: 'firewall', label: 'Next-Gen Firewall', labelAr: 'جدار حماية متقدم' },
          { value: 'waf', label: 'WAF', labelAr: 'جدار حماية التطبيقات' },
          { value: 'casb', label: 'CASB', labelAr: 'وسيط أمان السحابة' },
        ],
      },
    ],
  },

  // SECTION 4: Team & Workflow
  {
    id: 'team',
    title: 'Team & Workflow Setup',
    titleAr: 'إعداد الفريق وسير العمل',
    description: 'Configure your team structure for approval workflows',
    descriptionAr: 'قم بتكوين هيكل فريقك لمسارات الموافقة',
    icon: Users,
    questions: [
      {
        id: 'compliance_team_size',
        type: 'select',
        label: 'Compliance / GRC Team Size',
        labelAr: 'حجم فريق الامتثال',
        required: true,
        options: [
          { value: '1', label: '1 person (part-time)', labelAr: 'شخص واحد (جزئي)' },
          { value: '2-3', label: '2-3 people', labelAr: '٢-٣ أشخاص' },
          { value: '4-10', label: '4-10 people', labelAr: '٤-١٠ أشخاص' },
          { value: '11-25', label: '11-25 people', labelAr: '١١-٢٥ شخص' },
          { value: '25+', label: '25+ people', labelAr: '+٢٥ شخص' },
        ],
      },
      {
        id: 'key_roles',
        type: 'multiselect',
        label: 'Key Roles in Your Organization',
        labelAr: 'الأدوار الرئيسية في منظمتك',
        required: true,
        helpText: 'Select all roles that will use the platform',
        helpTextAr: 'اختر جميع الأدوار التي ستستخدم المنصة',
        options: [
          { value: 'ciso', label: 'CISO / Security Director', labelAr: 'مدير الأمن السيبراني' },
          { value: 'compliance-officer', label: 'Compliance Officer', labelAr: 'مسؤول الامتثال' },
          { value: 'risk-manager', label: 'Risk Manager', labelAr: 'مدير المخاطر' },
          { value: 'internal-auditor', label: 'Internal Auditor', labelAr: 'المدقق الداخلي' },
          { value: 'it-manager', label: 'IT Manager', labelAr: 'مدير تقنية المعلومات' },
          { value: 'dpo', label: 'Data Protection Officer', labelAr: 'مسؤول حماية البيانات' },
          { value: 'legal', label: 'Legal / General Counsel', labelAr: 'الشؤون القانونية' },
          { value: 'ceo-coo', label: 'CEO / COO', labelAr: 'الرئيس التنفيذي' },
          { value: 'board', label: 'Board / Audit Committee', labelAr: 'مجلس الإدارة' },
          { value: 'department-heads', label: 'Department Heads', labelAr: 'رؤساء الأقسام' },
        ],
      },
      {
        id: 'approval_workflow',
        type: 'select',
        label: 'Approval Workflow Style',
        labelAr: 'نمط سير العمل للموافقات',
        required: true,
        options: [
          { value: 'simple', label: 'Simple (Single Approver)', labelAr: 'بسيط (موافق واحد)' },
          { value: 'hierarchical', label: 'Hierarchical (Manager → Director → VP)', labelAr: 'تسلسلي (مدير ← مدير إدارة ← نائب الرئيس)' },
          { value: 'matrix', label: 'Matrix (Multiple Approvers)', labelAr: 'مصفوفي (موافقين متعددين)' },
          { value: 'risk-based', label: 'Risk-Based (High risk = more approvers)', labelAr: 'حسب المخاطر' },
        ],
      },
      {
        id: 'departments',
        type: 'textarea',
        label: 'List Your Departments / Business Units',
        labelAr: 'قائمة الأقسام / وحدات الأعمال',
        placeholder: 'e.g., IT, Finance, HR, Operations, Sales, Legal...',
        placeholderAr: 'مثال: تقنية المعلومات، المالية، الموارد البشرية...',
        helpText: 'These will be used for control ownership assignment',
        helpTextAr: 'ستُستخدم لتعيين ملكية الضوابط',
      },
      {
        id: 'integration_preference',
        type: 'multiselect',
        label: 'Preferred Integrations',
        labelAr: 'التكاملات المفضلة',
        options: [
          { value: 'ad', label: 'Active Directory / Azure AD', labelAr: 'دليل أكتيف' },
          { value: 'okta', label: 'Okta / Auth0', labelAr: 'أوكتا' },
          { value: 'slack', label: 'Slack', labelAr: 'سلاك' },
          { value: 'teams', label: 'Microsoft Teams', labelAr: 'تيمز' },
          { value: 'jira', label: 'Jira / ServiceNow', labelAr: 'جيرا' },
          { value: 'email', label: 'Email Notifications', labelAr: 'إشعارات البريد' },
          { value: 'api', label: 'Custom API', labelAr: 'واجهة برمجة مخصصة' },
        ],
      },
    ],
  },

  // SECTION 5: Contact & Preferences
  {
    id: 'contact',
    title: 'Contact & Preferences',
    titleAr: 'معلومات الاتصال والتفضيلات',
    description: 'How can we best serve you?',
    descriptionAr: 'كيف يمكننا خدمتك بشكل أفضل؟',
    icon: Mail,
    questions: [
      {
        id: 'contact_name',
        type: 'text',
        label: 'Primary Contact Name',
        labelAr: 'اسم جهة الاتصال الرئيسية',
        required: true,
      },
      {
        id: 'contact_title',
        type: 'text',
        label: 'Job Title',
        labelAr: 'المسمى الوظيفي',
        required: true,
      },
      {
        id: 'contact_email',
        type: 'text',
        label: 'Business Email',
        labelAr: 'البريد الإلكتروني',
        required: true,
        placeholder: 'name@company.com',
      },
      {
        id: 'contact_phone',
        type: 'text',
        label: 'Phone Number',
        labelAr: 'رقم الهاتف',
        placeholder: '+966 5X XXX XXXX',
      },
      {
        id: 'preferred_language',
        type: 'select',
        label: 'Preferred Language',
        labelAr: 'اللغة المفضلة',
        required: true,
        options: [
          { value: 'ar', label: 'العربية (Arabic)', labelAr: 'العربية' },
          { value: 'en', label: 'English', labelAr: 'الإنجليزية' },
          { value: 'both', label: 'Both / Bilingual', labelAr: 'كلاهما' },
        ],
      },
      {
        id: 'timeline',
        type: 'select',
        label: 'When do you need to be compliant?',
        labelAr: 'متى تحتاج لتحقيق الامتثال؟',
        required: true,
        options: [
          { value: 'urgent', label: 'Urgent (Within 1 month)', labelAr: 'عاجل (خلال شهر)' },
          { value: 'soon', label: 'Soon (1-3 months)', labelAr: 'قريب (١-٣ أشهر)' },
          { value: 'planned', label: 'Planned (3-6 months)', labelAr: 'مخطط (٣-٦ أشهر)' },
          { value: 'exploring', label: 'Exploring Options', labelAr: 'استكشاف الخيارات' },
        ],
      },
      {
        id: 'additional_notes',
        type: 'textarea',
        label: 'Additional Notes or Requirements',
        labelAr: 'ملاحظات أو متطلبات إضافية',
        placeholder: 'Any specific challenges, requirements, or questions...',
        placeholderAr: 'أي تحديات أو متطلبات أو أسئلة محددة...',
      },
    ],
  },
];

// =============================================================================
// COMPONENT
// =============================================================================

export default function OnboardingQuestionnaire() {
  const [currentSection, setCurrentSection] = useState(0);
  const [formData, setFormData] = useState<Record<string, any>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isComplete, setIsComplete] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [redirectUrl, setRedirectUrl] = useState<string | null>(null);

  // API Configuration
  const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://app.shahin-ai.com';

  const section = questionnaireSections[currentSection];
  const progress = ((currentSection + 1) / questionnaireSections.length) * 100;

  const handleInputChange = (questionId: string, value: any) => {
    setFormData(prev => ({ ...prev, [questionId]: value }));
  };

  const handleMultiSelect = (questionId: string, value: string) => {
    const current = formData[questionId] || [];
    const updated = current.includes(value)
      ? current.filter((v: string) => v !== value)
      : [...current, value];
    setFormData(prev => ({ ...prev, [questionId]: updated }));
  };

  const goNext = () => {
    if (currentSection < questionnaireSections.length - 1) {
      setCurrentSection(prev => prev + 1);
    }
  };

  const goPrev = () => {
    if (currentSection > 0) {
      setCurrentSection(prev => prev - 1);
    }
  };

  const handleSubmit = async () => {
    setIsSubmitting(true);
    setSubmitError(null);

    try {
      // Prepare trial signup data
      const trialData = {
        Email: formData.contact_email || '',
        FullName: formData.contact_name || '',
        CompanyName: formData.org_name || '',
        PhoneNumber: formData.contact_phone || '',
        CompanySize: formData.employee_count || '',
        Industry: formData.industry || '',
        TrialPlan: 'PROFESSIONAL',
        Locale: formData.preferred_language || 'ar',
        // Additional onboarding data
        OrganizationNameAr: formData.org_name_ar || '',
        Headquarters: formData.headquarters || '',
        OperationsCountries: formData.operations_countries || [],
        PrimaryRegulators: formData.primary_regulator || [],
        RequiredFrameworks: formData.required_frameworks || [],
        ComplianceMaturity: formData.compliance_maturity || '',
        InfrastructureType: formData.infrastructure_type || [],
        DataClassification: formData.data_classification || [],
        ComplianceTeamSize: formData.compliance_team_size || '',
        KeyRoles: formData.key_roles || [],
        ApprovalWorkflow: formData.approval_workflow || '',
        Timeline: formData.timeline || '',
        AdditionalNotes: formData.additional_notes || ''
      };

      const response = await fetch(`${API_BASE_URL}/api/Landing/StartTrial`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(trialData)
      });

      const result = await response.json();

      if (response.ok && result.success) {
        setIsComplete(true);
        setRedirectUrl(result.redirectUrl);
        console.log('Trial signup successful:', result);
        
        // Auto-redirect after 3 seconds
        if (result.redirectUrl) {
          setTimeout(() => {
            window.location.href = `${API_BASE_URL}${result.redirectUrl}`;
          }, 3000);
        }
      } else {
        setSubmitError(result.messageEn || result.message || 'Signup failed. Please try again.');
        console.error('Trial signup failed:', result);
      }
    } catch (error) {
      console.error('Network error:', error);
      setSubmitError('Network error. Please check your connection and try again.');
    }

    setIsSubmitting(false);
  };

  if (isComplete) {
    return (
      <section className="py-20 bg-gradient-to-b from-emerald-50 to-white">
        <div className="container mx-auto px-6">
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            className="max-w-2xl mx-auto text-center"
          >
            <div className="w-20 h-20 mx-auto mb-6 rounded-full bg-emerald-100 flex items-center justify-center">
              <CheckCircle className="w-10 h-10 text-emerald-600" />
            </div>
            <h2 className="text-3xl font-bold text-slate-900 mb-4">
              Thank You! شكراً لك
            </h2>
            <p className="text-lg text-slate-600 mb-4">
              Your organization profile has been submitted successfully.
            </p>
            <p className="text-lg text-slate-500 mb-8" dir="rtl">
              تم إرسال ملف منظمتك بنجاح. سيتواصل معك فريقنا قريباً.
            </p>
            <div className="bg-white p-6 rounded-2xl border border-slate-200">
              <h3 className="font-semibold text-slate-900 mb-2">What Happens Next?</h3>
              <ul className="text-left text-slate-600 space-y-2">
                <li className="flex items-center gap-2">
                  <CheckCircle className="w-4 h-4 text-emerald-500" />
                  Our team will review your profile (within 24 hours)
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="w-4 h-4 text-emerald-500" />
                  We'll prepare a tailored demo based on your requirements
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle className="w-4 h-4 text-emerald-500" />
                  Schedule a call to discuss your compliance roadmap
                </li>
              </ul>
            </div>
          </motion.div>
        </div>
      </section>
    );
  }

  return (
    <section id="onboarding" className="py-20 bg-gradient-to-b from-slate-50 to-white">
      <div className="container mx-auto px-6">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          className="text-center mb-12"
        >
          <h2 className="text-3xl md:text-4xl font-bold text-slate-900 mb-4">
            Start Your Compliance Journey
          </h2>
          <p className="text-lg text-slate-600 mb-2">
            Answer a few questions to get a personalized demo and compliance roadmap
          </p>
          <p className="text-lg text-slate-500" dir="rtl">
            أجب على بعض الأسئلة للحصول على عرض مخصص وخارطة طريق للامتثال
          </p>
        </motion.div>

        {/* Progress Bar */}
        <div className="max-w-4xl mx-auto mb-8">
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm text-slate-600">
              Step {currentSection + 1} of {questionnaireSections.length}
            </span>
            <span className="text-sm text-slate-600">{Math.round(progress)}% Complete</span>
          </div>
          <div className="h-2 bg-slate-200 rounded-full overflow-hidden">
            <motion.div
              className="h-full bg-gradient-to-r from-emerald-500 to-teal-500"
              initial={{ width: 0 }}
              animate={{ width: `${progress}%` }}
              transition={{ duration: 0.3 }}
            />
          </div>
        </div>

        {/* Section Navigation */}
        <div className="max-w-4xl mx-auto mb-8">
          <div className="flex flex-wrap gap-2 justify-center">
            {questionnaireSections.map((sec, idx) => (
              <button
                key={sec.id}
                onClick={() => setCurrentSection(idx)}
                className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                  idx === currentSection
                    ? 'bg-emerald-600 text-white'
                    : idx < currentSection
                    ? 'bg-emerald-100 text-emerald-700'
                    : 'bg-slate-100 text-slate-500'
                }`}
              >
                <sec.icon className="w-4 h-4" />
                <span className="hidden md:inline">{sec.title}</span>
              </button>
            ))}
          </div>
        </div>

        {/* Form Section */}
        <AnimatePresence mode="wait">
          <motion.div
            key={section.id}
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            className="max-w-4xl mx-auto"
          >
            <div className="bg-white rounded-2xl border border-slate-200 shadow-lg p-8">
              {/* Section Header */}
              <div className="flex items-center gap-4 mb-6">
                <div className="w-12 h-12 rounded-xl bg-emerald-100 flex items-center justify-center">
                  <section.icon className="w-6 h-6 text-emerald-600" />
                </div>
                <div>
                  <h3 className="text-xl font-bold text-slate-900">{section.title}</h3>
                  <p className="text-sm text-slate-500" dir="rtl">{section.titleAr}</p>
                </div>
              </div>
              <p className="text-slate-600 mb-8">{section.description}</p>

              {/* Questions */}
              <div className="space-y-6">
                {section.questions.map((question) => (
                  <div key={question.id} className="space-y-2">
                    <label className="block">
                      <span className="font-medium text-slate-900">
                        {question.label}
                        {question.required && <span className="text-red-500 ml-1">*</span>}
                      </span>
                      <span className="block text-sm text-slate-500" dir="rtl">
                        {question.labelAr}
                      </span>
                    </label>

                    {question.helpText && (
                      <p className="text-sm text-slate-500">{question.helpText}</p>
                    )}

                    {/* Text Input */}
                    {question.type === 'text' && (
                      <input
                        type="text"
                        value={formData[question.id] || ''}
                        onChange={(e) => handleInputChange(question.id, e.target.value)}
                        placeholder={question.placeholder}
                        className="w-full px-4 py-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-all"
                      />
                    )}

                    {/* Textarea */}
                    {question.type === 'textarea' && (
                      <textarea
                        value={formData[question.id] || ''}
                        onChange={(e) => handleInputChange(question.id, e.target.value)}
                        placeholder={question.placeholder}
                        rows={3}
                        className="w-full px-4 py-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-all resize-none"
                      />
                    )}

                    {/* Select */}
                    {question.type === 'select' && question.options && (
                      <select
                        value={formData[question.id] || ''}
                        onChange={(e) => handleInputChange(question.id, e.target.value)}
                        className="w-full px-4 py-3 border border-slate-300 rounded-xl focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-all bg-white"
                      >
                        <option value="">Select...</option>
                        {question.options.map((opt) => (
                          <option key={opt.value} value={opt.value}>
                            {opt.label} | {opt.labelAr}
                          </option>
                        ))}
                      </select>
                    )}

                    {/* Multi-Select */}
                    {question.type === 'multiselect' && question.options && (
                      <div className="flex flex-wrap gap-2">
                        {question.options.map((opt) => {
                          const isSelected = (formData[question.id] || []).includes(opt.value);
                          return (
                            <button
                              key={opt.value}
                              type="button"
                              onClick={() => handleMultiSelect(question.id, opt.value)}
                              className={`px-4 py-2 rounded-lg text-sm font-medium transition-all border ${
                                isSelected
                                  ? 'bg-emerald-600 text-white border-emerald-600'
                                  : 'bg-white text-slate-700 border-slate-300 hover:border-emerald-400'
                              }`}
                            >
                              {opt.label}
                            </button>
                          );
                        })}
                      </div>
                    )}
                  </div>
                ))}
              </div>

              {/* Navigation Buttons */}
              <div className="flex justify-between mt-10 pt-6 border-t border-slate-200">
                <button
                  onClick={goPrev}
                  disabled={currentSection === 0}
                  className={`flex items-center gap-2 px-6 py-3 rounded-xl font-medium transition-all ${
                    currentSection === 0
                      ? 'bg-slate-100 text-slate-400 cursor-not-allowed'
                      : 'bg-slate-200 text-slate-700 hover:bg-slate-300'
                  }`}
                >
                  <ArrowLeft className="w-5 h-5" />
                  Previous
                </button>

                {currentSection < questionnaireSections.length - 1 ? (
                  <button
                    onClick={goNext}
                    className="flex items-center gap-2 px-6 py-3 bg-emerald-600 text-white rounded-xl font-medium hover:bg-emerald-700 transition-all"
                  >
                    Next
                    <ArrowRight className="w-5 h-5" />
                  </button>
                ) : (
                  <button
                    onClick={handleSubmit}
                    disabled={isSubmitting}
                    className="flex items-center gap-2 px-8 py-3 bg-gradient-to-r from-emerald-600 to-teal-600 text-white rounded-xl font-medium hover:from-emerald-700 hover:to-teal-700 transition-all disabled:opacity-50"
                  >
                    {isSubmitting ? (
                      <>
                        <span className="animate-spin">⏳</span>
                        Submitting...
                      </>
                    ) : (
                      <>
                        Submit Application
                        <CheckCircle className="w-5 h-5" />
                      </>
                    )}
                  </button>
                )}
              </div>
            </div>
          </motion.div>
        </AnimatePresence>
      </div>
    </section>
  );
}
