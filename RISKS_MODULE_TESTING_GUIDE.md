# Risks Module Integration - Testing Guide

## ✅ Implementation Complete

All phases have been completed:
- ✅ Phase 1: DTO Mapper created
- ✅ Phase 2: Risk entity extended
- ✅ Phase 3: Database migration created
- ✅ Phase 4: AutoMapper updated
- ✅ Phase 5: Index.razor integrated
- ✅ Phase 6: Create.razor integrated
- ✅ Phase 7: Edit.razor integrated
- ✅ Phase 8: Error handling component created

---

## Step 1: Apply Database Migration

### Option A: Using EF Core CLI (Recommended)

```bash
cd /home/dogan/grc-system

# Apply the migration
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --startup-project src/GrcMvc/GrcMvc.csproj --context GrcDbContext

# Expected output:
# Applying migration '20250122_AddRiskUIFields'.
# Done.
```

### Option B: Manual SQL (If EF Core fails)

Connect to your PostgreSQL database and run:

```sql
-- Add new columns to Risks table
ALTER TABLE "Risks" ADD COLUMN "Title" text NULL;
ALTER TABLE "Risks" ADD COLUMN "RiskNumber" text NULL;
ALTER TABLE "Risks" ADD COLUMN "IdentifiedDate" timestamp with time zone NULL;
ALTER TABLE "Risks" ADD COLUMN "ResponsibleParty" text NULL;
ALTER TABLE "Risks" ADD COLUMN "ConsequenceArea" text NULL;
```

### Verify Migration

```sql
-- Check if columns were added
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'Risks'
AND column_name IN ('Title', 'RiskNumber', 'IdentifiedDate', 'ResponsibleParty', 'ConsequenceArea');
```

Expected result: 5 rows returned with all columns showing `is_nullable = 'YES'`

---

## Step 2: Fix Build Errors (If Any)

Before running the application, fix any build errors:

```bash
cd /home/dogan/grc-system/src/GrcMvc
dotnet build GrcMvc.csproj
```

**Known Issues to Fix:**
1. `AccountController.cs` - Missing `INotificationService` interface
2. `NotificationService.cs` - Ambiguous `UserNotificationPreference` reference
3. `Profile.cshtml` - Tag helper syntax errors

These are unrelated to Risks module but need to be fixed for the app to run.

---

## Step 3: Start the Application

```bash
cd /home/dogan/grc-system/src/GrcMvc
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5137
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Application URLs:**
- HTTP: http://localhost:5137
- HTTPS: https://localhost:7160 (if configured)

---

## Step 4: Test Risks Module

### Test 1: View Risks List

1. **Navigate to:** http://localhost:5137/risks
2. **Expected Behavior:**
   - Page loads without errors
   - Risks table displays (may be empty if no data)
   - Summary cards show counts (Critical, High, Medium, Low)
   - Filter dropdowns are visible
   - "Register New Risk" button is visible

3. **Verify:**
   - ✅ No console errors
   - ✅ Loading spinner appears then disappears
   - ✅ If errors occur, ErrorAlert component displays message

### Test 2: Create a Risk

1. **Navigate to:** http://localhost:5137/risks/create
2. **Fill in the form:**
   ```
   Title: "Test Risk - Data Breach Prevention"
   Category: "Compliance"
   Description: "Risk of unauthorized access to customer data"
   Inherent Score: 20
   Residual Score: 15
   Impact: "High"
   Likelihood: "Medium"
   Responsible Party: "Security Team"
   Owner: "John Doe"
   Identified Date: [Today's date]
   Target Closure Date: [3 months from today]
   Consequence Area: "Data Loss, Regulatory Penalty"
   Status: "Open"
   ```

3. **Click:** "Register Risk" button

4. **Expected Behavior:**
   - Form submits
   - Loading spinner appears
   - Redirects to /risks page
   - New risk appears in the list
   - Risk number is auto-generated (format: RISK-XXXXXXXX)

5. **Verify:**
   - ✅ Risk appears in list
   - ✅ Risk number is generated
   - ✅ All fields are saved correctly
   - ✅ No error messages

### Test 3: Edit a Risk

1. **From Risks list, click:** "Edit" on any risk
2. **Modify fields:**
   ```
   Title: "Updated Test Risk"
   Status: "Mitigated"
   Residual Score: 10
   ```

3. **Click:** "Save Changes"

4. **Expected Behavior:**
   - Form submits
   - Redirects to /risks page
   - Updated risk shows changes in list

5. **Verify:**
   - ✅ Changes are saved
   - ✅ Risk appears updated in list
   - ✅ No error messages

### Test 4: Filter Risks

1. **On Risks list page:**
   - Select "Open" from "Filter by Status" dropdown
   - **Verify:** Only Open risks are displayed

2. **Select "High" from "Filter by Rating" dropdown**
   - **Verify:** Only High-rated risks are displayed

3. **Change "Sort By" to "Risk Score (High)"**
   - **Verify:** Risks are sorted by Residual Score (descending)

4. **Click "Reset Filters"**
   - **Verify:** All risks are shown, filters cleared

5. **Verify:**
   - ✅ Filtering works correctly
   - ✅ Sorting works correctly
   - ✅ Reset clears all filters

### Test 5: Error Handling

#### Test 5a: Create Risk with Missing Fields

1. **Navigate to:** /risks/create
2. **Leave required fields empty:**
   - Title: (empty)
   - Category: (empty)
3. **Click:** "Register Risk"
4. **Expected:**
   - ErrorAlert component displays error message
   - Form does not submit
   - Error message: "Risk title is required" or "Risk category is required"

#### Test 5b: Edit Non-Existent Risk

1. **Navigate to:** /risks/00000000-0000-0000-0000-000000000000/edit
2. **Expected:**
   - ErrorAlert component displays: "Error loading risk: Risk with ID ... not found"
   - "Back to Risk Register" button is visible

#### Test 5c: Network Error Simulation

1. **Stop the database** (simulate connection error)
2. **Try to load risks list**
3. **Expected:**
   - ErrorAlert displays error message
   - Page doesn't crash
   - User-friendly error message shown

---

## Step 5: Database Verification

### Check Migration Applied

```sql
-- Connect to database
psql -U postgres -d GrcMvcDb

-- Check columns
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'Risks'
AND column_name IN ('Title', 'RiskNumber', 'IdentifiedDate', 'ResponsibleParty', 'ConsequenceArea')
ORDER BY column_name;
```

**Expected Output:**
```
    column_name     |          data_type          | is_nullable
--------------------+----------------------------+-------------
 ConsequenceArea    | text                       | YES
 IdentifiedDate     | timestamp with time zone    | YES
 ResponsibleParty  | text                       | YES
 RiskNumber         | text                       | YES
 Title              | text                       | YES
```

### Check Data Integrity

```sql
-- Verify risks can be created with new fields
SELECT 
    "Id",
    "Name",
    "Title",
    "RiskNumber",
    "IdentifiedDate",
    "ResponsibleParty",
    "ConsequenceArea",
    "Status"
FROM "Risks"
ORDER BY "CreatedDate" DESC
LIMIT 5;
```

---

## Step 6: Automated Test Script

Run the automated test script:

```bash
cd /home/dogan/grc-system
./scripts/test-risks-module.sh
```

This script will:
- Check if migration was applied (if database accessible)
- Test HTTP endpoints
- Provide manual testing checklist

---

## Troubleshooting

### Issue: Migration fails with "Build failed"

**Solution:**
1. Fix build errors first:
   ```bash
   dotnet build src/GrcMvc/GrcMvc.csproj
   ```
2. Fix the errors shown
3. Retry migration

### Issue: "More than one project found"

**Solution:**
```bash
cd /home/dogan/grc-system
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --startup-project src/GrcMvc/GrcMvc.csproj
```

### Issue: Risks page shows "Error loading risks"

**Possible Causes:**
1. Database connection issue
2. Service not registered
3. Migration not applied

**Solution:**
1. Check database connection string in `appsettings.json`
2. Verify `IRiskService` is registered in `Program.cs` (line 339)
3. Verify migration was applied

### Issue: Risk number not generated

**Solution:**
- Risk number is generated from Risk ID
- Check if risk was created successfully (has an ID)
- Risk number format: `RISK-XXXXXXXX` (first 8 chars of GUID)

### Issue: Filters not working

**Solution:**
1. Check browser console for JavaScript errors
2. Verify `ApplyFilters()` method is called
3. Check network tab for API calls

---

## Success Criteria

✅ **Migration Applied:**
- All 5 new columns exist in Risks table
- Columns are nullable (backward compatible)

✅ **Create Risk:**
- Risk can be created with all fields
- Risk number is auto-generated
- Redirects to list after creation

✅ **Edit Risk:**
- Risk can be loaded for editing
- Changes are saved
- Redirects to list after update

✅ **Filter Risks:**
- Status filter works
- Rating filter works
- Sort works
- Reset filters works

✅ **Error Handling:**
- ErrorAlert component displays errors
- User-friendly error messages
- No application crashes

---

## Next Steps

After successful testing:

1. **Remove TODO comments** from code (if any remain)
2. **Add logging** for production (ILogger injection)
3. **Add unit tests** for RiskDtoMapper
4. **Add integration tests** for RiskService
5. **Document API endpoints** (if exposing REST API)

---

## Files Modified

- ✅ `src/GrcMvc/Services/Mappers/RiskDtoMapper.cs` (NEW)
- ✅ `src/GrcMvc/Models/Entities/Risk.cs` (MODIFIED)
- ✅ `src/GrcMvc/Migrations/20250122_AddRiskUIFields.cs` (NEW)
- ✅ `src/GrcMvc/Mappings/AutoMapperProfile.cs` (MODIFIED)
- ✅ `src/GrcMvc/Components/Pages/Risks/Index.razor` (MODIFIED)
- ✅ `src/GrcMvc/Components/Pages/Risks/Create.razor` (MODIFIED)
- ✅ `src/GrcMvc/Components/Pages/Risks/Edit.razor` (MODIFIED)
- ✅ `src/GrcMvc/Components/Shared/ErrorAlert.razor` (NEW)

---

**Status:** ✅ **READY FOR TESTING**

Apply migration, start application, and follow the testing steps above.
