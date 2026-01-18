# Contact Form Implementation

## Overview

The contact form is implemented at `/contact` (localized as `/ar/contact` and `/en/contact`) and integrates with the existing GrcMvc backend API.

## Implementation Details

### Frontend (`src/app/[locale]/contact/page.tsx`)

- **Location**: `/contact` route
- **Form Fields**:
  - Name (required)
  - Email (required)
  - Phone (optional)
  - Company (optional)
  - Service/Subject (dropdown, optional)
  - Message (required)
- **Validation**: Client-side HTML5 validation (required fields, email format)
- **State Management**: React hooks (`useState`) for form data, submission state, and error handling
- **User Feedback**: Success/error messages with animations (Framer Motion)

### Backend Integration (`src/lib/api.ts`)

- **API Endpoint**: `POST /api/Landing/Contact`
- **CSRF Protection**: Automatic CSRF token management
  - Token fetched from `/api/Landing/csrf-token`
  - Token cached for 30 minutes
  - Automatic retry on token expiry
- **Error Handling**: Comprehensive error handling with user-friendly messages
- **Response Format**: `{ success: boolean, message: string }`

### Security Features

1. **CSRF Token Protection**: All form submissions include a valid CSRF token
2. **Input Validation**: Client-side validation for required fields and email format
3. **Backend Validation**: Server-side validation (handled by GrcMvc backend)
4. **Rate Limiting**: **MISSING** - Currently handled by backend (if implemented). Frontend should add:
   - Client-side rate limiting stub (prevent rapid submissions)
   - Visual feedback when rate limit is hit
   - Consider adding a captcha placeholder for future implementation

### Rate Limiting (TODO)

**Current Status**: Rate limiting is assumed to be handled by the backend API.

**Recommended Frontend Implementation**:
```typescript
// Add to contact form component
const [lastSubmissionTime, setLastSubmissionTime] = useState<number | null>(null)
const RATE_LIMIT_SECONDS = 60 // 1 minute between submissions

const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault()
  
  // Rate limiting check
  if (lastSubmissionTime && Date.now() - lastSubmissionTime < RATE_LIMIT_SECONDS * 1000) {
    setError(t('form.rateLimit'))
    return
  }
  
  // ... existing submission logic ...
  setLastSubmissionTime(Date.now())
}
```

**Backend Rate Limiting**: Should be implemented in the GrcMvc backend controller (`LandingController.Contact`). Recommended:
- IP-based rate limiting (e.g., 5 submissions per hour per IP)
- Email-based rate limiting (e.g., 3 submissions per day per email)
- Use ASP.NET Core rate limiting middleware or a library like `AspNetCoreRateLimit`

### Captcha (Optional - Future Enhancement)

**Current Status**: No captcha implementation.

**Recommended Implementation**:
- **Option 1**: Google reCAPTCHA v3 (invisible, score-based)
- **Option 2**: hCaptcha (privacy-focused alternative)
- **Option 3**: Cloudflare Turnstile (modern, privacy-friendly)

**Placeholder Structure**:
```typescript
// Add to contact form
import { useReCaptcha } from 'next-recaptcha-v3' // Example library

const { executeRecaptcha } = useReCaptcha()

const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault()
  
  // Get reCAPTCHA token
  const token = await executeRecaptcha('contact_form')
  
  const response = await submitContact({
    ...formData,
    recaptchaToken: token // Include in request
  })
}
```

**Backend Verification**: The backend should verify the captcha token before processing the submission.

## API Contract

### Request (`ContactRequest`)
```typescript
{
  name: string          // Required
  email: string         // Required, valid email format
  phone?: string        // Optional
  company?: string      // Optional
  subject: string       // Required (defaults to "General Inquiry" if service not selected)
  message: string       // Required
}
```

### Response (`ContactResponse`)
```typescript
{
  success: boolean      // true if submission successful
  message: string       // Success or error message
}
```

## Error Handling

1. **Network Errors**: Displayed as user-friendly error messages
2. **Validation Errors**: Shown inline with form fields (HTML5 validation)
3. **API Errors**: Backend error messages displayed in error banner
4. **CSRF Token Errors**: Automatically retried with a new token

## Localization

All form labels, placeholders, and messages are localized via `next-intl`:
- English: `messages/en.json` → `contact.*`
- Arabic: `messages/ar.json` → `contact.*`

## Testing Checklist

- [x] Form validation (required fields)
- [x] Email format validation
- [x] Form submission to backend
- [x] Success message display
- [x] Error message display
- [x] CSRF token handling
- [ ] Rate limiting (frontend stub)
- [ ] Rate limiting (backend implementation)
- [ ] Captcha integration (if implemented)
- [ ] Accessibility (keyboard navigation, screen readers)
- [ ] Mobile responsiveness

## Missing Items

1. **Rate Limiting Frontend Stub**: Client-side rate limiting to prevent rapid submissions
2. **Captcha Integration**: No captcha currently implemented (optional enhancement)
3. **Backend Rate Limiting**: Verify backend has rate limiting implemented
4. **Email Notification**: Verify backend sends email notifications on form submission
5. **Form Analytics**: Consider adding analytics to track form submissions and conversion rates

## Next Steps

1. Implement frontend rate limiting stub
2. Evaluate need for captcha (based on spam volume)
3. Verify backend rate limiting configuration
4. Add form analytics (optional)
5. Test end-to-end flow in production environment
