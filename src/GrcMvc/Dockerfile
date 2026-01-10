FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/GrcMvc/GrcMvc.csproj", "GrcMvc/"]
RUN dotnet restore "GrcMvc/GrcMvc.csproj"

# Copy all source code
COPY src/GrcMvc/ GrcMvc/

# ============================================================
# MANDATORY QUALITY CHECK - Build will fail if validation fails
# ============================================================
WORKDIR "/src/GrcMvc"

# Build the project (includes compile-time validation)
RUN dotnet build "GrcMvc.csproj" -c Release -o /app/build

# Validate all Shahin modules are present (MANDATORY)
RUN echo "Running mandatory quality checks..." && \
    MISSING=0 && \
    for controller in OnboardingController ControlsController EvidenceController \
                      RiskIndicatorsController ExceptionsController VaultController \
                      WorkflowUIController ShahinAIIntegrationController; do \
        if [ ! -f "Controllers/${controller}.cs" ]; then \
            echo "ERROR: Missing ${controller}.cs"; \
            MISSING=$((MISSING + 1)); \
        fi; \
    done && \
    if [ $MISSING -gt 0 ]; then \
        echo "BUILD FAILED: $MISSING Shahin module(s) missing"; \
        exit 1; \
    fi && \
    echo "Quality check passed: All Shahin modules present"

# Validate critical views exist (MANDATORY)
RUN echo "Validating views..." && \
    MISSING=0 && \
    if [ ! -f "Views/Shared/_Layout.cshtml" ]; then \
        echo "ERROR: Missing _Layout.cshtml"; \
        MISSING=$((MISSING + 1)); \
    fi && \
    if [ ! -f "Views/_ViewStart.cshtml" ]; then \
        echo "ERROR: Missing _ViewStart.cshtml"; \
        MISSING=$((MISSING + 1)); \
    fi && \
    if [ $MISSING -gt 0 ]; then \
        echo "BUILD FAILED: Critical views missing"; \
        exit 1; \
    fi && \
    echo "View validation passed"

FROM build AS publish
RUN dotnet publish "GrcMvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security (Debian-based commands for aspnet image)
RUN groupadd -g 1000 appuser && \
    useradd -m -u 1000 -g appuser appuser && \
    chown -R appuser:appuser /app

USER appuser

ENTRYPOINT ["dotnet", "GrcMvc.dll"]
