# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["src/SSPInboundClient/SSPInboundClient.csproj", "src/SSPInboundClient/"]
RUN dotnet restore "src/SSPInboundClient/SSPInboundClient.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/SSPInboundClient"
RUN dotnet build "SSPInboundClient.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SSPInboundClient.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - create runtime image
FROM base AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy published application
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "SSPInboundClient.dll"]