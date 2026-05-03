FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Gateway/QuickBite.Gateway/QuickBite.Gateway.csproj", "Gateway/QuickBite.Gateway/"]
RUN dotnet restore "Gateway/QuickBite.Gateway/QuickBite.Gateway.csproj"

COPY . .
WORKDIR "/src/Gateway/QuickBite.Gateway"
RUN dotnet publish "QuickBite.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "QuickBite.Gateway.dll"]
