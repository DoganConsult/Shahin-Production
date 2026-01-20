# Migration and Seeding Steps for GrcMainSectors Table

## ✅ Completed

1. ✅ Created `GrcMainSector` entity class
2. ✅ Updated `GrcSubSectorMapping` with foreign key relationship
3. ✅ Added entity configuration in `GrcDbContext`
4. ✅ Created seed file `GrcMainSectorSeeds.cs` with 18 main sectors
5. ✅ Updated `GosiSectorSeeds.cs` to link foreign keys
6. ✅ Updated `SeedController.cs` to call main sector seed before sub-sector seed

## ⚠️ Pending (Build Errors Must Be Fixed First)

The migration cannot be created until build errors are resolved:

### Current Build Errors:
1. `PlatformAdmin.IsActive` property missing (multiple files)
2. Razor view error in `Support/Submit.cshtml`

### Once Build Succeeds:

1. **Create Migration:**
   ```bash
   cd src/GrcMvc
   dotnet ef migrations add AddGrcMainSectorsTable
   ```

2. **Apply Migration:**
   ```bash
   dotnet ef database update
   ```

3. **Seed Data:**
   - Main sectors will be seeded automatically when calling `/api/seed/all` or `/api/seed/sectors`
   - Or manually via SeedController endpoints

## 📋 Migration Will Create

### GrcMainSectors Table
- Primary key: `Id` (Guid)
- Unique index: `IX_GrcMainSector_Code` on `SectorCode`
- Indexes: `IsActive`, `DisplayOrder`
- 18 rows (one per main sector)

### GrcSubSectorMappings Table Updates
- New column: `MainSectorId` (Guid?, nullable)
- Foreign key: `MainSectorId` → `GrcMainSectors.Id`
- Index: `IX_GrcSubSectorMapping_MainSectorId`

## 🔄 Seeding Order

1. **Main Sectors** (GrcMainSectorSeeds) - Creates 18 main sectors
2. **Sub-Sector Mappings** (GosiSectorSeeds) - Creates ~70 sub-sectors and links to main sectors

## 📝 Notes

- Foreign key relationship is optional (`MainSectorId` can be null) for backward compatibility
- Main sector names are denormalized in `GrcSubSectorMappings` for performance
- Seed methods check if data exists before seeding (idempotent)

---

**Status:** Ready for migration once build errors are resolved
