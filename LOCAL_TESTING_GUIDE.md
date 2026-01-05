# Local Build, Deploy, and Testing Guide

## ✅ Build Status
- **Build**: ✅ Successful (with expected warnings)
- **Database**: Starting...
- **Application**: Starting on http://localhost:5137

## 🚀 Quick Start

### 1. Start Database (if not running)
```bash
cd /home/dogan/grc-system
docker-compose -f docker-compose.grcmvc.yml up -d db
```

### 2. Run Application
```bash
cd /home/dogan/grc-system/src/GrcMvc
export PATH="$PATH:/usr/share/dotnet:$HOME/.dotnet/tools"
dotnet run --project GrcMvc.csproj --urls "http://localhost:5137"
```

### 3. Access Application
- **URL**: http://localhost:5137
- **Health Check**: http://localhost:5137/health/live

## 🧪 Testing Checklist

### RTL/Localization Testing

#### 1. Language Switching
- [ ] Open http://localhost:5137
- [ ] Check default language (should be Arabic/RTL)
- [ ] Click language switcher in navbar
- [ ] Switch to English - verify LTR direction
- [ ] Switch back to Arabic - verify RTL direction
- [ ] Refresh page - verify language persists

#### 2. RTL Layout Testing (Arabic Mode)
- [ ] Navigate to Dashboard - verify RTL alignment
- [ ] Navigate to Reports - verify RTL alignment
- [ ] Navigate to Risks - verify RTL alignment
- [ ] Navigate to Assessments - verify RTL alignment
- [ ] Navigate to Audits - verify RTL alignment
- [ ] Navigate to Evidence - verify RTL alignment
- [ ] Navigate to Policies - verify RTL alignment
- [ ] Navigate to Workflows - verify RTL alignment
- [ ] Navigate to Inbox - verify RTL alignment
- [ ] Navigate to Controls - verify RTL alignment
- [ ] Navigate to Approvals - verify RTL alignment

#### 3. Shared Components Testing
- [ ] Open any page with LoadingSpinner - verify "جاري التحميل..." (Arabic) or "Loading..." (English)
- [ ] Open any modal - verify "إغلاق" (Close) button in Arabic
- [ ] Open any confirm dialog - verify "تأكيد" (Confirm) and "إلغاء" (Cancel) buttons
- [ ] Check status badges - verify localized status text

#### 4. Form Validation Testing
- [ ] Navigate to Reports > Create
- [ ] Try to submit empty form - verify localized validation messages
- [ ] Check field labels are localized
- [ ] Test in both Arabic and English

#### 5. API Testing
- [ ] Test API endpoints with Postman/curl
- [ ] Verify error messages are localized
- [ ] Test in both Arabic and English

#### 6. Date/Number Formatting
- [ ] Check date displays in Reports pages
- [ ] Verify dates format correctly in Arabic (Arabic calendar format)
- [ ] Verify dates format correctly in English (English format)
- [ ] Check number formatting in both languages

### Functional Testing

#### 7. Core Features
- [ ] Login/Logout
- [ ] Dashboard loads correctly
- [ ] Reports listing works
- [ ] Create new report
- [ ] View report details
- [ ] Download report (if available)

#### 8. Navigation
- [ ] All menu items work
- [ ] Breadcrumbs display correctly
- [ ] Back buttons work
- [ ] Links navigate correctly

#### 9. Responsive Design
- [ ] Test on desktop (1920x1080)
- [ ] Test on tablet (768x1024)
- [ ] Test on mobile (375x667)
- [ ] Verify RTL works on all screen sizes

## 🔍 Debugging

### Check Application Logs
```bash
# View application logs
docker-compose -f docker-compose.grcmvc.yml logs -f grcmvc

# Or if running directly
tail -f /app/logs/grcmvc-*.log
```

### Check Database Connection
```bash
docker-compose -f docker-compose.grcmvc.yml exec db psql -U postgres -d GrcMvcDb -c "SELECT 1;"
```

### Check Health Endpoints
```bash
# Live health check
curl http://localhost:5137/health/live

# Ready health check
curl http://localhost:5137/health/ready
```

## 🐛 Common Issues

### Issue: Application won't start
**Solution**: Check database is running and connection string is correct

### Issue: Language not switching
**Solution**: Clear browser cookies for localhost:5137 and try again

### Issue: RTL not working
**Solution**: 
1. Check browser console for errors
2. Verify Bootstrap RTL CSS is loading
3. Check `rtl.css` is loaded

### Issue: Localized strings not showing
**Solution**:
1. Verify resource files exist in `src/GrcMvc/Resources/`
2. Check browser console for errors
3. Verify `IStringLocalizer` is injected correctly

## 📊 Expected Results

### Arabic (Default)
- ✅ Page direction: RTL
- ✅ Text alignment: Right
- ✅ Bootstrap RTL CSS loaded
- ✅ All buttons/text in Arabic
- ✅ Dates in Arabic format

### English
- ✅ Page direction: LTR
- ✅ Text alignment: Left
- ✅ Bootstrap regular CSS loaded
- ✅ All buttons/text in English
- ✅ Dates in English format

## 🎯 Success Criteria

All tests should pass:
- ✅ Language switching works
- ✅ RTL/LTR direction changes correctly
- ✅ All pages display correctly in both languages
- ✅ Shared components are localized
- ✅ Form validation messages are localized
- ✅ API error messages are localized
- ✅ Dates format correctly for each locale
- ✅ Language preference persists across sessions
