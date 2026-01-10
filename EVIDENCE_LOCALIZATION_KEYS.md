# Evidence Localization Keys

This document lists all localization keys used in the Evidence views.

## Files Converted
- `src/GrcMvc/Views/Evidence/Create.cshtml`
- `src/GrcMvc/Views/Evidence/Edit.cshtml`
- `src/GrcMvc/Views/Evidence/Details.cshtml`
- `src/GrcMvc/Views/Evidence/Delete.cshtml`
- `src/GrcMvc/Views/Evidence/ByAudit.cshtml`
- `src/GrcMvc/Views/Evidence/ByType.cshtml`
- `src/GrcMvc/Views/Evidence/ByClassification.cshtml`
- `src/GrcMvc/Views/Evidence/Expiring.cshtml`
- `src/GrcMvc/Views/Evidence/Statistics.cshtml`

## Localization Keys by Category

### Page Titles
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_Create_Title` | Upload New Evidence | Create view title |
| `Evidence_Edit_Title` | Edit Evidence | Edit view title |
| `Evidence_Details_Title` | Evidence Details | Details view title |
| `Evidence_Delete_Title` | Delete Evidence | Delete view title |
| `Evidence_ByAudit_Title` | Evidence for Audit | By Audit view title |
| `Evidence_ByType_Title` | Evidence by Type | By Type view title |
| `Evidence_ByClassification_Title` | Evidence by Classification | By Classification view title |
| `Evidence_Expiring_Title` | Expiring Evidence (Next 30 Days) | Expiring view title |
| `Evidence_Statistics_Title` | Evidence Statistics | Statistics view title |

### Form Fields
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_Name` | Evidence Name | Form field label |
| `Evidence_NamePlaceholder` | e.g. Q1 Access Log | Input placeholder |
| `Evidence_SelectType` | Select Type... | Dropdown default option |
| `Evidence_Source` | Source System/Person | Form field label |
| `Evidence_SourcePlaceholder` | e.g. AD Server or John Doe | Input placeholder |
| `Evidence_CollectionDate` | Collection Date | Form field label |
| `Evidence_ExpirationDate` | Expiration Date (Optional) | Form field label |
| `Evidence_Location` | File Location / URL | Form field label |
| `Evidence_Tags` | Tags | Form field label |
| `Evidence_TagsPlaceholder` | Comma separated tags | Input placeholder |
| `Evidence_Notes` | Notes | Form field label |
| `Evidence_Classification` | Classification | Form field label / column header |

### Evidence Types
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_Type_Document` | Document | Evidence type option |
| `Evidence_Type_Screenshot` | Screenshot | Evidence type option |
| `Evidence_Type_Log` | Log File | Evidence type option |
| `Evidence_Type_Report` | Report | Evidence type option |
| `Evidence_Type_Email` | Email | Evidence type option |
| `Evidence_Type_Other` | Other | Evidence type option |

### Evidence Status
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_Status_Active` | Active | Status option |
| `Evidence_Status_Pending` | Pending Review | Status option |
| `Evidence_Status_Draft` | Draft | Status option |
| `Evidence_Status_Expired` | Expired | Status option |
| `Evidence_Status_Archived` | Archived | Status option |

### Data Classification
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_Classification_Public` | Public | Classification level |
| `Evidence_Classification_Internal` | Internal | Classification level |
| `Evidence_Classification_Confidential` | Confidential | Classification level |
| `Evidence_Classification_Restricted` | Restricted | Classification level |

### Actions & Buttons
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_CreateEvidence` | Create Evidence | Submit button in Create view |
| `Evidence_DeleteEvidence` | Delete Evidence | Delete confirmation button |
| `Evidence_Download` | Download / View | Download button |
| `Evidence_RenewUpdate` | Renew / Update | Action button in Expiring view |
| `Evidence_OrUploadFile` | Or upload a file (feature coming soon) | Helper text |

### Details View Sections
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_GeneralInformation` | General Information | Card header |
| `Evidence_Metadata` | Metadata | Card header |
| `Evidence_ValidityOwnership` | Validity & Ownership | Card header |

### Delete View
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_DeleteConfirmation` | Delete Confirmation | Card header |
| `Evidence_DeleteWarning` | Are you sure you want to delete this evidence? | Warning message |
| `Evidence_DeleteWarningMessage` | This action cannot be undone. The evidence | Warning message prefix |
| `Evidence_DeletePermanent` | will be permanently deleted. | Warning message suffix |
| `Evidence_Collected` | Collected | Field label |

### By Audit View
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_ForAuditId` | For Audit ID | Label for audit ID display |
| `Evidence_BackToAudit` | Back to Audit | Navigation button |
| `Evidence_AllEvidence` | All Evidence | Navigation button |
| `Evidence_NoEvidenceForAudit` | No evidence attached to this audit. | Empty state message |

### By Type View
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_EvidenceType` | Evidence Type | Page subtitle |
| `Evidence_NoEvidenceOfType` | No evidence found of this type. | Empty state message |

### By Classification View
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_NoEvidenceWithClassification` | No evidence found with this classification. | Empty state message |

### Expiring View
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_NoEvidenceExpiringSoon` | No evidence expiring soon. | Empty state message |
| `Evidence_AllEvidenceUpToDate` | All evidence is up to date! | Empty state additional text |
| `Evidence_DaysLeft` | Days Left | Table column header |
| `Evidence_Expired` | Expired | Status label for expired items |
| `Evidence_Days` | days | Unit label for days remaining |

### Statistics View
| Key | English Value | Usage |
|-----|---------------|-------|
| `Evidence_TotalEvidence` | Total Evidence | Metric card title |
| `Evidence_Active` | Active | Metric card title |
| `Evidence_ExpiringSoon` | Expiring Soon | Metric card title |
| `Evidence_Archived` | Archived | Metric card title |
| `Evidence_ByType` | Evidence by Type | Chart title |
| `Evidence_DataClassification` | Data Classification | Chart title |
| `Evidence_StatusDistribution` | Status Distribution | Chart title |
| `Evidence_Count` | Count | Chart label |

### Common Keys (Reused from SharedResource)
| Key | English Value | Usage |
|-----|---------------|-------|
| `BackToList` | Back to List | Navigation button |
| `Cancel` | Cancel | Cancel button |
| `SaveChanges` | Save Changes | Submit button in Edit view |
| `Edit` | Edit | Edit button / action link |
| `Details` | Details | Details button / action link |
| `Delete` | Delete | Delete button / action link |
| `Actions` | Actions | Table column header |
| `Name` | Name | Table column header / field label |
| `Description` | Description | Field label |
| `Status` | Status | Table column header / field label |
| `Type` | Type | Table column header / field label |
| `Owner` | Owner | Table column header / field label |
| `View` | View | Action button |

## Implementation Notes

1. All views now use the pattern:
   ```csharp
   @using Microsoft.AspNetCore.Mvc.Localization
   @inject IHtmlLocalizer<SharedResource> L
   ```

2. ViewData["Title"] uses `.Value` to extract the string:
   ```csharp
   ViewData["Title"] = L["Evidence_Create_Title"].Value;
   ```

3. Inline text uses direct localization:
   ```csharp
   <h2>@L["Evidence_Create_Title"]</h2>
   ```

4. Form field labels and placeholders:
   ```csharp
   <label>@L["Evidence_Name"]</label>
   <input placeholder="@L["Evidence_NamePlaceholder"]" />
   ```

5. Select options use localized values:
   ```csharp
   <option value="Document">@L["Evidence_Type_Document"]</option>
   ```

## Naming Convention

All Evidence-specific keys follow the pattern: `Evidence_{Context}_{Detail}`

Examples:
- `Evidence_Create_Title` - Page title for create view
- `Evidence_Type_Document` - Evidence type enum value
- `Evidence_Status_Active` - Status enum value
- `Evidence_Classification_Public` - Classification enum value

## Next Steps

1. Add these keys to `SharedResource.en.resx` with English values
2. Add these keys to `SharedResource.ar.resx` with Arabic translations
3. Test all Evidence views with both English and Arabic cultures
4. Verify dropdowns, labels, and messages display correctly
5. Test empty states and validation messages
