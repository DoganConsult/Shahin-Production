@echo off
echo Starting Shahin GRC Platform...

set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://localhost:8888
set ConnectionStrings__DefaultConnection=Host=localhost;Port=5433;Database=GrcMvcDb;Username=postgres;Password=postgres
set ConnectionStrings__GrcAuthDb=Host=localhost;Port=5433;Database=GrcMvcDb;Username=postgres;Password=postgres
set ConnectionStrings__HangfireConnection=Host=localhost;Port=5433;Database=GrcMvcDb;Username=postgres;Password=postgres
set ConnectionStrings__Redis=localhost:6379
set Redis__ConnectionString=localhost:6379
set Redis__Enabled=true
set JwtSettings__Secret=ShahinAI-Dev-JWT-Secret-Key-2026-Minimum-32-Characters

cd /d "c:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc\publish-fresh"
dotnet GrcMvc.dll
