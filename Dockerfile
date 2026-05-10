# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ElectricityPlanner.Api.csproj .
RUN dotnet restore ElectricityPlanner.Api.csproj
COPY . .
RUN dotnet publish ElectricityPlanner.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ElectricityPlanner.Api.dll"]
