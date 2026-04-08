FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Gateway/FoodDelivery.Gateway/FoodDelivery.Gateway.csproj", "Gateway/FoodDelivery.Gateway/"]
RUN dotnet restore "Gateway/FoodDelivery.Gateway/FoodDelivery.Gateway.csproj"

COPY . .
WORKDIR "/src/Gateway/FoodDelivery.Gateway"
RUN dotnet publish "FoodDelivery.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FoodDelivery.Gateway.dll"]
