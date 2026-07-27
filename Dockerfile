FROM mcr.microsoft.com/dotnet/sdk:9.0.203 AS build
WORKDIR /src

COPY . .

RUN dotnet restore \
    "src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj"

RUN dotnet publish \
    "src/GtMotive.Estimate.Microservice.Host/GtMotive.Estimate.Microservice.Host.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "GtMotive.Estimate.Microservice.Host.dll"]
