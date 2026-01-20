# KSA Sector Mapping Summary

## 📊 Database Structure

### Main Sectors: **18 Sectors**

The database contains **18 main GRC sectors** mapped according to KSA (Kingdom of Saudi Arabia) standards.

### Sub-Sectors: **~70 GOSI Sub-Sectors**

These 18 main sectors map to approximately **70 GOSI (General Organization for Social Insurance) sub-sectors** based on:
- **ISIC Rev 4** (International Standard Industrial Classification)
- **Saudi Arabia's National Classification of Economic Activities**
- Reference: `stats.gov.sa` - National Classification of Economic Activities

---

## 🏢 The 18 Main GRC Sectors

### Original 9 Sectors

1. **BANKING** (الخدمات المصرفية والمالية)
   - Banking & Financial Services
   - Frameworks: SAMA-CSF, NCA-ECC, PDPL, SAMA-AML, PCI-DSS
   - Controls: 844

2. **HEALTHCARE** (الرعاية الصحية والطبية)
   - Healthcare & Medical
   - Frameworks: NCA-ECC, PDPL, CBAHI-HAS, MOH-HIS
   - Controls: 569

3. **GOVERNMENT** (القطاع الحكومي والعام)
   - Government & Public Sector
   - Frameworks: NCA-ECC, NCA-CSCC, PDPL, DGA-CLOUD
   - Controls: 322

4. **TELECOM** (الاتصالات)
   - Telecommunications
   - Frameworks: CST-CRF, NCA-ECC, NCA-CSCC, PDPL
   - Controls: 369

5. **ENERGY** (الطاقة والمرافق)
   - Energy & Utilities
   - Frameworks: NCA-ECC, NCA-CSCC, HCIS, PDPL
   - Controls: 339

6. **RETAIL** (التجزئة والتجارة الإلكترونية)
   - Retail & E-Commerce
   - Frameworks: PDPL, NCA-ECC, PCI-DSS, MOCI-ECOM
   - Controls: 606

7. **TECHNOLOGY** (التقنية والبرمجيات)
   - Technology & Software
   - Frameworks: NCA-ECC, PDPL, OWASP-ASVS, ISO-27001
   - Controls: 538

8. **INSURANCE** (التأمين)
   - Insurance
   - Frameworks: SAMA-CSF, SAMA-INSURANCE, NCA-ECC, PDPL, SAMA-AML
   - Controls: 560

9. **EDUCATION** (التعليم)
   - Education
   - Frameworks: NCA-ECC, PDPL, MOE-EDUCATION, ISO-27001
   - Controls: 397

### Additional 9 Sectors

10. **TRANSPORTATION** (النقل والخدمات اللوجستية)
    - Transportation & Logistics

11. **CONSTRUCTION** (البناء والتشييد والهندسة)
    - Construction & Engineering

12. **MANUFACTURING** (الصناعات التحويلية)
    - Manufacturing & Industry

13. **REAL_ESTATE** (العقارات)
    - Real Estate

14. **HOSPITALITY** (الضيافة والسياحة)
    - Hospitality & Tourism

15. **MEDIA** (الإعلام والترفيه)
    - Media & Entertainment

16. **AGRICULTURE** (الزراعة والغذاء)
    - Agriculture & Food

17. **MINING** (التعدين واستغلال المحاجر)
    - Mining & Quarrying

18. **PROFESSIONAL_SERVICES** (الخدمات المهنية)
    - Professional Services

---

## 📋 Database Tables

### 1. GrcSubSectorMappings Table
**Purpose:** Maps 70+ GOSI sub-sectors to 18 main GRC sectors

**Key Fields:**
- `GosiCode` - GOSI/ISIC code (e.g., "01", "05", "10", "64")
- `IsicSection` - ISIC Section letter (A-U)
- `SubSectorNameEn` - Sub-sector name in English
- `SubSectorNameAr` - Sub-sector name in Arabic
- `MainSectorCode` - One of 18 main GRC sector codes
- `MainSectorNameEn` - Main sector name in English
- `MainSectorNameAr` - Main sector name in Arabic
- `PrimaryRegulator` - Primary regulator for this sub-sector
- `RegulatoryNotes` - Additional regulatory notes

**Location:** `GrcDbContext.GrcSubSectorMappings`

### 2. SectorFrameworkIndex Table
**Purpose:** Fast lookup for sector → framework mappings

**Key Fields:**
- `SectorCode` - One of 18 main sector codes
- `FrameworkCode` - Framework code (e.g., "NCA-ECC", "SAMA-CSF")
- `OrgType` - Organization type filter
- `IsActive` - Active status

**Indexes:**
- `IX_SectorFrameworkIndex_Sector_OrgType` - Fast sector + org type lookup
- `IX_SectorFrameworkIndex_Sector_Framework` - Framework by sector

---

## 🔍 How It's Used in Onboarding

### Step A: Organization Identity
**Field:** `IndustrySector` dropdown

**Available Options (in StepA.cshtml):**
1. Banking
2. Insurance
3. Healthcare
4. Telecom
5. Energy
6. Government
7. Retail
8. Technology
9. Manufacturing
10. Education
11. RealEstate
12. Transportation
13. Other

**Note:** The view shows 13 options, but the database supports all 18 sectors.

### Step D: Scope Definition
**Filtering:** Business units, processes, and systems are filtered based on:
- Selected industry sector (Step A)
- Selected regulators (Step C)

---

## 📊 Sector Statistics

| Category | Count | Details |
|----------|-------|---------|
| **Main GRC Sectors** | **18** | Defined in `GrcMainSectors` class |
| **GOSI Sub-Sectors** | **70+** | Mapped to 18 main sectors |
| **ISIC Sections** | **A-U** | 21 sections (ISIC Rev 4) |
| **Sectors in Step A Dropdown** | **13** | Currently shown in onboarding wizard |
| **Sectors with Full Framework Mapping** | **9** | Original sectors with detailed framework mappings |

---

## 🔧 Code References

### Sector Constants
**Location:** `Models/Entities/GrcSubSectorMapping.cs`
```csharp
public static class GrcMainSectors
{
    // 18 sector constants defined
    public const string BANKING = "BANKING";
    public const string HEALTHCARE = "HEALTHCARE";
    // ... etc
}
```

### Database Entity
**Location:** `Data/GrcDbContext.cs`
```csharp
public DbSet<GrcSubSectorMapping> GrcSubSectorMappings { get; set; } = null!;
```

### Seed Data
**Location:** `Data/Seeds/GosiSectorSeeds.cs`
- Seeds 70+ GOSI sub-sectors
- Maps them to 18 main GRC sectors

---

## 🎯 Summary

**Answer:** The database contains **18 main GRC sectors** listed as per KSA mapping, which map to **70+ GOSI sub-sectors** based on ISIC Rev 4 classification.

**Current Status:**
- ✅ 18 sectors defined in code (`GrcMainSectors`)
- ✅ 70+ sub-sectors mapped in database (`GrcSubSectorMappings`)
- ⚠️ Step A dropdown shows 13 sectors (should be updated to show all 18)

---

**Last Updated:** 2026-01-12
**Reference:** Saudi Arabia's National Classification of Economic Activities (ISIC Rev 4)
