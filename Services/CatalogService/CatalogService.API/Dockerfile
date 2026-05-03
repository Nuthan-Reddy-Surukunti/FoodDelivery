FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/CatalogService/CatalogService.API/CatalogService.API.csproj", "Services/CatalogService/CatalogService.API/"]
COPY ["Services/CatalogService/CatalogService.Application/CatalogService.Application.csproj", "Services/CatalogService/CatalogService.Application/"]
COPY ["Services/CatalogService/CatalogService.Domain/CatalogService.Domain.csproj", "Services/CatalogService/CatalogService.Domain/"]
COPY ["Services/CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj", "Services/CatalogService/CatalogService.Infrastructure/"]
COPY ["Shared/QuickBite.Shared/QuickBite.Shared.csproj", "Shared/QuickBite.Shared/"]
RUN dotnet restore "Services/CatalogService/CatalogService.API/CatalogService.API.csproj"

COPY . .
WORKDIR "/src/Services/CatalogService/CatalogService.API"
RUN dotnet publish "CatalogService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CatalogService.API.dll"]
