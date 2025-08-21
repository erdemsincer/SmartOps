# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Sadece InvoiceService bağımlılıklarını kopyala (cache-friendly)
COPY shared/Contracts/SmartOps.Contracts.csproj shared/Contracts/
COPY services/InvoiceService/src/InvoiceService.Domain/InvoiceService.Domain.csproj services/InvoiceService/src/InvoiceService.Domain/
COPY services/InvoiceService/src/InvoiceService.Infrastructure/InvoiceService.Infrastructure.csproj services/InvoiceService/src/InvoiceService.Infrastructure/
COPY services/InvoiceService/src/InvoiceService.Application/InvoiceService.Application.csproj services/InvoiceService/src/InvoiceService.Application/
COPY services/InvoiceService/src/InvoiceService.API/InvoiceService.API.csproj services/InvoiceService/src/InvoiceService.API/

# Restore
RUN dotnet restore services/InvoiceService/src/InvoiceService.API/InvoiceService.API.csproj

# Tüm kaynakları kopyala
COPY . .

# Publish
WORKDIR /src/services/InvoiceService/src/InvoiceService.API
RUN dotnet publish InvoiceService.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8090
ENV ASPNETCORE_URLS=http://+:8090
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "InvoiceService.API.dll"]
