FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/GrcMvc/GrcMvc.csproj", "GrcMvc/"]
RUN dotnet restore "GrcMvc/GrcMvc.csproj"
COPY src/GrcMvc/ GrcMvc/
WORKDIR "/src/GrcMvc"
RUN dotnet build "GrcMvc.csproj" -c Release -o /app/build

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