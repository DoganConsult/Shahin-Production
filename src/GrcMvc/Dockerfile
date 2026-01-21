# ═══════════════════════════════════════════════════════════════════════════════
# SHAHIN AI GRC PLATFORM - DOCKERFILE
# Multi-stage build for ASP.NET Core 8.0 MVC Application
# ═══════════════════════════════════════════════════════════════════════════════

# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install required packages
RUN apk add --no-cache \
    icu-libs \
    tzdata \
    curl

# Set timezone and enable globalization (for Arabic/multi-language support)
ENV TZ=UTC
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8

# Stage 2: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["GrcMvc.csproj", "./"]
RUN dotnet restore "GrcMvc.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "GrcMvc.csproj" -c Release -o /app/build

# Stage 3: Publish stage
FROM build AS publish
RUN dotnet publish "GrcMvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app

# Create non-root user for security
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -D appuser

# Copy published application
COPY --from=publish /app/publish .

# Copy static files and resources
COPY wwwroot ./wwwroot
COPY Resources ./Resources

# Set ownership
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:80/health/live || exit 1

# Entry point
ENTRYPOINT ["dotnet", "GrcMvc.dll"]
