# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy full source code
COPY . ./

# Publish application
RUN dotnet publish -c Release -o /app/publish

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# 🔥 IMPORTANT FIX: allow external access (LAN + Docker)
ENV ASPNETCORE_URLS=http://+:8080

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose API port
EXPOSE 8080

# Start application
ENTRYPOINT ["dotnet", "RestaurantSystem.dll"]