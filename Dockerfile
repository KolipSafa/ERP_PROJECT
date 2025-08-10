# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all sources (monorepo)
COPY . .

# Restore and publish API
RUN dotnet restore src/Backend/API.Web/API.Web.csproj \
 && dotnet publish src/Backend/API.Web/API.Web.csproj -c Release -o /app/out --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy published output
COPY --from=build /app/out .

# Render ortamında portu `Program.cs` dinamik olarak `PORT` env değişkeninden alır.
# Lokal kullanım için varsayılan 10000 portunu expose ediyoruz (Render EXPOSE'a bakmaz).
EXPOSE 10000

ENTRYPOINT ["dotnet", "API.Web.dll"]


