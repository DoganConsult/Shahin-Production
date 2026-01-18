# Expanding Environment Variables: Style-Preserving Guide

## 🎯 Purpose

This guide shows you how to add new environment variables or expand the system **without breaking existing patterns or style**. Follow these steps to maintain consistency.

---

## 📋 Step-by-Step Expansion Process

### Step 1: Add to ABP Settings Constants

**File:** `Settings/AppSettings.cs`

**Pattern to Follow:**
```csharp
// Category: Description
public const string NewVariableName = "GrcMvc.Category.VariableName";
```

**Example - Adding a New Service:**
```csharp
// Integration Services
public const string NewServiceApiKey = "GrcMvc.Integrations.NewServiceApiKey";
public const string NewServiceBaseUrl = "GrcMvc.Integrations.NewServiceBaseUrl";
public const string NewServiceEnabled = "GrcMvc.Integrations.NewServiceEnabled";
```

**✅ Style Rules:**
- Use PascalCase for constant names
- Group by category (Security, Integrations, Email, etc.)
- Use descriptive names
- Follow existing naming pattern: `GrcMvc.{Category}.{VariableName}`

---

### Step 2: Add to ABP Settings Definition Provider

**File:** `Settings/GrcMvcSettingDefinitionProvider.cs`

**Pattern to Follow:**
```csharp
// Category comment
context.Add(
    new SettingDefinition(
        AppSettings.NewVariableName,
        defaultValue: "",  // or "default-value"
        displayName: L("DisplayName:GrcMvc.Category.NewVariableName"),
        description: L("Description:GrcMvc.Category.NewVariableName"),
        isEncrypted: true,  // true for secrets, false for non-secrets
        isVisibleToClients: false  // false for secrets, true for public settings
    )
);
```

**Example - Adding New Service Settings:**
```csharp
// New Service Integration
context.Add(
    new SettingDefinition(
        AppSettings.NewServiceApiKey,
        defaultValue: "",
        displayName: L("DisplayName:GrcMvc.Integrations.NewServiceApiKey"),
        description: L("Description:GrcMvc.Integrations.NewServiceApiKey"),
        isEncrypted: true,  // ← Secret, encrypt it
        isVisibleToClients: false  // ← Hide from frontend
    )
);

context.Add(
    new SettingDefinition(
        AppSettings.NewServiceBaseUrl,
        defaultValue: "https://api.newservice.com",
        displayName: L("DisplayName:GrcMvc.Integrations.NewServiceBaseUrl"),
        description: L("Description:GrcMvc.Integrations.NewServiceBaseUrl"),
        isEncrypted: false,  // ← Not a secret
        isVisibleToClients: false  // ← Still hide from frontend
    )
);

context.Add(
    new SettingDefinition(
        AppSettings.NewServiceEnabled,
        defaultValue: "false",
        displayName: L("DisplayName:GrcMvc.Integrations.NewServiceEnabled"),
        description: L("Description:GrcMvc.Integrations.NewServiceEnabled"),
        isEncrypted: false,
        isVisibleToClients: false
    )
);
```

**✅ Style Rules:**
- Always use `AppSettings.ConstantName` (not hardcoded strings)
- Set `isEncrypted: true` for secrets (API keys, passwords, tokens)
- Set `isVisibleToClients: false` for sensitive data
- Provide meaningful default values
- Use localization keys (`L("...")`)

---

### Step 3: Add to Environment Variable Setting Value Provider

**File:** `Settings/EnvironmentVariableSettingValueProvider.cs`

**Pattern to Follow:**
```csharp
private string? GetEnvironmentVariableName(string settingName)
{
    return settingName switch
    {
        // Existing mappings...
        AppSettings.NewVariableName => "NEW_VARIABLE_NAME",  // ← Add here
        _ => null
    };
}
```

**Example:**
```csharp
private string? GetEnvironmentVariableName(string settingName)
{
    return settingName switch
    {
        // ... existing mappings ...
        
        // New Service
        AppSettings.NewServiceApiKey => "NEW_SERVICE_API_KEY",
        AppSettings.NewServiceBaseUrl => "NEW_SERVICE_BASE_URL",
        AppSettings.NewServiceEnabled => "NEW_SERVICE_ENABLED",
        
        _ => null
    };
}
```

**✅ Style Rules:**
- Use UPPER_SNAKE_CASE for environment variable names
- Map to descriptive names
- Add alternative formats if needed (e.g., `NewService__ApiKey`)

---

### Step 4: Add to Environment Variable Service

**File:** `Services/Implementations/EnvironmentVariableService.cs`

**A. Add to GetKnownVariables() Method:**

**Pattern:**
```csharp
new() { 
    Key = "ENV_VAR_NAME", 
    Category = "Category Name", 
    IsRequired = true/false, 
    IsSecret = true/false, 
    Description = "Human-readable description" 
}
```

**Example:**
```csharp
private List<EnvironmentVariableItem> GetKnownVariables()
{
    return new List<EnvironmentVariableItem>
    {
        // ... existing variables ...
        
        // New Service
        new() { 
            Key = "NEW_SERVICE_API_KEY", 
            Category = "Integration", 
            IsRequired = false, 
            IsSecret = true, 
            Description = "API key for New Service integration" 
        },
        new() { 
            Key = "NEW_SERVICE_BASE_URL", 
            Category = "Integration", 
            IsRequired = false, 
            IsSecret = false, 
            Description = "Base URL for New Service API" 
        },
        new() { 
            Key = "NEW_SERVICE_ENABLED", 
            Category = "Integration", 
            IsRequired = false, 
            IsSecret = false, 
            Description = "Enable/disable New Service (true/false)" 
        },
    };
}
```

**B. Add to GetAbpSettingName() Method:**

**Pattern:**
```csharp
return envVarName switch
{
    // ... existing mappings ...
    "ENV_VAR_NAME" or "AlternativeFormat" => AppSettings.NewVariableName,
    _ => null
};
```

**Example:**
```csharp
public string? GetAbpSettingName(string envVarName)
{
    return envVarName switch
    {
        // ... existing mappings ...
        
        // New Service
        "NEW_SERVICE_API_KEY" or "NewService__ApiKey" => AppSettings.NewServiceApiKey,
        "NEW_SERVICE_BASE_URL" or "NewService__BaseUrl" => AppSettings.NewServiceBaseUrl,
        "NEW_SERVICE_ENABLED" or "NewService__Enabled" => AppSettings.NewServiceEnabled,
        
        _ => null
    };
}
```

**✅ Style Rules:**
- Use same category as other similar variables
- Set `IsRequired = true` only for critical variables
- Set `IsSecret = true` for API keys, passwords, tokens
- Provide clear descriptions
- Support multiple environment variable formats

---

### Step 5: Update Connection String Resolution (If Adding Database)

**File:** `Extensions/WebApplicationBuilderExtensions.cs`

**Only needed if adding a new database connection string.**

**Pattern:**
```csharp
private static void ResolveConnectionStrings(WebApplicationBuilder builder)
{
    // ... existing code ...
    
    // Resolve new database connection string
    var newDbConnectionString = GetConnectionStringWithFallback(
        builder,
        "NewDatabaseConnection",
        new[] { "ConnectionStrings__NewDatabaseConnection", "NEW_DB_CONNECTION_STRING" }
    );

    if (!string.IsNullOrWhiteSpace(newDbConnectionString))
    {
        builder.Configuration["ConnectionStrings:NewDatabaseConnection"] = newDbConnectionString;
    }
}
```

**✅ Style Rules:**
- Use same pattern as existing connection strings
- Support multiple environment variable formats
- Set in IConfiguration for all points to use

---

### Step 6: Update Database Contexts (If Adding Database)

**File:** `Extensions/ServiceCollectionExtensions.cs`

**Only needed if adding a new database context.**

**Pattern:**
```csharp
public static IServiceCollection AddDatabaseContexts(this IServiceCollection services, IConfiguration configuration)
{
    // ... existing code ...
    
    var newDbConnectionString = configuration.GetConnectionString("NewDatabaseConnection");
    if (!string.IsNullOrWhiteSpace(newDbConnectionString))
    {
        services.AddDbContext<NewDbContext>(options =>
        {
            options.UseNpgsql(newDbConnectionString);
        });
    }
    
    return services;
}
```

---

## 🎨 Style Consistency Checklist

### Naming Conventions

- [ ] **ABP Settings Constants:** `GrcMvc.{Category}.{VariableName}` (PascalCase)
- [ ] **Environment Variables:** `UPPER_SNAKE_CASE` or `Category__VariableName`
- [ ] **C# Constants:** `PascalCase` matching ABP setting name
- [ ] **Categories:** Use existing categories (Database, Security, Email, Integration, etc.)

### Encryption & Visibility

- [ ] **Secrets:** `isEncrypted: true, isVisibleToClients: false`
- [ ] **Public Settings:** `isEncrypted: false, isVisibleToClients: true`
- [ ] **URLs/Endpoints:** `isEncrypted: false, isVisibleToClients: false`
- [ ] **Feature Flags:** `isEncrypted: false, isVisibleToClients: false`

### Code Organization

- [ ] **Group by Category:** Keep related settings together
- [ ] **Alphabetical Order:** Within each category
- [ ] **Comments:** Add category comments for clarity
- [ ] **Consistent Formatting:** Match existing code style

### Documentation

- [ ] **Update AppSettings.cs:** Add constant
- [ ] **Update SettingDefinitionProvider:** Add definition
- [ ] **Update Value Provider:** Add mapping
- [ ] **Update EnvironmentVariableService:** Add to known variables
- [ ] **Update GetAbpSettingName:** Add mapping

---

## 📝 Example: Adding a Complete New Service

### Scenario: Adding Stripe Payment Integration

**Step 1: Add Constants**
```csharp
// Settings/AppSettings.cs
public const string StripeApiKey = "GrcMvc.Payments.StripeApiKey";
public const string StripePublishableKey = "GrcMvc.Payments.StripePublishableKey";
public const string StripeWebhookSecret = "GrcMvc.Payments.StripeWebhookSecret";
public const string StripeEnabled = "GrcMvc.Payments.StripeEnabled";
```

**Step 2: Add Settings Definitions**
```csharp
// Settings/GrcMvcSettingDefinitionProvider.cs
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
        isEncrypted: false,  // Public key, not secret
        isVisibleToClients: true  // Can be used in frontend
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

**Step 3: Add to Value Provider**
```csharp
// Settings/EnvironmentVariableSettingValueProvider.cs
private string? GetEnvironmentVariableName(string settingName)
{
    return settingName switch
    {
        // ... existing ...
        AppSettings.StripeApiKey => "STRIPE_API_KEY",
        AppSettings.StripePublishableKey => "STRIPE_PUBLISHABLE_KEY",
        AppSettings.StripeWebhookSecret => "STRIPE_WEBHOOK_SECRET",
        AppSettings.StripeEnabled => "STRIPE_ENABLED",
        _ => null
    };
}
```

**Step 4: Add to Environment Variable Service**
```csharp
// Services/Implementations/EnvironmentVariableService.cs
// In GetKnownVariables():
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

// In GetAbpSettingName():
"STRIPE_API_KEY" or "Stripe__ApiKey" => AppSettings.StripeApiKey,
"STRIPE_PUBLISHABLE_KEY" or "Stripe__PublishableKey" => AppSettings.StripePublishableKey,
"STRIPE_WEBHOOK_SECRET" or "Stripe__WebhookSecret" => AppSettings.StripeWebhookSecret,
"STRIPE_ENABLED" or "Stripe__Enabled" => AppSettings.StripeEnabled,
```

**That's it!** The new service is now fully integrated following the exact same pattern.

---

## 🔍 Verification Steps

After adding new variables:

1. **Build the project:**
   ```bash
   dotnet build
   ```

2. **Check for compilation errors:**
   - All constants referenced correctly
   - All mappings complete
   - No hardcoded strings

3. **Test in Admin UI:**
   - Navigate to `/admin/environment-variables`
   - Verify new variables appear in correct category
   - Check encryption status (secrets should be encrypted)
   - Test updating values

4. **Test Environment Variable Reading:**
   ```bash
   export NEW_SERVICE_API_KEY="test-key"
   dotnet run
   # Check logs for correct value
   ```

5. **Test ABP Settings:**
   - Set value via Admin UI
   - Verify it's encrypted in database
   - Verify it takes precedence over env var

---

## ⚠️ Common Mistakes to Avoid

### ❌ Don't Do This:

```csharp
// ❌ Hardcoded string instead of constant
context.Add(new SettingDefinition("GrcMvc.NewService.ApiKey", ...));

// ❌ Missing encryption for secrets
context.Add(new SettingDefinition(AppSettings.NewServiceApiKey, ..., isEncrypted: false));

// ❌ Exposing secrets to frontend
context.Add(new SettingDefinition(AppSettings.NewServiceApiKey, ..., isVisibleToClients: true));

// ❌ Incomplete mapping
// Missing from GetEnvironmentVariableName() or GetAbpSettingName()
```

### ✅ Do This Instead:

```csharp
// ✅ Use constant
context.Add(new SettingDefinition(AppSettings.NewServiceApiKey, ...));

// ✅ Encrypt secrets
context.Add(new SettingDefinition(AppSettings.NewServiceApiKey, ..., isEncrypted: true));

// ✅ Hide secrets from frontend
context.Add(new SettingDefinition(AppSettings.NewServiceApiKey, ..., isVisibleToClients: false));

// ✅ Complete all mappings
// Add to GetEnvironmentVariableName(), GetAbpSettingName(), GetKnownVariables()
```

---

## 📚 Quick Reference

### File Locations

| Component | File Path |
|-----------|-----------|
| Constants | `Settings/AppSettings.cs` |
| Definitions | `Settings/GrcMvcSettingDefinitionProvider.cs` |
| Value Provider | `Settings/EnvironmentVariableSettingValueProvider.cs` |
| Service | `Services/Implementations/EnvironmentVariableService.cs` |
| Connection Strings | `Extensions/WebApplicationBuilderExtensions.cs` |

### Required Updates for New Variable

1. ✅ Add constant to `AppSettings.cs`
2. ✅ Add definition to `GrcMvcSettingDefinitionProvider.cs`
3. ✅ Add mapping to `EnvironmentVariableSettingValueProvider.cs`
4. ✅ Add to `GetKnownVariables()` in `EnvironmentVariableService.cs`
5. ✅ Add to `GetAbpSettingName()` in `EnvironmentVariableService.cs`

---

## 🎯 Summary

**To add new environment variables without breaking style:**

1. **Follow the pattern** - Copy existing similar variables
2. **Update all 5 locations** - Constants, Definitions, Value Provider, Service (2 methods)
3. **Maintain naming conventions** - Use same style as existing code
4. **Set encryption correctly** - Secrets encrypted, public settings not
5. **Test thoroughly** - Verify in Admin UI and via environment variables

**The system is designed to be easily expandable while maintaining consistency!**

---

*Follow this guide and your new variables will integrate seamlessly with the existing system, maintaining the same high-quality style and patterns.*
