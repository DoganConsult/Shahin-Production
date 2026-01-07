/**
 * Shahin-AI GRC Platform - Onboarding Data Types
 * 
 * These types define the data structure for organization onboarding.
 * Used for:
 * - Frontend questionnaire form
 * - Backend API validation
 * - Database entity mapping
 * - Experience mapping / control assignment
 */

// =============================================================================
// ENUMS & CONSTANTS
// =============================================================================

export type IndustrySector =
  | 'financial'
  | 'healthcare'
  | 'government'
  | 'energy'
  | 'retail'
  | 'telecom'
  | 'manufacturing'
  | 'education'
  | 'real-estate'
  | 'other';

export type EmployeeRange =
  | '1-50'
  | '51-200'
  | '201-500'
  | '501-1000'
  | '1001-5000'
  | '5000+';

export type HeadquartersLocation =
  | 'riyadh'
  | 'jeddah'
  | 'dammam'
  | 'makkah'
  | 'madinah'
  | 'other-ksa'
  | 'gcc'
  | 'mena'
  | 'international';

export type CountryCode =
  | 'sa' | 'ae' | 'bh' | 'kw' | 'qa' | 'om' | 'eg' | 'jo' | 'eu' | 'us' | 'other';

export type RegulatorCode =
  | 'nca'
  | 'sama'
  | 'cma'
  | 'citc'
  | 'moh'
  | 'sdaia'
  | 'mcit'
  | 'zatca'
  | 'csc'
  | 'other';

export type FrameworkCode =
  | 'nca-ecc'
  | 'nca-ccc'
  | 'pdpl'
  | 'sama-csf'
  | 'sama-bcm'
  | 'iso27001'
  | 'iso22301'
  | 'soc2'
  | 'pci-dss'
  | 'hipaa'
  | 'gdpr'
  | 'csc';

export type ComplianceMaturity =
  | 'initial'
  | 'developing'
  | 'defined'
  | 'managed'
  | 'optimized';

export type AuditFrequency =
  | 'monthly'
  | 'quarterly'
  | 'semi-annual'
  | 'annual'
  | 'ad-hoc';

export type InfrastructureType =
  | 'on-premise'
  | 'private-cloud'
  | 'public-cloud'
  | 'hybrid'
  | 'saas'
  | 'colocation';

export type CloudProvider =
  | 'aws'
  | 'azure'
  | 'gcp'
  | 'alibaba'
  | 'oracle'
  | 'stc'
  | 'mobily'
  | 'other';

export type DataClassification =
  | 'pii'
  | 'financial'
  | 'health'
  | 'government'
  | 'ip'
  | 'customer'
  | 'employee';

export type ERPSystem =
  | 'sap'
  | 'oracle-erp'
  | 'dynamics365'
  | 'odoo'
  | 'erpnext'
  | 'custom'
  | 'none';

export type SecurityTool =
  | 'siem'
  | 'edr'
  | 'vuln-scanner'
  | 'iam'
  | 'dlp'
  | 'firewall'
  | 'waf'
  | 'casb';

export type TeamSize =
  | '1'
  | '2-3'
  | '4-10'
  | '11-25'
  | '25+';

export type OrgRole =
  | 'ciso'
  | 'compliance-officer'
  | 'risk-manager'
  | 'internal-auditor'
  | 'it-manager'
  | 'dpo'
  | 'legal'
  | 'ceo-coo'
  | 'board'
  | 'department-heads';

export type ApprovalWorkflowStyle =
  | 'simple'
  | 'hierarchical'
  | 'matrix'
  | 'risk-based';

export type IntegrationPreference =
  | 'ad'
  | 'okta'
  | 'slack'
  | 'teams'
  | 'jira'
  | 'email'
  | 'api';

export type PreferredLanguage = 'ar' | 'en' | 'both';

export type Timeline =
  | 'urgent'
  | 'soon'
  | 'planned'
  | 'exploring';

export type CriticalSystemsCount =
  | '1-5'
  | '6-15'
  | '16-30'
  | '31-50'
  | '50+';

// =============================================================================
// MAIN DATA INTERFACES
// =============================================================================

/**
 * Organization Profile Section
 */
export interface OrganizationProfile {
  /** Organization name in English */
  org_name: string;
  /** Organization name in Arabic (optional) */
  org_name_ar?: string;
  /** Primary industry sector */
  industry: IndustrySector;
  /** Employee count range */
  employee_count: EmployeeRange;
  /** Headquarters location */
  headquarters: HeadquartersLocation;
  /** Countries where organization operates */
  operations_countries: CountryCode[];
}

/**
 * Regulatory Requirements Section
 */
export interface RegulatoryRequirements {
  /** Primary regulators governing the organization */
  primary_regulator: RegulatorCode[];
  /** Required compliance frameworks */
  required_frameworks: FrameworkCode[];
  /** Current compliance maturity level */
  compliance_maturity: ComplianceMaturity;
  /** Expected audit frequency */
  audit_frequency?: AuditFrequency;
  /** Upcoming audits and deadlines (free text) */
  upcoming_audits?: string;
}

/**
 * IT Assets & Infrastructure Section
 */
export interface ITAssets {
  /** Types of infrastructure used */
  infrastructure_type: InfrastructureType[];
  /** Cloud service providers */
  cloud_providers?: CloudProvider[];
  /** Number of critical systems */
  critical_systems_count?: CriticalSystemsCount;
  /** Types of data processed */
  data_classification: DataClassification[];
  /** ERP and business systems */
  erp_systems?: ERPSystem[];
  /** Existing security tools */
  security_tools?: SecurityTool[];
}

/**
 * Team & Workflow Section
 */
export interface TeamWorkflow {
  /** Size of compliance/GRC team */
  compliance_team_size: TeamSize;
  /** Key roles in the organization */
  key_roles: OrgRole[];
  /** Approval workflow style preference */
  approval_workflow: ApprovalWorkflowStyle;
  /** List of departments/business units (free text) */
  departments?: string;
  /** Preferred system integrations */
  integration_preference?: IntegrationPreference[];
}

/**
 * Contact & Preferences Section
 */
export interface ContactPreferences {
  /** Primary contact name */
  contact_name: string;
  /** Contact job title */
  contact_title: string;
  /** Business email */
  contact_email: string;
  /** Phone number (optional) */
  contact_phone?: string;
  /** Preferred language for communication */
  preferred_language: PreferredLanguage;
  /** Compliance timeline urgency */
  timeline: Timeline;
  /** Additional notes (free text) */
  additional_notes?: string;
}

/**
 * Complete Onboarding Data
 * Combines all sections into a single submission
 */
export interface OnboardingData {
  organization: OrganizationProfile;
  regulatory: RegulatoryRequirements;
  assets: ITAssets;
  team: TeamWorkflow;
  contact: ContactPreferences;
  /** Submission metadata */
  metadata: {
    submitted_at: string; // ISO timestamp
    source: 'website' | 'api' | 'manual';
    version: string;
  };
}

// =============================================================================
// TEAM MEMBER CONFIGURATION (for workflow setup)
// =============================================================================

/**
 * Team member definition for workflow assignment
 */
export interface TeamMember {
  id: string;
  name: string;
  name_ar?: string;
  email: string;
  role: OrgRole;
  department: string;
  is_approver: boolean;
  approval_level?: number; // 1 = immediate, 2 = department, 3 = executive
}

/**
 * Department definition for control ownership
 */
export interface Department {
  id: string;
  name: string;
  name_ar?: string;
  head_id: string; // TeamMember.id
  control_domains: string[]; // e.g., ['access-control', 'network-security']
}

/**
 * Workflow configuration for the organization
 */
export interface WorkflowConfig {
  members: TeamMember[];
  departments: Department[];
  approval_style: ApprovalWorkflowStyle;
  escalation_days: number;
  reminder_days: number;
}

// =============================================================================
// EXPERIENCE MAPPING (for control assignment)
// =============================================================================

/**
 * Questions needed from organization for experience mapping
 */
export const EXPERIENCE_MAPPING_QUESTIONS = {
  // Asset-based questions
  assets: [
    'What critical systems do you operate?',
    'What types of data do you process?',
    'What cloud providers do you use?',
    'What security tools are already in place?',
  ],
  
  // Team-based questions
  team: [
    'Who is responsible for each control domain?',
    'What is your approval hierarchy?',
    'Who owns evidence collection for each area?',
    'Who receives audit findings?',
  ],
  
  // Compliance-based questions
  compliance: [
    'Which regulators govern your operations?',
    'Which frameworks are required?',
    'What is your compliance maturity?',
    'What are your audit deadlines?',
  ],
  
  // Operational questions
  operations: [
    'How many locations do you operate from?',
    'Do you have third-party vendors with data access?',
    'Do you process cross-border data?',
    'What ERP/business systems do you use?',
  ],
};

/**
 * Control domain mapping based on organization profile
 */
export interface ControlDomainMapping {
  domain_id: string;
  domain_name: string;
  domain_name_ar: string;
  owner_department: string;
  owner_member_id?: string;
  applicable_frameworks: FrameworkCode[];
  priority: 'critical' | 'high' | 'medium' | 'low';
}

// =============================================================================
// API RESPONSE TYPES
// =============================================================================

export interface OnboardingSubmitResponse {
  success: boolean;
  organization_id: string;
  tenant_id: string;
  message: string;
  next_steps: string[];
  assigned_frameworks: FrameworkCode[];
  estimated_controls: number;
}

export interface OnboardingValidationError {
  field: string;
  message: string;
  message_ar: string;
}
