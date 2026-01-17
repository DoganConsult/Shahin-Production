Safeguards implemented
1. Fixed Program.cs — use migrations instead of EnsureCreated()
Changed: Replaced EnsureCreated() with Migrate() for GrcAuthDbContext
Why: EnsureCreated() bypasses migrations and can create incomplete schemas
Result: Migrations are applied on startup, ensuring the schema matches ApplicationUser
2. Added documentation
Created: docs/IDENTITY_SCHEMA_SAFEGUARDS.md
Contains:
Why migrations are required
How to create new migrations
Verification checklist
Troubleshooting guide
List of all ApplicationUser properties
3. Added code comments
Added warning comments in Program.cs explaining why Migrate() must be used
References the safeguard documentation
What this prevents
Missing columns: migrations ensure all ApplicationUser properties are in the database
Schema drift: migrations track changes and can be versioned/rolled back
Silent failures: Migrate() will fail if migrations are missing, making issues visible
Next steps when adding properties
Add the property to ApplicationUser.cs
Create a migration: dotnet ef migrations add AddPropertyName --context GrcAuthDbContext --output-dir Migrations/Auth
Apply it: dotnet ef database update --context GrcAuthDbContext
Test: verify the form can save/load the new property
The application will automatically apply migrations on startup, so the schema stays in sync with the entity model.---
trigger: manual
---

