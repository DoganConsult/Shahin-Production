# Template: Adding New Environment Variable

## 📋 Copy-Paste Template

Use this template to add new environment variables while maintaining style consistency.

---

## Step 1: Add Constant to AppSettings.cs

**File:** `Settings/AppSettings.cs`

```csharp
// Category Name (e.g., Integration, Security, Email)
public const string NewVariableName = "GrcMvc.Category.VariableName";
```

**Example:**
```csharp
// Payment Services
public const string StripeApiKey = "GrcMvc.Payments.StripeApiKey";
```

---

## Step 2: Add Definition to GrcMvcSettingDefinitionProvider.cs

**File:** `Settings/GrcMvcSettingDefinitionProvider.cs`

```csharp
// Category comment
context.Add(
    new SettingDefinition(
        AppSettings.NewVariableName,
        defaultValue: "",  // or "default-value" for non-secrets
        displayName: L("DisplayName:GrcMvc.Category.NewVariableName"),
        description: L("Description:GrcMvc.Category.NewVariableName"),
        isEncrypted: true,  // true for secrets, false for public settings
        isVisibleToClients: false  // false for secrets, true for public settings
    )
);
```

**Example (Secret):**
```csharp
// Payment Services
context.Add(
    new SettingDefinition(
        AppSettings.StripeApiKey,
        defaultValue: "",
        displayName: L("DisplayName:GrcMvc.Payments.StripeApiKey"),
        description: L("Description:GrcMvc.Payments.StripeApiKey"),
        isEncrypted: true,
        isVisibleToClients: false
    )
);
```

**Example (Public Setting):**
```csharp
context.Add(
    new SettingDefinition(
        AppSettings.StripePublishableKey,
        defaultValue: "",
        displayName: L("DisplayName:GrcMvc.Payments.StripePublishableKey"),
        description: L("Description:GrcMvc.Payments.StripePublishableKey"),
        isEncrypted: false,  // Public key, not secret
        isVisibleToClients: true  // Can be used in frontend
    )
);
```

---

## Step 3: Add Mapping to EnvironmentVariableSettingValueProvider.cs

**File:** `Settings/EnvironmentVariableSettingValueProvider.cs`

**In `GetEnvironmentVariableName()` method:**
```csharp
AppSettings.NewVariableName => "NEW_VARIABLE_NAME",
```

**Example:**
```csharp
AppSettings.StripeApiKey => "STRIPE_API_KEY",
```

---

## Step 4: Add to EnvironmentVariableService.cs

**File:** `Services/Implementations/EnvironmentVariableService.cs`

**A. In `GetKnownVariables()` method:**
```csharp
new() { 
    Key = "NEW_VARIABLE_NAME", 
    Category = "Category Name", 
    IsRequired = false,  // true only for critical variables
    IsSecret = true,  // true for secrets, false for public
    Description = "Human-readable description" 
},
```

**Example:**
```csharp
new() { 
    Key = "STRIPE_API_KEY", 
    Category = "Payments", 
    IsRequired = false, 
    IsSecret = true, 
    Description = "Stripe API secret key" 
},
```

**B. In `GetAbpSettingName()` method:**
```csharp
"NEW_VARIABLE_NAME" or "Category__VariableName" => AppSettings.NewVariableName,
```

**Example:**
```csharp
"STRIPE_API_KEY" or "Stripe__ApiKey" => AppSettings.StripeApiKey,
```

---

## ✅ Complete Example: Adding Stripe Integration

### 1. AppSettings.cs
```csharp
// Payment Services
public const string StripeApiKey = "GrcMvc.Payments.StripeApiKey";
public const string StripePublishableKey = "GrcMvc.Payments.StripePublishableKey";
public const string StripeWebhookSecret = "GrcMvc.Payments.StripeWebhookSecret";
public const string StripeEnabled = "GrcMvc.Payments.StripeEnabled";
```

### 2. GrcMvcSettingDefinitionProvider.cs
```csharp
// Payment Services
context.Add(
    new SettingDefinition(
        AppSettings.StripeApiKey,
        defaultValue: "",
        displayName: L("DisplayName:GrcMvc.Payments.StripeApiKey"),
        description: L("Description:GrcMvc.Payments.StripeApiKey"),
        isEncrypted: true,
        isVisibleToClients: false
    )
);

context.Add(
    new SettingDefinition(
        AppSettings.StripePublishableKey,
        defaultValue: "",
        displayName: L("DisplayName:GrcMvc.Payments.StripePublishableKey"),
        description: L("Description:GrcMvc.Payments.StripePublishableKey"),
        isEncrypted: false,
        isVisibleToClients: true
    )
);

context.Add(
    new SettingDefinition(
        AppSettings.StripeWebhookSecret,
        defaultValue: "",
        displayName: L("DisplayName:GrcMvc.Payments.StripeWebhookSecret"),
        description: L("Description:GrcMvc.Payments.StripeWebhookSecret"),
        isEncrypted: true,
        isVisibleToClients: false
    )
);

context.Add(
    new SettingDefinition(
        AppSettings.StripeEnabled,
        defaultValue: "false",
        displayName: L("DisplayName:GrcMvc.Payments.StripeEnabled"),
        description: L("Description:GrcMvc.Payments.StripeEnabled"),
        isEncrypted: false,
        isVisibleToClients: false
    )
);
```

### 3. EnvironmentVariableSettingValueProvider.cs
```csharp
private string? GetEnvironmentVariableName(string settingName)
{
    return settingName switch
    {
        // ... existing mappings ...
        AppSettings.StripeApiKey => "STRIPE_API_KEY",
        AppSettings.StripePublishableKey => "STRIPE_PUBLISHABLE_KEY",
        AppSettings.StripeWebhookSecret => "STRIPE_WEBHOOK_SECRET",
        AppSettings.StripeEnabled => "STRIPE_ENABLED",
        _ => null
    };
}
```

### 4. EnvironmentVariableService.cs

**In `GetKnownVariables()`:**
```csharp
// Payment Services
new() { 
    Key = "STRIPE_API_KEY", 
    Category = "Payments", 
    IsRequired = false, 
    IsSecret = true, 
    Description = "Stripe API secret key" 
},
new() { 
    Key = "STRIPE_PUBLISHABLE_KEY", 
    Category = "Payments", 
    IsRequired = false, 
    IsSecret = false, 
    Description = "Stripe publishable key (public)" 
},
new() { 
    Key = "STRIPE_WEBHOOK_SECRET", 
    Category = "Payments", 
    IsRequired = false, 
    IsSecret = true, 
    Description = "Stripe webhook signing secret" 
},
new() { 
    Key = "STRIPE_ENABLED", 
    Category = "Payments", 
    IsRequired = false, 
    IsSecret = false, 
    Description = "Enable/disable Stripe payments (true/false)" 
},
```

**In `GetAbpSettingName()`:**
```csharp
"STRIPE_API_KEY" or "Stripe__ApiKey" => AppSettings.StripeApiKey,
"STRIPE_PUBLISHABLE_KEY" or "Stripe__PublishableKey" => AppSettings.StripePublishableKey,
"STRIPE_WEBHOOK_SECRET" or "Stripe__WebhookSecret" => AppSettings.StripeWebhookSecret,
"STRIPE_ENABLED" or "Stripe__Enabled" => AppSettings.StripeEnabled,
```

---

## 🎯 Quick Checklist

- [ ] Added constant to `AppSettings.cs`
- [ ] Added definition to `GrcMvcSettingDefinitionProvider.cs`
- [ ] Set `isEncrypted` correctly (true for secrets)
- [ ] Set `isVisibleToClients` correctly (false for secrets)
- [ ] Added mapping to `EnvironmentVariableSettingValueProvider.cs`
- [ ] Added to `GetKnownVariables()` in `EnvironmentVariableService.cs`
- [ ] Added to `GetAbpSettingName()` in `EnvironmentVariableService.cs`
- [ ] Tested in Admin UI (`/admin/environment-variables`)
- [ ] Verified encryption (if secret)
- [ ] Verified environment variable reading

---

## 📝 Naming Convention Reference

| Type | Format | Example |
|------|--------|---------|
| ABP Setting Constant | `GrcMvc.{Category}.{Name}` | `GrcMvc.Payments.StripeApiKey` |
| C# Constant | `PascalCase` | `StripeApiKey` |
| Environment Variable | `UPPER_SNAKE_CASE` | `STRIPE_API_KEY` |
| Alternative Format | `Category__VariableName` | `Stripe__ApiKey` |

---

*Copy this template, fill in your values, and maintain perfect style consistency!*
