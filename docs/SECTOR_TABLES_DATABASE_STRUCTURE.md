# Sector Tables Database Structure

## ✅ Current Status

**Answer:** Yes, we have tables in the database for both main and sub sectors.

---

## 📊 Database Tables

### 1. **GrcMainSectors** Table (NEW - Created)

**Purpose:** Stores the 18 main GRC sectors for KSA

**Table Name:** `GrcMainSectors`

**Columns:**
| Column | Type | Description |
|--------|------|-------------|
| `Id` | Guid (PK) | Primary key |
| `SectorCode` | string(50) | Unique sector code (e.g., "BANKING", "HEALTHCARE") |
| `SectorNameEn` | string(200) | Sector name in English |
| `SectorNameAr` | string(200) | Sector name in Arabic |
| `DescriptionEn` | string(1000) | Sector description in English |
| `DescriptionAr` | string(1000) | Sector description in Arabic |
| `PrimaryRegulator` | string(100) | Primary regulator (e.g., "SAMA", "NCA") |
| `FrameworkCount` | int | Number of applicable frameworks |
| `TotalControlCount` | int | Total controls across all frameworks |
| `DisplayOrder` | int | UI display order |
| `IsActive` | bool | Active status |
| `Icon` | string(50) | Icon/emoji for UI |
| `ColorCode` | string(20) | Color code for UI |
| `CreatedAt` | DateTime | Creation timestamp |
| `IsDeleted` | bool | Soft delete flag |

**Indexes:**
- `IX_GrcMainSector_Code` (Unique) - Fast lookup by sector code
- `IX_GrcMainSector_Active` - Filter active sectors
- `IX_GrcMainSector_DisplayOrder` - UI sorting

**Expected Rows:** 18 (one for each main sector)

---

### 2. **GrcSubSectorMappings** Table (EXISTING)

**Purpose:** Maps 70+ GOSI sub-sectors to 18 main GRC sectors

**Table Name:** `GrcSubSectorMappings`

**Columns:**
| Column | Type | Description |
|--------|------|-------------|
| `Id` | Guid (PK) | Primary key |
| `GosiCode` | string(10) | GOSI/ISIC code (e.g., "01", "05", "64") |
| `IsicSection` | string(2) | ISIC Section letter (A-U) |
| `SubSectorNameEn` | string(300) | Sub-sector name in English |
| `SubSectorNameAr` | string(300) | Sub-sector name in Arabic |
| `MainSectorCode` | string(50) | Main sector code (FK to GrcMainSectors) |
| `MainSectorId` | Guid? | Foreign key to GrcMainSectors.Id |
| `MainSectorNameEn` | string(200) | Main sector name (denormalized) |
| `MainSectorNameAr` | string(200) | Main sector name Arabic (denormalized) |
| `PrimaryRegulator` | string(100) | Primary regulator for this sub-sector |
| `RegulatoryNotes` | string(1000) | Additional regulatory notes |
| `DisplayOrder` | int | Display order within main sector |
| `IsActive` | bool | Active status |
| `CreatedAt` | DateTime | Creation timestamp |

**Indexes:**
- `IX_GrcSubSectorMapping_GosiCode` (Unique) - Fast lookup by GOSI code
- `IX_GrcSubSectorMapping_MainSectorCode` - Filter by main sector code
- `IX_GrcSubSectorMapping_MainSectorId` - Foreign key index
- `IX_GrcSubSectorMapping_IsicSection` - Filter by ISIC section
- `IX_GrcSubSectorMapping_Active` - Filter active sub-sectors

**Foreign Key:**
- `MainSectorId` → `GrcMainSectors.Id` (Optional, with SetNull on delete)

**Expected Rows:** ~70 (one for each GOSI sub-sector)

---

## 🔗 Relationship

```
GrcMainSector (1) ──────< (Many) GrcSubSectorMapping
```

- One main sector can have many sub-sectors
- Each sub-sector belongs to one main sector
- Relationship is optional (MainSectorId can be null for backward compatibility)

---

## 📋 Data Structure

### Main Sectors (18)

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

### Sub-Sectors (~70)

Mapped from GOSI/ISIC codes (01-99) to the 18 main sectors.

**Example:**
- GOSI Code "64" → Main Sector "BANKING"
- GOSI Code "86" → Main Sector "HEALTHCARE"
- GOSI Code "61" → Main Sector "TELECOM"

---

## 🔍 Query Examples

### Get All Main Sectors
```sql
SELECT * FROM "GrcMainSectors" 
WHERE "IsActive" = true 
ORDER BY "DisplayOrder";
```

### Get All Sub-Sectors for a Main Sector
```sql
SELECT s.* 
FROM "GrcSubSectorMappings" s
INNER JOIN "GrcMainSectors" m ON s."MainSectorCode" = m."SectorCode"
WHERE m."SectorCode" = 'BANKING'
  AND s."IsActive" = true
ORDER BY s."DisplayOrder";
```

### Get Main Sector with Sub-Sectors Count
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

## 🚀 Next Steps

1. **Create Migration** - Add `GrcMainSectors` table
2. **Seed Data** - Populate 18 main sectors
3. **Update Sub-Sector Mappings** - Link `MainSectorId` foreign keys
4. **Update Code** - Use `GrcMainSectors` table instead of constants

---

## 📝 Code References

- **Entity:** `Models/Entities/GrcMainSector.cs`
- **Entity:** `Models/Entities/GrcSubSectorMapping.cs`
- **DbContext:** `Data/GrcDbContext.cs`
- **Seed Data:** `Data/Seeds/GosiSectorSeeds.cs`

---

**Last Updated:** 2026-01-12
**Status:** ✅ Tables defined, migration needed
