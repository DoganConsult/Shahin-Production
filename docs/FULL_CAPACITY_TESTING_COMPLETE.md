# Full Capacity Testing - Complete Setup

## ✅ All Issues Fixed & Migration Applied

### 1. Build Errors Fixed ✅
- ✅ `PlatformAdmin.IsActive` property added
- ✅ Razor view error in `Support/Submit.cshtml` fixed
- ✅ `EndpointMonitoringService` errors fixed (ErrorRate calculation, type conversion)
- ✅ `SectorTestController` created and compiled successfully

### 2. Database Migration ✅
- ✅ Migration `AddGrcMainSectorsTable` created
- ✅ Migration applied to database successfully
- ✅ Tables created:
  - `GrcMainSectors` (18 main sectors)
  - `GrcSubSectorMappings` updated with `MainSectorId` foreign key

### 3. Seed Data Ready ✅
- ✅ `GrcMainSectorSeeds.cs` - Seeds 18 main sectors
- ✅ `GosiSectorSeeds.cs` - Seeds ~70 sub-sectors with foreign key linking
- ✅ `SeedController.cs` - Updated to seed main sectors before sub-sectors

### 4. Test Endpoints Created ✅
- ✅ `SectorTestController` - Full testing API endpoints

---

## 🧪 Testing Endpoints

### Base URL: `/api/test/sectors`

All endpoints require `PlatformAdmin` role authentication.

### 1. Seed and Verify (Full Test)
**POST** `/api/test/sectors/seed-and-verify`

Seeds all sector data and verifies relationships in one call.

**Response:**
```json
{
  "success": true,
  "message": "Sector seeding and verification completed",
  "results": {
    "mainSectorsSeeded": true,
    "subSectorsSeeded": true,
    "foreignKeysLinked": true,
    "mainSectorCount": 18,
    "subSectorCount": 70,
    "linkedSubSectorCount": 70,
    "errors": []
  }
}
```

### 2. Verify Current State
**GET** `/api/test/sectors/verify`

Verifies current sector data without seeding.

**Response:**
```json
{
  "success": true,
  "summary": {
    "mainSectorCount": 18,
    "subSectorCount": 70,
    "linkedSubSectorCount": 70,
    "unlinkedSubSectorCount": 0,
    "allLinked": true
  },
  "sectorStats": [...],
  "unlinkedSubSectors": []
}
```

### 3. Get All Main Sectors
**GET** `/api/test/sectors/main-sectors`

Returns all 18 main sectors with their sub-sectors.

### 4. Get Sub-Sectors for a Sector
**GET** `/api/test/sectors/sub-sectors/{sectorCode}`

**Example:** `GET /api/test/sectors/sub-sectors/BANKING`

### 5. Fix Foreign Keys
**POST** `/api/test/sectors/fix-foreign-keys`

Links any unlinked sub-sectors to their main sectors.

---

## 🚀 Quick Test Commands

### Option 1: Using Test Controller (Recommended)
```bash
# Full test - seed and verify
POST http://localhost:5010/api/test/sectors/seed-and-verify

# Verify current state
GET http://localhost:5010/api/test/sectors/verify

# Get all main sectors
GET http://localhost:5010/api/test/sectors/main-sectors

# Get sub-sectors for Banking
GET http://localhost:5010/api/test/sectors/sub-sectors/BANKING
```

### Option 2: Using SeedController
```bash
# Seed all data (includes sectors)
POST /api/seed/all

# Seed only sectors
POST /api/seed/sectors
```

---

## ✅ Expected Results

### Main Sectors Table
- **Count:** 18 sectors
- **All Active:** Yes
- **All Sectors:**
  1. BANKING
  2. HEALTHCARE
  3. GOVERNMENT
  4. TELECOM
  5. ENERGY
  6. RETAIL
  7. TECHNOLOGY
  8. INSURANCE
  9. EDUCATION
  10. TRANSPORTATION
  11. CONSTRUCTION
  12. MANUFACTURING
  13. REAL_ESTATE
  14. HOSPITALITY
  15. MEDIA
  16. AGRICULTURE
  17. MINING
  18. PROFESSIONAL_SERVICES

### Sub-Sectors Table
- **Count:** ~70 sub-sectors
- **All Linked:** Every sub-sector has `MainSectorId` set
- **All Active:** All sub-sectors are active by default

### Relationships
- ✅ One-to-Many: Each main sector has multiple sub-sectors
- ✅ Foreign Keys: All `MainSectorId` values are valid GUIDs
- ✅ Navigation: `MainSector.SubSectors` returns all related sub-sectors
- ✅ Indexes: Performance indexes created for fast queries

---

## 📊 Database Verification Queries

### Count Main Sectors
```sql
SELECT COUNT(*) FROM "GrcMainSectors" WHERE "IsActive" = true;
-- Expected: 18
```

### Count Sub-Sectors
```sql
SELECT COUNT(*) FROM "GrcSubSectorMappings" WHERE "IsActive" = true;
-- Expected: ~70
```

### Count Linked Sub-Sectors
```sql
SELECT COUNT(*) FROM "GrcSubSectorMappings" 
WHERE "MainSectorId" IS NOT NULL;
-- Expected: ~70 (all should be linked)
```

### Get Main Sector with Sub-Sector Count
```sql
SELECT 
    m."SectorCode",
    m."SectorNameEn",
    COUNT(s."Id") as SubSectorCount
FROM "GrcMainSectors" m
LEFT JOIN "GrcSubSectorMappings" s ON s."MainSectorCode" = m."SectorCode"
WHERE m."IsActive" = true
GROUP BY m."SectorCode", m."SectorNameEn"
ORDER BY m."DisplayOrder";
```

---

## 🎯 Full Capacity Test Checklist

- [x] Migration created and applied
- [x] Main sectors table created
- [x] Sub-sectors table updated with foreign key
- [x] Seed data files created
- [x] Test endpoints created
- [x] Build succeeds
- [ ] **Run seed-and-verify endpoint** ← Next step
- [ ] Verify 18 main sectors seeded
- [ ] Verify ~70 sub-sectors seeded
- [ ] Verify all foreign keys linked
- [ ] Test navigation properties
- [ ] Test filtering by sector

---

## 📝 Files Created/Modified

### New Files
- `Models/Entities/GrcMainSector.cs` - Main sector entity
- `Data/Seeds/GrcMainSectorSeeds.cs` - Main sector seed data
- `Controllers/Api/SectorTestController.cs` - Test endpoints
- `docs/SECTOR_TESTING_GUIDE.md` - Testing documentation
- `docs/FULL_CAPACITY_TESTING_COMPLETE.md` - This file

### Modified Files
- `Models/Entities/GrcSubSectorMapping.cs` - Added foreign key
- `Data/GrcDbContext.cs` - Added DbSet and entity configuration
- `Data/Seeds/GosiSectorSeeds.cs` - Added foreign key linking logic
- `Controllers/Api/SeedController.cs` - Added main sector seeding
- `Models/Entities/PlatformAdmin.cs` - Added IsActive property
- `Views/Support/Submit.cshtml` - Fixed Razor syntax
- `Services/Implementations/EndpointMonitoringService.cs` - Fixed errors

---

## 🚀 Next Steps

1. **Start the application:**
   ```bash
   cd src/GrcMvc
   dotnet run
   ```

2. **Run full test:**
   ```bash
   POST http://localhost:5010/api/test/sectors/seed-and-verify
   ```

3. **Verify results:**
   ```bash
   GET http://localhost:5010/api/test/sectors/verify
   ```

4. **Check main sectors:**
   ```bash
   GET http://localhost:5010/api/test/sectors/main-sectors
   ```

---

**Status:** ✅ **READY FOR FULL CAPACITY TESTING**

All code is complete, migration is applied, and test endpoints are ready. You can now test the full capacity of the sector tables system!

---

**Last Updated:** 2026-01-12
