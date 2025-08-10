# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching
COPY src/Backend/ERP.sln src/Backend/
COPY src/Backend/API.Web/API.Web.csproj src/Backend/API.Web/
COPY src/Backend/Application/Application.csproj src/Backend/Application/
COPY src/Backend/Application.Interfaces/Application.Interfaces.csproj src/Backend/Application.Interfaces/
COPY src/Backend/Core.Domain/Core.Domain.csproj src/Backend/Core.Domain/
COPY src/Backend/Infrastructure/Infrastructure.csproj src/Backend/Infrastructure/

# Restore only with project files
RUN dotnet restore src/Backend/API.Web/API.Web.csproj

# Now copy the rest of the backend sources
COPY src/Backend/ src/Backend/

# Publish API
RUN dotnet publish src/Backend/API.Web/API.Web.csproj -c Release -o /app/out --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy published output
COPY --from=build /app/out .

# Render ortamında portu `Program.cs` dinamik olarak `PORT` env değişkeninden alır.
# Lokal kullanım için varsayılan 10000 portunu expose ediyoruz (Render EXPOSE'a bakmaz).
EXPOSE 10000

ENTRYPOINT ["dotnet", "API.Web.dll"]


