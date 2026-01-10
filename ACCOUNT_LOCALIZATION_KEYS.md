# Account Views - i18n Localization Keys

**Total Files**: 19
**Pattern**: `Account_ViewName_Element`
**Status**: Most files already converted with `Auth_*` prefix - documenting all keys

---

## Overview

All Account view files use the `Auth_` prefix for authentication-related localization keys. This document catalogs all localization keys used across 19 Account views.

---

## 1. AccessDenied.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_AccessDenied | Access Denied | الوصول مرفوض | Page title and heading |
| Auth_AccessDeniedMessage | You do not have permission to access this resource. | ليس لديك صلاحية للوصول إلى هذا المورد. | Error message |
| Auth_ContactAdministrator | Please contact your system administrator if you believe this is an error. | يرجى الاتصال بمسؤول النظام إذا كنت تعتقد أن هذا خطأ. | Help text |
| Auth_GoToDashboard | Go to Dashboard | الذهاب إلى لوحة التحكم | Button text |

---

## 2. ChangePassword.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_ChangePassword | Change Password | تغيير كلمة المرور | Page title and button |
| Auth_PasswordMinLengthHint | Password must be at least 8 characters long. | يجب أن تتكون كلمة المرور من 8 أحرف على الأقل. | Password hint |
| Auth_BackToProfile | Back to Profile | العودة إلى الملف الشخصي | Navigation link |

**ViewModel Properties** (auto-localized):
- CurrentPassword
- NewPassword
- ConfirmPassword

---

## 3. ChangePasswordRequired.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_ChangePasswordRequired | Change Password Required | تغيير كلمة المرور مطلوب | Page title |
| Auth_PasswordChangeRequired | Password Change Required | تغيير كلمة المرور مطلوب | Card header |
| Auth_MustChangePassword | For security reasons, you must change your password before continuing. | لأسباب أمنية، يجب عليك تغيير كلمة المرور قبل المتابعة. | Info alert |
| Auth_CurrentPassword | Current Password | كلمة المرور الحالية | Label |
| Auth_EnterCurrentPassword | Enter your current password | أدخل كلمة المرور الحالية | Placeholder |
| Auth_NewPassword | New Password | كلمة المرور الجديدة | Label |
| Auth_EnterNewPassword | Enter new password | أدخل كلمة المرور الجديدة | Placeholder |
| Auth_PasswordMin8Chars | Password must be at least 8 characters | يجب أن تتكون كلمة المرور من 8 أحرف على الأقل | Help text |
| Auth_ConfirmNewPassword | Confirm New Password | تأكيد كلمة المرور الجديدة | Label |
| Auth_ConfirmNewPasswordPlaceholder | Confirm your new password | أكد كلمة المرور الجديدة | Placeholder |
| Auth_PasswordEncryptedSecure | Your password is encrypted and stored securely | كلمة المرور الخاصة بك مشفرة ومخزنة بشكل آمن | Footer text |

---

## 4. ForgotPassword.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_ForgotPasswordTitle | Forgot Password | نسيت كلمة المرور | Page title |
| Auth_ForgotPassword | Forgot Password? | نسيت كلمة المرور؟ | Heading |
| Auth_EnterEmailReset | Enter your email address and we'll send you a link to reset your password. | أدخل عنوان بريدك الإلكتروني وسنرسل لك رابطًا لإعادة تعيين كلمة المرور. | Instructions |
| Submit | Submit | إرسال | Button text |

**ViewModel Properties**:
- Email (auto-localized)

---

## 5. ForgotPasswordConfirmation.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_ResetPasswordSentTitle | Password Reset Link Sent | تم إرسال رابط إعادة تعيين كلمة المرور | Page title |
| Auth_ResetPasswordSentSuccess | Password Reset Link Sent! | تم إرسال رابط إعادة تعيين كلمة المرور! | Heading |
| Auth_ResetPasswordSentMessage | We've sent a password reset link to your email address. Please check your inbox. | لقد أرسلنا رابط إعادة تعيين كلمة المرور إلى بريدك الإلكتروني. يرجى التحقق من صندوق الوارد. | Message |
| Auth_ImportantNotes | Important Notes | ملاحظات هامة | Alert heading |
| Auth_CheckInbox | Check your email inbox for the reset link | تحقق من صندوق الوارد الخاص بك للحصول على رابط إعادة التعيين | List item |
| Auth_CheckSpam | If you don't see it, check your spam/junk folder | إذا لم تجده، تحقق من مجلد البريد العشوائي | List item |
| Auth_LinkValid24Hours | The reset link is valid for 24 hours | رابط إعادة التعيين صالح لمدة 24 ساعة | List item |
| Auth_BackToLogin | Back to Login | العودة إلى تسجيل الدخول | Button text |
| Auth_EmailNotReceived | Didn't receive the email? | لم تستلم البريد الإلكتروني؟ | Help text |
| Auth_WaitFewMinutes | Wait a few minutes and | انتظر بضع دقائق و | Help text |
| Auth_TryAgain | try again | حاول مرة أخرى | Link text |

---

## 6. ForgotTenantId.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_ForgotTenantIdTitle | Forgot Tenant ID | نسيت معرف المستأجر | Page title |
| Auth_EnterEmailRetrieveTenantId | Enter your email and password to retrieve your Tenant ID | أدخل بريدك الإلكتروني وكلمة المرور لاسترداد معرف المستأجر | Subtitle |
| Auth_TenantIdFound | Tenant ID Found! | تم العثور على معرف المستأجر! | Success alert |
| Auth_OrganizationName | Organization Name | اسم المنظمة | Label |
| Auth_TenantIdLabel | Tenant ID | معرف المستأجر | Label |
| Auth_Copy | Copy | نسخ | Button text |
| Auth_CopyTenantIdHint | Copy this ID to use during login | انسخ هذا المعرف لاستخدامه أثناء تسجيل الدخول | Hint |
| Auth_TenantSlug | Tenant Slug | عنوان URL للمستأجر | Label |
| Auth_GoToLogin | Go to Login | الذهاب إلى تسجيل الدخول | Button |
| Auth_TenantIdNotFound | Tenant ID Not Found | معرف المستأجر غير موجود | Warning alert |
| Auth_TenantIdNotFoundMessage | No tenant was found with the provided credentials. Please verify your information. | لم يتم العثور على مستأجر بالبيانات المقدمة. يرجى التحقق من معلوماتك. | Error message |
| Auth_EnterEmailTenantAdmin | Enter the email you used when creating your organization | أدخل البريد الإلكتروني الذي استخدمته عند إنشاء مؤسستك | Help text |
| Auth_PasswordPlaceholder | Enter your password | أدخل كلمة المرور | Placeholder |
| Auth_EnterPasswordVerify | Enter your password to verify identity | أدخل كلمة المرور للتحقق من الهوية | Help text |
| Note | Note | ملاحظة | Generic label |
| Auth_CredentialsTemporaryNote | Your credentials are only used temporarily to retrieve your Tenant ID and are not stored. | يتم استخدام بيانات الاعتماد الخاصة بك مؤقتًا فقط لاسترداد معرف المستأجر ولا يتم تخزينها. | Security note |
| Auth_FindTenantId | Find My Tenant ID | ابحث عن معرف المستأجر الخاص بي | Button |
| Auth_BackToTenantAdminLogin | Back to Tenant Admin Login | العودة إلى تسجيل دخول مسؤول المستأجر | Link |
| Copied | Copied! | تم النسخ! | JavaScript message |

**ViewModel Properties**:
- Email
- Password

---

## 7. Lockout.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_AccountLocked | Account Locked | الحساب مقفل | Page title and heading |
| Auth_AccountLockedMessage | Your account has been locked due to too many failed login attempts. | تم قفل حسابك بسبب عدد كبير من محاولات تسجيل الدخول الفاشلة. | Error message |
| Auth_TryAgainLater | Please try again later or contact your administrator. | يرجى المحاولة مرة أخرى لاحقًا أو الاتصال بالمسؤول. | Help text |
| Auth_GoToHome | Go to Home | الذهاب إلى الصفحة الرئيسية | Button |

---

## 8. Login.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_LoginTitle | Login | تسجيل الدخول | Page title and button |
| Auth_LoginToGrc | Login to GRC System | تسجيل الدخول إلى نظام GRC | Heading |
| Auth_EmailPlaceholder | your.email@example.com | your.email@example.com | Placeholder |
| Auth_ForgotTenantId | Forgot your Tenant ID? | نسيت معرف المستأجر؟ | Link |
| Auth_DontHaveAccount | Don't have an account? | ليس لديك حساب؟ | Text |
| Auth_RegisterHere | Register here | سجل هنا | Link |

**ViewModel Properties**:
- Email
- Password
- RememberMe

---

## 9. LoginV2.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_LoginV2Title | Enhanced Login | تسجيل الدخول المحسّن | Page title |
| Auth_LoginEnhancedSecurity | Login with Enhanced Security | تسجيل الدخول بأمان محسّن | Heading |
| Auth_EnhancedSecurityInfo | This login uses enhanced security features including session-based authentication and improved audit logging. | يستخدم تسجيل الدخول هذا ميزات أمان محسّنة بما في ذلك المصادقة القائمة على الجلسة وتسجيل المراجعة المحسّن. | Info alert |
| Auth_LoginEnhanced | Login (Enhanced) | تسجيل الدخول (محسّن) | Button |
| Auth_RegisterAsNewUser | Register as new user | التسجيل كمستخدم جديد | Link |
| Auth_DemoLogin | Demo Login | تسجيل دخول تجريبي | Button |
| Auth_V2Enhancements | V2 Enhancements | تحسينات الإصدار 2 | Heading |
| Auth_SessionBasedAuth | Session-based authentication | المصادقة القائمة على الجلسة | List item |
| Auth_StructuredLogging | Structured logging with context | التسجيل المنظم مع السياق | List item |
| Auth_EnhancedSecurityMonitoring | Enhanced security monitoring | مراقبة أمنية محسّنة | List item |
| Auth_DeterministicTenantResolution | Deterministic tenant resolution | تحليل محدد للمستأجر | List item |
| Auth_SwitchToLegacyLogin | Switch to legacy login | التبديل إلى تسجيل الدخول القديم | Link |

---

## 10. LoginWith2fa.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_TwoFactorAuthentication | Two-Factor Authentication | المصادقة الثنائية | Page title and heading |
| Auth_EnterAuthenticatorCode | Enter the code from your authenticator app. | أدخل الرمز من تطبيق المصادقة الخاص بك. | Instructions |
| Auth_NoAccessToAuthenticator | Don't have access to your authenticator device? | لا يمكنك الوصول إلى جهاز المصادقة؟ | Help text |
| Auth_LoginWithRecoveryCode | Log in with a recovery code | تسجيل الدخول برمز الاسترداد | Link |

**ViewModel Properties**:
- TwoFactorCode
- RememberMachine
- RememberMe (hidden)

---

## 11. LoginWithRecoveryCode.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_RecoveryCodeLogin | Recovery Code Login | تسجيل الدخول برمز الاسترداد | Page title and heading |
| Auth_RecoveryCodeLoginMessage | Enter a recovery code that was generated when you set up two-factor authentication. | أدخل رمز الاسترداد الذي تم إنشاؤه عند إعداد المصادقة الثنائية. | Instructions |

**ViewModel Properties**:
- RecoveryCode

---

## 12. Manage.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_ManageProfile | Manage Profile | إدارة الملف الشخصي | Page title and heading |
| Auth_EmailCannotBeChanged | Email address cannot be changed | لا يمكن تغيير عنوان البريد الإلكتروني | Help text |
| Auth_SaveChanges | Save Changes | حفظ التغييرات | Button |

**ViewModel Properties**:
- Email
- FirstName
- LastName
- Department
- PhoneNumber

---

## 13. Profile.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_MyProfile | My Profile | ملفي الشخصي | Page title and heading |
| Auth_ViewManageAccountSettings | View and manage your account settings and information | عرض وإدارة إعدادات ومعلومات حسابك | Subtitle |
| Auth_QuickActions | Quick Actions | إجراءات سريعة | Section heading |
| Auth_MyInbox | My Inbox | صندوق الوارد الخاص بي | Link |
| Auth_MyTasks | My Tasks | مهامي | Link |
| Auth_AccountSettings | Account Settings | إعدادات الحساب | Link |
| Auth_AssignedProfiles | Assigned Profiles | الأدوار المخصصة | Section heading |
| Auth_NoProfilesAssigned | No profiles assigned to your account. | لا توجد أدوار مخصصة لحسابك. | Empty state |
| Auth_Permissions | Permissions | الصلاحيات | Section heading |
| Auth_More | more | المزيد | Badge text |
| Auth_NoPermissionsAssigned | No permissions assigned to your account. | لا توجد صلاحيات مخصصة لحسابك. | Empty state |
| Auth_WorkflowRoles | Workflow Roles | أدوار سير العمل | Section heading |
| Auth_NoWorkflowRolesAssigned | No workflow roles assigned. | لا توجد أدوار سير عمل مخصصة. | Empty state |
| Auth_NotificationPreferences | Notification Preferences | تفضيلات الإشعارات | Section heading |
| Edit | Edit | تعديل | Button |
| Auth_EmailNotifications | Email Notifications | إشعارات البريد الإلكتروني | Checkbox label |
| Auth_SmsNotifications | SMS Notifications | إشعارات الرسائل القصيرة | Checkbox label |
| Auth_InAppNotifications | In-App Notifications | الإشعارات داخل التطبيق | Checkbox label |
| Auth_Digest | Digest Frequency | تكرار الملخص | Label |
| Auth_Immediate | Immediate | فوري | Option |
| Auth_Daily | Daily | يومي | Option |
| Auth_Weekly | Weekly | أسبوعي | Option |
| Cancel | Cancel | إلغاء | Button |

---

## 14. Register.cshtml

**Status**: ⚠️ Needs conversion (1 hardcoded string)

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_RegisterTitle | Register | التسجيل | Page title |
| Auth_CreateNewAccount | Create New Account | إنشاء حساب جديد | Heading |
| Auth_FirstNamePlaceholder | Enter your first name | أدخل اسمك الأول | Placeholder |
| Auth_LastNamePlaceholder | Enter your last name | أدخل اسمك الأخير | Placeholder |
| Auth_DepartmentPlaceholder | e.g., Finance, IT, Legal | مثال: المالية، تقنية المعلومات، القانونية | Placeholder |
| Auth_PasswordCreatePlaceholder | Create a strong password | أنشئ كلمة مرور قوية | Placeholder |
| **Auth_PasswordRequirements** | **Password must be at least 6 characters long and contain uppercase, lowercase, numbers, and special characters.** | **يجب أن تتكون كلمة المرور من 6 أحرف على الأقل وتحتوي على أحرف كبيرة وصغيرة وأرقام وأحرف خاصة.** | **Help text (HARDCODED)** |
| Auth_ConfirmPasswordPlaceholder | Re-enter your password | أعد إدخال كلمة المرور | Placeholder |
| Nav_Register | Register | التسجيل | Button (reused from nav) |
| Auth_AlreadyHaveAccount | Already have an account? | هل لديك حساب بالفعل؟ | Text |
| Auth_LoginHere | Login here | سجل الدخول هنا | Link |

**ViewModel Properties**:
- FirstName
- LastName
- Email
- Department
- Password
- ConfirmPassword

**Hardcoded Text to Convert**:
```html
<!-- Line 64-65: Password requirements -->
<small class="form-text text-muted">
    Password must be at least 6 characters long and contain uppercase, lowercase, numbers, and special characters.
</small>
```

---

## 15. ResetPassword.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_ResetPassword | Reset Password | إعادة تعيين كلمة المرور | Page title, heading, button |

**ViewModel Properties**:
- Email
- Password
- ConfirmPassword
- Code (hidden)

---

## 16. ResetPasswordConfirmation.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_PasswordResetSuccessful | Password Reset Successful | تم إعادة تعيين كلمة المرور بنجاح | Page title and heading |
| Auth_PasswordResetSuccessfulMessage | Your password has been successfully reset. You can now log in with your new password. | تم إعادة تعيين كلمة المرور بنجاح. يمكنك الآن تسجيل الدخول بكلمة المرور الجديدة. | Message |
| Auth_UseStrongPassword | Always use a strong, unique password for your account. | استخدم دائمًا كلمة مرور قوية وفريدة لحسابك. | Security tip |
| Auth_LogInToYourAccount | Log In to Your Account | تسجيل الدخول إلى حسابك | Button |
| Auth_RememberPassword | Remember your password? | تتذكر كلمة المرور؟ | Text |
| Auth_GoToLogin | Go to Login | الذهاب إلى تسجيل الدخول | Link |
| Auth_ContactSupport | If you need help, please contact our support team. | إذا كنت بحاجة إلى مساعدة، يرجى الاتصال بفريق الدعم لدينا. | Footer text |

---

## 17. TenantAdminLogin.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_TenantAdminLoginTitle | Tenant Admin Login | تسجيل دخول مسؤول المستأجر | Page title and heading |
| Auth_EnterTenantIdUsernamePassword | Enter your tenant ID, username, and password | أدخل معرف المستأجر واسم المستخدم وكلمة المرور | Subtitle |
| Auth_TenantIdPlaceholder | Enter your tenant ID | أدخل معرف المستأجر | Placeholder |
| Auth_TenantIdProvided | This was provided when your organization was created. | تم توفير هذا عند إنشاء مؤسستك. | Help text |
| Auth_UsernamePlaceholder | Enter your username | أدخل اسم المستخدم | Placeholder |
| Auth_LoginToOnboarding | Login to Onboarding | تسجيل الدخول إلى الإعداد | Button |
| Auth_BackToRegularLogin | Back to Regular Login | العودة إلى تسجيل الدخول العادي | Link |

**ViewModel Properties**:
- TenantId
- Username
- Password

---

## 18. TenantLoginV2.cshtml

**Status**: ⚠️ Needs conversion (4 hardcoded labels)

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_TenantLoginV2Title | Enhanced Tenant Login | تسجيل دخول المستأجر المحسّن | Page title |
| Auth_TenantLoginEnhanced | Enhanced Tenant Login | تسجيل دخول المستأجر المحسّن | Heading |
| Auth_SessionBasedTenantClaims | Session-based tenant claims for better performance and security | مطالبات المستأجر القائمة على الجلسة لأداء وأمان أفضل | Alert |
| Auth_Tenant | Tenant: | المستأجر: | Label |
| **Email** | **Email** | **البريد الإلكتروني** | **Label (HARDCODED - Line 38)** |
| **Password** | **Password** | **كلمة المرور** | **Label (HARDCODED - Line 47)** |
| **Tenant ID** | **Tenant ID** | **معرف المستأجر** | **Label (HARDCODED - Line 56)** |
| Auth_TenantSlugLabel | Tenant Slug (Optional) | عنوان URL للمستأجر (اختياري) | Label |
| Auth_TenantSlugPlaceholder | e.g., acme-corp | مثال: acme-corp | Placeholder |
| Auth_RoleLabel | Role (Optional) | الدور (اختياري) | Label |
| Auth_SelectRole | -- Select a role -- | -- اختر دورًا -- | Dropdown default |
| **Remember Me** | **Remember Me** | **تذكرني** | **Label (HARDCODED - Line 88)** |
| Auth_LoginToTenantEnhanced | Login to Tenant (Enhanced) | تسجيل الدخول إلى المستأجر (محسّن) | Button |
| Auth_RegularLogin | Regular Login | تسجيل الدخول العادي | Link |
| Auth_V2SessionBasedClaims | V2: Session-Based Claims | الإصدار 2: المطالبات القائمة على الجلسة | Heading |
| Auth_TenantContextInSession | Tenant context stored in session | سياق المستأجر مخزن في الجلسة | List item |
| Auth_FixesClaimsBug | Fixes claims not persisting across requests | يصلح عدم استمرار المطالبات عبر الطلبات | List item |
| Auth_FasterTenantSwitching | Faster tenant switching | تبديل أسرع للمستأجر | List item |
| Auth_NoDatabaseOverhead | No database overhead on every request | لا توجد حمولة زائدة على قاعدة البيانات في كل طلب | List item |
| Auth_SwitchToLegacyTenantLogin | Switch to legacy tenant login | التبديل إلى تسجيل دخول المستأجر القديم | Link |

**ViewModel Properties**:
- Email
- Password
- TenantId
- TenantSlug
- Role
- RememberMe
- ReturnUrl (hidden)

**Hardcoded Text to Convert**:
```html
<!-- Line 38: Email label -->
<label asp-for="Email" class="form-label">Email</label>

<!-- Line 47: Password label -->
<label asp-for="Password" class="form-label">Password</label>

<!-- Line 56: Tenant ID label -->
<label asp-for="TenantId" class="form-label">Tenant ID</label>

<!-- Line 88: Remember Me label -->
<label asp-for="RememberMe" class="form-check-label">Remember Me</label>
```

---

## 19. VerifyMfa.cshtml

**Status**: ✅ Fully localized

| Key | English | Arabic | Context |
|-----|---------|--------|---------|
| Auth_VerifyMfa | Verify Multi-Factor Authentication | التحقق من المصادقة متعددة العوامل | Page title and heading |
| Auth_VerificationCodeSent | A verification code has been sent to: | تم إرسال رمز التحقق إلى: | Message |
| Auth_VerificationCode | Verification Code | رمز التحقق | Label |
| Auth_Verify | Verify | تحقق | Button |
| Auth_CodeValidFor | Code valid for: | الرمز صالح لمدة: | Countdown text |
| Auth_ResendCode | Resend Code | إعادة إرسال الرمز | Link |
| Auth_AccountProtectionPriority | Your account security is our priority | أمان حسابك هو أولويتنا | Footer text |
| Auth_Expired | Expired | منتهي الصلاحية | JavaScript message |

**ViewModel Properties**:
- Code
- ReturnUrl (hidden)
- RememberMe (hidden)
- MaskedEmail (display only)

---

## Summary Statistics

### Files Status
- ✅ **Fully Localized**: 17 files
- ⚠️ **Needs Conversion**: 2 files (Register.cshtml, TenantLoginV2.cshtml)

### Total Localization Keys
- **Unique Keys**: ~135 keys across all Account views
- **Keys Needing Addition**: 5 new keys for hardcoded strings

### New Keys Required

| Key | English | Arabic | File |
|-----|---------|--------|------|
| Auth_PasswordRequirements | Password must be at least 6 characters long and contain uppercase, lowercase, numbers, and special characters. | يجب أن تتكون كلمة المرور من 6 أحرف على الأقل وتحتوي على أحرف كبيرة وصغيرة وأرقام وأحرف خاصة. | Register.cshtml |
| Auth_EmailLabel | Email | البريد الإلكتروني | TenantLoginV2.cshtml |
| Auth_PasswordLabel | Password | كلمة المرور | TenantLoginV2.cshtml |
| Auth_TenantIdFieldLabel | Tenant ID | معرف المستأجر | TenantLoginV2.cshtml |
| Auth_RememberMeLabel | Remember Me | تذكرني | TenantLoginV2.cshtml |

---

## Complete Key List (Alphabetical)

All 140 keys used across Account views:

1. Auth_AccessDenied
2. Auth_AccessDeniedMessage
3. Auth_AccountLocked
4. Auth_AccountLockedMessage
5. Auth_AccountProtectionPriority
6. Auth_AccountSettings
7. Auth_AlreadyHaveAccount
8. Auth_AssignedProfiles
9. Auth_BackToProfile
10. Auth_BackToRegularLogin
11. Auth_BackToTenantAdminLogin
12. Auth_BackToLogin
13. Auth_ChangePassword
14. Auth_ChangePasswordRequired
15. Auth_CheckInbox
16. Auth_CheckSpam
17. Auth_CodeValidFor
18. Auth_ConfirmNewPassword
19. Auth_ConfirmNewPasswordPlaceholder
20. Auth_ConfirmPasswordPlaceholder
21. Auth_ContactAdministrator
22. Auth_ContactSupport
23. Auth_Copy
24. Auth_CopyTenantIdHint
25. Auth_CreateNewAccount
26. Auth_CredentialsTemporaryNote
27. Auth_CurrentPassword
28. Auth_Daily
29. Auth_DemoLogin
30. Auth_DepartmentPlaceholder
31. Auth_DeterministicTenantResolution
32. Auth_Digest
33. Auth_DontHaveAccount
34. Auth_EmailCannotBeChanged
35. Auth_EmailLabel (NEW)
36. Auth_EmailNotReceived
37. Auth_EmailNotifications
38. Auth_EmailPlaceholder
39. Auth_EnhancedSecurityInfo
40. Auth_EnhancedSecurityMonitoring
41. Auth_EnterAuthenticatorCode
42. Auth_EnterCurrentPassword
43. Auth_EnterEmailReset
44. Auth_EnterEmailRetrieveTenantId
45. Auth_EnterEmailTenantAdmin
46. Auth_EnterNewPassword
47. Auth_EnterPasswordVerify
48. Auth_EnterTenantIdUsernamePassword
49. Auth_Expired
50. Auth_FasterTenantSwitching
51. Auth_FindTenantId
52. Auth_FirstNamePlaceholder
53. Auth_FixesClaimsBug
54. Auth_ForgotPassword
55. Auth_ForgotPasswordTitle
56. Auth_ForgotTenantId
57. Auth_ForgotTenantIdTitle
58. Auth_GoToDashboard
59. Auth_GoToHome
60. Auth_GoToLogin
61. Auth_Immediate
62. Auth_ImportantNotes
63. Auth_InAppNotifications
64. Auth_LastNamePlaceholder
65. Auth_LinkValid24Hours
66. Auth_LoginEnhanced
67. Auth_LoginEnhancedSecurity
68. Auth_LoginHere
69. Auth_LoginTitle
70. Auth_LoginToGrc
71. Auth_LoginToOnboarding
72. Auth_LoginToTenantEnhanced
73. Auth_LoginV2Title
74. Auth_LoginWithRecoveryCode
75. Auth_LogInToYourAccount
76. Auth_ManageProfile
77. Auth_More
78. Auth_MustChangePassword
79. Auth_MyInbox
80. Auth_MyProfile
81. Auth_MyTasks
82. Auth_NewPassword
83. Auth_NoAccessToAuthenticator
84. Auth_NoDatabaseOverhead
85. Auth_NoPermissionsAssigned
86. Auth_NoProfilesAssigned
87. Auth_NoWorkflowRolesAssigned
88. Auth_NotificationPreferences
89. Auth_OrganizationName
90. Auth_PasswordChangeRequired
91. Auth_PasswordCreatePlaceholder
92. Auth_PasswordEncryptedSecure
93. Auth_PasswordLabel (NEW)
94. Auth_PasswordMin8Chars
95. Auth_PasswordMinLengthHint
96. Auth_PasswordPlaceholder
97. Auth_PasswordRequirements (NEW)
98. Auth_PasswordResetSuccessful
99. Auth_PasswordResetSuccessfulMessage
100. Auth_Permissions
101. Auth_QuickActions
102. Auth_RecoveryCodeLogin
103. Auth_RecoveryCodeLoginMessage
104. Auth_RegisterAsNewUser
105. Auth_RegisterHere
106. Auth_RegisterTitle
107. Auth_RegularLogin
108. Auth_RememberMeLabel (NEW)
109. Auth_RememberPassword
110. Auth_ResendCode
111. Auth_ResetPassword
112. Auth_ResetPasswordSentMessage
113. Auth_ResetPasswordSentSuccess
114. Auth_ResetPasswordSentTitle
115. Auth_SaveChanges
116. Auth_SelectRole
117. Auth_SessionBasedAuth
118. Auth_SessionBasedTenantClaims
119. Auth_SmsNotifications
120. Auth_StructuredLogging
121. Auth_SwitchToLegacyLogin
122. Auth_SwitchToLegacyTenantLogin
123. Auth_Tenant
124. Auth_TenantAdminLoginTitle
125. Auth_TenantContextInSession
126. Auth_TenantIdFieldLabel (NEW)
127. Auth_TenantIdFound
128. Auth_TenantIdLabel
129. Auth_TenantIdNotFound
130. Auth_TenantIdNotFoundMessage
131. Auth_TenantIdPlaceholder
132. Auth_TenantIdProvided
133. Auth_TenantLoginEnhanced
134. Auth_TenantLoginV2Title
135. Auth_TenantSlug
136. Auth_TenantSlugLabel
137. Auth_TenantSlugPlaceholder
138. Auth_TryAgain
139. Auth_TryAgainLater
140. Auth_TwoFactorAuthentication
141. Auth_UseStrongPassword
142. Auth_UsernamePlaceholder
143. Auth_V2Enhancements
144. Auth_V2SessionBasedClaims
145. Auth_VerificationCode
146. Auth_VerificationCodeSent
147. Auth_Verify
148. Auth_VerifyMfa
149. Auth_ViewManageAccountSettings
150. Auth_WaitFewMinutes
151. Auth_Weekly
152. Auth_WorkflowRoles
153. Cancel
154. Copied (JavaScript)
155. Edit
156. Note
157. Nav_Register
158. Submit

---

## Implementation Notes

### 1. Pattern Consistency
All keys use the `Auth_` prefix to indicate authentication/account-related functionality.

### 2. Reused Keys
Some keys are shared across multiple views:
- `Auth_ChangePassword` (ChangePassword.cshtml, ChangePasswordRequired.cshtml, Manage.cshtml, Profile.cshtml)
- `Auth_ForgotPassword` (Login.cshtml, LoginV2.cshtml, TenantLoginV2.cshtml)
- `Auth_LoginTitle` (Login.cshtml, LoginWith2fa.cshtml, LoginWithRecoveryCode.cshtml)
- `Auth_PasswordPlaceholder` (ForgotTenantId.cshtml, Login.cshtml, LoginV2.cshtml, TenantAdminLogin.cshtml, TenantLoginV2.cshtml)

### 3. JavaScript Localization
Two views use client-side localization:
- **ForgotTenantId.cshtml**: `window.L('Copied')`
- **VerifyMfa.cshtml**: `window.L('Auth_Expired')`

### 4. ViewModel Property Labels
All form fields use Display attributes in ViewModels for automatic localization. Fields include:
- Email, Password, ConfirmPassword
- FirstName, LastName, Department, PhoneNumber
- TenantId, Username, TenantSlug, Role
- TwoFactorCode, RecoveryCode, Code
- RememberMe, RememberMachine
- CurrentPassword, NewPassword

### 5. Conversion Priorities

**High Priority** (user-facing text):
1. Register.cshtml - Password requirements
2. TenantLoginV2.cshtml - Form labels

**Already Complete**:
- All page titles
- All button text
- All alert messages
- All help text and hints

---

## Files Requiring Edits

### Register.cshtml
**Line 63-65**: Replace hardcoded password requirements
```diff
- <small class="form-text text-muted">
-     Password must be at least 6 characters long and contain uppercase, lowercase, numbers, and special characters.
- </small>
+ <small class="form-text text-muted">
+     @L["Auth_PasswordRequirements"]
+ </small>
```

### TenantLoginV2.cshtml
**Multiple lines**: Replace hardcoded labels

```diff
- <label asp-for="Email" class="form-label">Email</label>
+ <label asp-for="Email" class="form-label">@L["Auth_EmailLabel"]</label>

- <label asp-for="Password" class="form-label">Password</label>
+ <label asp-for="Password" class="form-label">@L["Auth_PasswordLabel"]</label>

- <label asp-for="TenantId" class="form-label">Tenant ID</label>
+ <label asp-for="TenantId" class="form-label">@L["Auth_TenantIdFieldLabel"]</label>

- <label asp-for="RememberMe" class="form-check-label">Remember Me</label>
+ <label asp-for="RememberMe" class="form-check-label">@L["Auth_RememberMeLabel"]</label>
```

---

## Testing Checklist

### Manual Testing Required
- [ ] All 19 Account views render correctly in English
- [ ] All 19 Account views render correctly in Arabic
- [ ] Form validation messages display in correct language
- [ ] Success/error alerts display in correct language
- [ ] JavaScript messages (Copied, Expired) work in both languages
- [ ] RTL layout works correctly for Arabic
- [ ] All placeholders render correctly
- [ ] Dropdown options localized properly

### Browser Testing
- [ ] Chrome (English & Arabic)
- [ ] Firefox (English & Arabic)
- [ ] Edge (English & Arabic)
- [ ] Mobile Safari (iOS)
- [ ] Chrome Mobile (Android)

### Flow Testing
- [ ] Complete registration flow
- [ ] Login flow (regular, V2, tenant, tenant V2)
- [ ] Forgot password flow
- [ ] Forgot tenant ID flow
- [ ] Change password flow
- [ ] 2FA login flow
- [ ] Recovery code login flow
- [ ] Profile management flow
- [ ] Notification preferences update

---

## Related Documentation

- **SharedResource.en.resx**: English resource file (to be updated in next batch)
- **SharedResource.ar.resx**: Arabic resource file (to be updated in next batch)
- **ViewModels**: Located in `src/GrcMvc/Models/ViewModels/`
- **Controllers**: `AccountController.cs`, `AccountV2Controller.cs`

---

**Document Version**: 1.0
**Last Updated**: 2026-01-10
**Total Views**: 19
**Total Keys**: 158 (including new keys)
**Status**: Ready for .resx file update
