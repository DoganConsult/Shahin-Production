# 🚀 Quick Deploy: Landing Page to Production

## One-Command Deployment

### Option 1: Vercel (Recommended)
```bash
cd shahin-ai-website
npm install
npx vercel --prod
```

### Option 2: Netlify
```bash
cd shahin-ai-website
npm install
npx netlify deploy --prod
```

### Option 3: Using Deploy Script
```bash
./scripts/deploy-landing-page.sh
```

---

## Post-Deployment Verification

```bash
./scripts/verify-production-deployment.sh
```

---

## Files Modified for Production

| File | Change |
|------|--------|
| `shahin-ai-website/components/sections/OnboardingQuestionnaire.tsx` | Added real API integration to `/api/Landing/StartTrial` |
| `shahin-ai-website/netlify.toml` | Configured build, redirects, and security headers |
| `shahin-ai-website/.env.example` | Environment configuration template |
| `src/GrcMvc/appsettings.json` | Added CORS origins for `shahin-ai.com` |
| `src/GrcMvc/Controllers/LandingController.cs` | Fixed `StartTrial` endpoint for cross-origin requests |

---

## Domain Configuration

| Domain | Points To |
|--------|-----------|
| `shahin-ai.com` | Landing page (Vercel/Netlify) |
| `app.shahin-ai.com` | GrcMvc application |

---

## Complete User Journey

```
1. Visit shahin-ai.com
2. Fill OnboardingQuestionnaire (5 sections)
3. Submit → POST /api/Landing/StartTrial
4. Redirect to /grc-free-trial
5. Complete registration
6. Activation email sent
7. Click activation link
8. Login → Onboarding wizard
9. OrgProfile → ReviewScope → CreatePlan
10. Auto-provisioning (teams, RACI)
11. Dashboard access → Full GRC platform
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| CORS error | Check `appsettings.json` has `shahin-ai.com` in `Cors.AllowedOrigins` |
| 400 Bad Request | Ensure `[IgnoreAntiforgeryToken]` on `StartTrial` endpoint |
| Redirect fails | Check `netlify.toml` redirects configuration |
| Build fails | Run `npm install` first, check Node.js version 18+ |

---

**Full documentation:** `DEPLOYMENT_GUIDE_LANDING_TO_PRODUCTION.md`
