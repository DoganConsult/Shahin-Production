// API Response Types

export interface ApiResponse<T = unknown> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

// CSRF Token
export interface CsrfTokenResponse {
  success: boolean;
  token: string;
  headerName: string;
}

// Contact Form
export interface ContactRequest {
  name: string;
  email: string;
  company?: string;
  phone?: string;
  subject: string;
  message: string;
}

export interface ContactResponse {
  success: boolean;
  message: string;
}

// Newsletter
export interface NewsletterRequest {
  email: string;
  name?: string;
  locale?: string;
  interests?: string[];
}

export interface NewsletterResponse {
  success: boolean;
  message: string;
}

// Trial Signup
export interface TrialSignupRequest {
  email: string;
  fullName: string;
  companyName: string;
  phoneNumber?: string;
  companySize?: string;
  industry?: string;
  trialPlan?: string;
  locale?: string;
}

export interface TrialSignupResponse {
  success: boolean;
  messageEn: string;
  messageAr: string;
  redirectUrl: string;
  signupId: string;
}

// Landing Page Data
export interface LandingPageData {
  clientLogos: ClientLogo[];
  trustBadges: TrustBadge[];
  faqs: FaqItem[];
  statistics: StatisticsData;
  features: FeatureItem[];
  partners: PartnerItem[];
  testimonials: TestimonialItem[];
}

export interface ClientLogo {
  name: string;
  logoUrl: string;
  websiteUrl?: string;
}

export interface TrustBadge {
  name: string;
  imageUrl: string;
  description?: string;
}

export interface FaqItem {
  question: string;
  questionAr?: string;
  answer: string;
  answerAr?: string;
  category?: string;
}

export interface StatisticsData {
  totalClients: number;
  controlsManaged: number;
  frameworksSupported: number;
  uptime: string;
}

export interface FeatureItem {
  title: string;
  titleAr?: string;
  description: string;
  descriptionAr?: string;
  icon?: string;
  category?: string;
}

export interface PartnerItem {
  name: string;
  logoUrl: string;
  type: string;
  description?: string;
}

export interface TestimonialItem {
  name: string;
  role: string;
  company: string;
  content: string;
  contentAr?: string;
  rating?: number;
  imageUrl?: string;
}
