# Identity Schema Safeguards

## ⚠️ Critical: Always Use Migrations for GrcAuthDbContext

**NEVER use `EnsureCreated()` for `GrcAuthDbContext`** - it bypasses migrations and can create incomplete schemas.

## ✅ Correct Approach

### 1. Always Use Migrations

The `GrcAuthDbContext` **MUST** use EF Core migrations to ensure the database schema matches the `ApplicationUser` entity exactly.

**Why?**
- `EnsureCreated()` creates tables from the model but doesn't track changes
- It can miss custom columns if the model isn't fully loaded
- Migrations ensure all `ApplicationUser` properties are in the database
- Migrations are versioned and can be rolled back

### 2. Current Configuration

**Program.cs** (lines 1594-1597):
```csharp
// Apply migrations for Auth database (ensures schema matches ApplicationUser entity)
var authContext = services.GetRequiredService<GrcAuthDbContext>();
Console.WriteLine("🔄 Applying Auth database migrations...");
authContext.Database.Migrate();
Console.WriteLine("✅ Auth database migrations applied");
```

**This ensures:**
- All migrations are applied on startup
- Schema always matches the `ApplicationUser` entity
- Custom columns (FirstName, LastName, Abilities, etc.) are present

### 3. Creating New Migrations

When adding new properties to `ApplicationUser`:

```bash
cd Shahin-Jan-2026/src/GrcMvc
dotnet ef migrations add AddNewPropertyToApplicationUser --context GrcAuthDbContext --output-dir Migrations/Auth
dotnet ef database update --context GrcAuthDbContext
```

### 4. Migration Location

- **Path**: `src/GrcMvc/Migrations/Auth/`
- **Context**: `GrcAuthDbContext`
- **Database**: `GrcAuthDb`

### 5. Verification Checklist

Before deploying, verify:

- [ ] All `ApplicationUser` properties have corresponding database columns
- [ ] Migration has been created and applied
- [ ] `Program.cs` uses `Migrate()` not `EnsureCreated()` for `GrcAuthDbContext`
- [ ] Test forms can save/load all `ApplicationUser` properties

### 6. Common Properties in ApplicationUser

Ensure these columns exist in `AspNetUsers`:

**Required Properties:**
- `FirstName` (TEXT NOT NULL)
- `LastName` (TEXT NOT NULL)
- `Department` (TEXT NOT NULL)
- `JobTitle` (TEXT NOT NULL)
- `KsaCompetencyLevel` (INTEGER NOT NULL DEFAULT 3)
- `IsActive` (BOOLEAN NOT NULL DEFAULT true)
- `CreatedDate` (TIMESTAMP WITH TIME ZONE NOT NULL)
- `MustChangePassword` (BOOLEAN NOT NULL DEFAULT true)

**Nullable Properties:**
- `RoleProfileId` (UUID NULL)
- `KnowledgeAreas` (TEXT NULL)
- `Skills` (TEXT NULL)
- `Abilities` (TEXT NULL)
- `AssignedScope` (TEXT NULL)
- `LastLoginDate` (TIMESTAMP WITH TIME ZONE NULL)
- `RefreshToken` (TEXT NULL)
- `RefreshTokenExpiry` (TIMESTAMP WITH TIME ZONE NULL)
- `LastPasswordChangedAt` (TIMESTAMP WITH TIME ZONE NULL)

### 7. Troubleshooting

**Issue**: Missing columns in `AspNetUsers` table

**Solution**:
1. Check if migration exists: `dotnet ef migrations list --context GrcAuthDbContext`
2. Apply migration: `dotnet ef database update --context GrcAuthDbContext`
3. Verify columns: Check database schema matches `ApplicationUser.cs`

**Issue**: `EnsureCreated()` was used instead of migrations

**Solution**:
1. Remove the database or drop the `AspNetUsers` table
2. Apply migrations: `dotnet ef database update --context GrcAuthDbContext`
3. Verify `Program.cs` uses `Migrate()` not `EnsureCreated()`

## 📝 Migration History

- **20260115064458_AddApplicationUserCustomColumns**: Initial migration that creates `AspNetUsers` table with all `ApplicationUser` custom columns

## 🔒 Prevention

To prevent this issue from happening again:

1. **Code Review**: Always check that `GrcAuthDbContext` uses `Migrate()`, not `EnsureCreated()`
2. **CI/CD**: Add a check to verify migrations are applied
3. **Documentation**: Keep this file updated when adding new properties
4. **Testing**: Test user forms after schema changes
