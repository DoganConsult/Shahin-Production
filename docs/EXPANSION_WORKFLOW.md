# Environment Variable Expansion Workflow

## 🔄 Visual Expansion Process

```
┌─────────────────────────────────────────────────────────┐
│  STEP 1: Define the Constant                            │
│  File: Settings/AppSettings.cs                          │
│                                                          │
│  public const string NewVar = "GrcMvc.Cat.Var";         │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  STEP 2: Add ABP Setting Definition                     │
│  File: Settings/GrcMvcSettingDefinitionProvider.cs       │
│                                                          │
│  context.Add(new SettingDefinition(                      │
│      AppSettings.NewVar,                                 │
│      isEncrypted: true/false,                            │
│      isVisibleToClients: true/false                      │
│  ));                                                     │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  STEP 3: Map to Environment Variable                     │
│  File: Settings/EnvironmentVariableSettingValueProvider.cs│
│                                                          │
│  AppSettings.NewVar => "NEW_VAR"                        │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  STEP 4A: Add to Known Variables                       │
│  File: Services/.../EnvironmentVariableService.cs        │
│                                                          │
│  GetKnownVariables() {                                   │
│      new() { Key = "NEW_VAR", ... }                     │
│  }                                                       │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  STEP 4B: Add to ABP Mapping                            │
│  File: Services/.../EnvironmentVariableService.cs        │
│                                                          │
│  GetAbpSettingName() {                                   │
│      "NEW_VAR" => AppSettings.NewVar                     │
│  }                                                       │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  ✅ DONE: New Variable Fully Integrated                  │
│  • Appears in Admin UI                                   │
│  • Supports environment variables                        │
│  • Supports ABP Settings (encrypted)                    │
│  • Follows existing style                                │
└─────────────────────────────────────────────────────────┘
```

---

## 📋 Quick Reference: 5 Files to Update

| Step | File | What to Add |
|------|------|-------------|
| 1 | `Settings/AppSettings.cs` | Constant definition |
| 2 | `Settings/GrcMvcSettingDefinitionProvider.cs` | Setting definition |
| 3 | `Settings/EnvironmentVariableSettingValueProvider.cs` | Env var mapping |
| 4A | `Services/.../EnvironmentVariableService.cs` | Known variable |
| 4B | `Services/.../EnvironmentVariableService.cs` | ABP mapping |

---

## ⚡ Quick Copy-Paste Snippets

### For Secrets (API Keys, Passwords, Tokens)

```csharp
// 1. AppSettings.cs
public const string NewSecret = "GrcMvc.Category.NewSecret";

// 2. GrcMvcSettingDefinitionProvider.cs
context.Add(
    new SettingDefinition(
        AppSettings.NewSecret,
        defaultValue: "",
        displayName: L("DisplayName:GrcMvc.Category.NewSecret"),
        description: L("Description:GrcMvc.Category.NewSecret"),
        isEncrypted: true,        // ← Encrypt
        isVisibleToClients: false // ← Hide from frontend
    )
);

// 3. EnvironmentVariableSettingValueProvider.cs
AppSettings.NewSecret => "NEW_SECRET",

// 4. EnvironmentVariableService.cs - GetKnownVariables()
new() { 
    Key = "NEW_SECRET", 
    Category = "Category", 
    IsRequired = false, 
    IsSecret = true, 
    Description = "Description" 
},

// 5. EnvironmentVariableService.cs - GetAbpSettingName()
"NEW_SECRET" or "Category__NewSecret" => AppSettings.NewSecret,
```

### For Public Settings (URLs, Feature Flags)

```csharp
// 1. AppSettings.cs
public const string NewPublicSetting = "GrcMvc.Category.NewPublicSetting";

// 2. GrcMvcSettingDefinitionProvider.cs
context.Add(
    new SettingDefinition(
        AppSettings.NewPublicSetting,
        defaultValue: "default-value",
        displayName: L("DisplayName:GrcMvc.Category.NewPublicSetting"),
        description: L("Description:GrcMvc.Category.NewPublicSetting"),
        isEncrypted: false,       // ← Not encrypted
        isVisibleToClients: true  // ← Can use in frontend
    )
);

// 3. EnvironmentVariableSettingValueProvider.cs
AppSettings.NewPublicSetting => "NEW_PUBLIC_SETTING",

// 4. EnvironmentVariableService.cs - GetKnownVariables()
new() { 
    Key = "NEW_PUBLIC_SETTING", 
    Category = "Category", 
    IsRequired = false, 
    IsSecret = false, 
    Description = "Description" 
},

// 5. EnvironmentVariableService.cs - GetAbpSettingName()
"NEW_PUBLIC_SETTING" or "Category__NewPublicSetting" => AppSettings.NewPublicSetting,
```

---

## 🎯 Style Preservation Rules

### ✅ DO:
- Use existing categories (Database, Security, Email, Integration, Payments, etc.)
- Follow naming conventions exactly
- Set encryption correctly (secrets = true, public = false)
- Add to all 5 locations
- Use constants, never hardcode strings
- Group related variables together

### ❌ DON'T:
- Create new categories unless necessary
- Mix naming styles
- Skip any of the 5 locations
- Hardcode values
- Expose secrets to frontend
- Forget encryption for secrets

---

## 🔍 Verification Commands

After adding new variables:

```bash
# 1. Build to check for errors
dotnet build

# 2. Check Admin UI
# Navigate to: /admin/environment-variables
# Verify new variables appear

# 3. Test environment variable
export NEW_VARIABLE_NAME="test-value"
dotnet run
# Check logs for correct value

# 4. Test ABP Settings
# Set value via Admin UI
# Verify it's encrypted (if secret)
# Verify it takes precedence over env var
```

---

*Follow this workflow and your expansions will seamlessly integrate with the existing system!*
