# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Sadece OrderService bağımlılıklarını kopyala (cache-friendly)
COPY shared/Contracts/SmartOps.Contracts.csproj shared/Contracts/
COPY services/OrderService/src/OrderService.Domain/OrderService.Domain.csproj services/OrderService/src/OrderService.Domain/
COPY services/OrderService/src/OrderService.Infrastructure/OrderService.Infrastructure.csproj services/OrderService/src/OrderService.Infrastructure/
COPY services/OrderService/src/OrderService.Application/OrderService.Application.csproj services/OrderService/src/OrderService.Application/
COPY services/OrderService/src/OrderService.API/OrderService.API.csproj services/OrderService/src/OrderService.API/

# Restore
RUN dotnet restore services/OrderService/src/OrderService.API/OrderService.API.csproj

# Tüm kaynakları kopyala
COPY . .

# Publish
WORKDIR /src/services/OrderService/src/OrderService.API
RUN dotnet publish OrderService.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "OrderService.API.dll"]
