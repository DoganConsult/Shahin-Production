# Risk Localization Keys

This document lists all localization keys used in the Risk views (Matrix and Report).

## Files Converted
- `src/GrcMvc/Views/Risk/Matrix.cshtml`
- `src/GrcMvc/Views/Risk/Report.cshtml`

## Localization Keys by Category

### Page Titles
| Key | English Value | Usage |
|-----|---------------|-------|
| `Risk_Matrix_Title` | Risk Matrix | Page title for Risk Matrix view |
| `Risk_Report_Title` | Risk Report | Page title for Risk Report view |

### Risk Matrix View
| Key | English Value | Usage |
|-----|---------------|-------|
| `Risk_HeatMap` | Risk Heatmap (Probability vs Impact) | Heat map card header |
| `Risk_Probability` | Probability | Table header for probability axis |
| `Risk_Impact` | Impact | Table header for impact axis |
| `Risk_Impact_1_Negligible` | 1 - Negligible | Impact level 1 |
| `Risk_Impact_2_Minor` | 2 - Minor | Impact level 2 |
| `Risk_Impact_3_Moderate` | 3 - Moderate | Impact level 3 |
| `Risk_Impact_4_Major` | 4 - Major | Impact level 4 |
| `Risk_Impact_5_Critical` | 5 - Critical | Impact level 5 |
| `Risk_Probability_1_Rare` | 1 - Rare | Probability level 1 |
| `Risk_Probability_2_Unlikely` | 2 - Unlikely | Probability level 2 |
| `Risk_Probability_3_Possible` | 3 - Possible | Probability level 3 |
| `Risk_Probability_4_Likely` | 4 - Likely | Probability level 4 |
| `Risk_Probability_5_Frequent` | 5 - Frequent | Probability level 5 |
| `Risk_Risks` | Risks | Modal title prefix |
| `Risk_RiskLevels` | Risk Levels | Statistics card header |
| `Risk_Level_High` | High (15-25) | High risk level label |
| `Risk_Level_Medium` | Medium (8-14) | Medium risk level label |
| `Risk_Level_Low` | Low (1-7) | Low risk level label |
| `Risk_TopCategories` | Top Categories | Statistics card header |
| `Risk_TopRisksByScore` | Top Risks by Score | Statistics card header |
| `Risk_NewRisk` | New Risk | Button to create new risk |

### Risk Report View
| Key | English Value | Usage |
|-----|---------------|-------|
| `Risk_PrintReport` | Print Report | Print button text |
| `Risk_ExecutiveSummary` | Executive Summary | Executive summary card header |
| `Risk_TotalRisks` | Total Risks | Metric label |
| `Risk_HighPriority` | High Priority | Metric label for high priority risks |
| `Risk_MediumPriority` | Medium Priority | Metric label for medium priority risks |
| `Risk_LowPriority` | Low Priority | Metric label for low priority risks |
| `Risk_DistributionByCategory` | Risk Distribution by Category | Chart title |
| `Risk_RiskStatus` | Risk Status | Chart title |
| `Risk_DetailedRiskRegister` | Detailed Risk Register | Table header |
| `Risk` | Risk | Table column header |
| `Risk_InherentScore` | Inherent Score | Table column header |
| `Risk_ResidualScore` | Residual Score | Table column header |
| `Risk_GeneratedOn` | Generated on | Report footer prefix |
| `Risk_ByGRCSystem` | by GRC System | Report footer suffix |

### Common Keys (Reused)
| Key | English Value | Usage |
|-----|---------------|-------|
| `BackToList` | Back to List | Navigation button |
| `View` | View | Action button in modal |
| `Name` | Name | Table column header |
| `Category` | Category | Table column header |
| `RiskScore` | Score | Table column header |
| `Owner` | Owner | Table column header / field label |
| `Actions` | Actions | Table column header |
| `Status` | Status | Table column header / field label |
| `DueDate` | Due Date | Table column header |

## Implementation Notes

1. All views now use the pattern:
   ```csharp
   @using Microsoft.AspNetCore.Mvc.Localization
   @inject IHtmlLocalizer<SharedResource> L
   ```

2. ViewData["Title"] uses `.Value` to extract the string:
   ```csharp
   ViewData["Title"] = L["Risk_Matrix_Title"].Value;
   ```

3. Inline text uses direct localization:
   ```csharp
   <h2>@L["Risk_Matrix_Title"]</h2>
   ```

4. Functions use `.Value` for string interpolation:
   ```csharp
   5 => L["Risk_Probability_5_Frequent"].Value
   ```

## Next Steps

1. Add these keys to `SharedResource.en.resx` with English values
2. Add these keys to `SharedResource.ar.resx` with Arabic translations
3. Test all Risk views with both English and Arabic cultures
4. Verify that all probability and impact labels display correctly in the matrix
