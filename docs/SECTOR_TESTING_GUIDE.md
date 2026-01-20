# Sector Tables Testing Guide

## ✅ Migration Applied

The `AddGrcMainSectorsTable` migration has been successfully applied to the database.

---

## 🧪 Testing Endpoints

### 1. Seed and Verify (Full Test)
**Endpoint:** `POST /api/test/sectors/seed-and-verify`

**Description:** Seeds all sector data and verifies relationships

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
**Endpoint:** `GET /api/test/sectors/verify`

**Description:** Verifies current sector data without seeding

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
**Endpoint:** `GET /api/test/sectors/main-sectors`

**Description:** Returns all main sectors with their sub-sectors

### 4. Get Sub-Sectors for a Sector
**Endpoint:** `GET /api/test/sectors/sub-sectors/{sectorCode}`

**Example:** `GET /api/test/sectors/sub-sectors/BANKING`

### 5. Fix Foreign Keys
**Endpoint:** `POST /api/test/sectors/fix-foreign-keys`

**Description:** Links any unlinked sub-sectors to their main sectors

---

## 🚀 Quick Test Commands

### Using curl (if running locally):
```bash
# Seed and verify
curl -X POST http://localhost:5010/api/test/sectors/seed-and-verify \
  -H "Authorization: Bearer YOUR_TOKEN"

# Verify current state
curl -X GET http://localhost:5010/api/test/sectors/verify \
  -H "Authorization: Bearer YOUR_TOKEN"

# Get all main sectors
curl -X GET http://localhost:5010/api/test/sectors/main-sectors \
  -H "Authorization: Bearer YOUR_TOKEN"

# Get sub-sectors for Banking
curl -X GET http://localhost:5010/api/test/sectors/sub-sectors/BANKING \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Using SeedController (Alternative):
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
- **All sectors:** BANKING, HEALTHCARE, GOVERNMENT, TELECOM, ENERGY, RETAIL, TECHNOLOGY, INSURANCE, EDUCATION, TRANSPORTATION, CONSTRUCTION, MANUFACTURING, REAL_ESTATE, HOSPITALITY, MEDIA, AGRICULTURE, MINING, PROFESSIONAL_SERVICES

### Sub-Sectors Table
- **Count:** ~70 sub-sectors
- **All linked:** Every sub-sector should have `MainSectorId` set
- **All active:** All sub-sectors should be active by default

### Relationships
- **One-to-Many:** Each main sector has multiple sub-sectors
- **Foreign Keys:** All `MainSectorId` values should be valid GUIDs
- **Navigation:** `MainSector.SubSectors` should return all related sub-sectors

---

## 🔍 Verification Checklist

- [x] Migration applied successfully
- [ ] Main sectors seeded (18 sectors)
- [ ] Sub-sectors seeded (~70 sub-sectors)
- [ ] All foreign keys linked (MainSectorId set)
- [ ] Navigation properties work
- [ ] Indexes created for performance
- [ ] Seed methods are idempotent (can run multiple times)

---

## 📊 Database Queries for Manual Verification

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

## 🎯 Full Capacity Test

Run the complete test:

1. **Seed Data:**
   ```bash
   POST /api/test/sectors/seed-and-verify
   ```

2. **Verify Results:**
   ```bash
   GET /api/test/sectors/verify
   ```

3. **Check Main Sectors:**
   ```bash
   GET /api/test/sectors/main-sectors
   ```

4. **Test Specific Sector:**
   ```bash
   GET /api/test/sectors/sub-sectors/BANKING
   ```

All endpoints require `PlatformAdmin` role authentication.

---

**Last Updated:** 2026-01-12
**Status:** ✅ Ready for testing
