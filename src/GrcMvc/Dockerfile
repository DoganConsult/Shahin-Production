# ══════════════════════════════════════════════════════════════
# Shahin AI GRC Platform - Production Dockerfile
# ABP Framework Integrated - Enterprise Architecture
# ══════════════════════════════════════════════════════════════

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user for security
RUN adduser --disabled-password --gecos '' --shell /bin/bash dotnetuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file first (for layer caching)
COPY ["src/GrcMvc/GrcMvc.csproj", "src/GrcMvc/"]

# Restore dependencies
RUN dotnet restore "src/GrcMvc/GrcMvc.csproj"

# Copy everything else
COPY . .

# Build application
WORKDIR "/src/src/GrcMvc"
RUN dotnet build "GrcMvc.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GrcMvc.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# ══════════════════════════════════════════════════════════════
# Final runtime image
# ══════════════════════════════════════════════════════════════
FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Create necessary directories
RUN mkdir -p /app/logs /app/wwwroot /app/certificates

# Set ownership for non-root user
RUN chown -R dotnetuser:dotnetuser /app

# Switch to non-root user
USER dotnetuser

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "GrcMvc.dll"]

# ══════════════════════════════════════════════════════════════
# Build Info & Labels  
# ══════════════════════════════════════════════════════════════
LABEL org.opencontainers.image.title="Shahin AI GRC Platform"
LABEL org.opencontainers.image.description="Enterprise GRC Platform with ABP Framework Integration"
LABEL org.opencontainers.image.vendor="Shahin AI"
LABEL org.opencontainers.image.version="2026.01.18"
LABEL org.opencontainers.image.created="2026-01-18"
LABEL org.opencontainers.image.source="https://github.com/doganlap/Shahin-Ai"
LABEL abp.framework.version="8.2.2"
LABEL abp.integration.status="complete"