FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/AdminService/AdminService.API/AdminService.API.csproj", "Services/AdminService/AdminService.API/"]
COPY ["Services/AdminService/AdminService.Application/AdminService.Application.csproj", "Services/AdminService/AdminService.Application/"]
COPY ["Services/AdminService/AdminService.Domain/AdminService.Domain.csproj", "Services/AdminService/AdminService.Domain/"]
COPY ["Services/AdminService/AdminService.Infrastructure/AdminService.Infrastructure.csproj", "Services/AdminService/AdminService.Infrastructure/"]
COPY ["Shared/QuickBite.Shared/QuickBite.Shared.csproj", "Shared/QuickBite.Shared/"]
RUN dotnet restore "Services/AdminService/AdminService.API/AdminService.API.csproj"

COPY . .
WORKDIR "/src/Services/AdminService/AdminService.API"
RUN dotnet publish "AdminService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AdminService.API.dll"]
