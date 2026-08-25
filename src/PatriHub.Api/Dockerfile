FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PatriHub.slnx ./
COPY src/PatriHub.Domain/PatriHub.Domain.csproj src/PatriHub.Domain/
COPY src/PatriHub.Application/PatriHub.Application.csproj src/PatriHub.Application/
COPY src/PatriHub.Infrastructure/PatriHub.Infrastructure.csproj src/PatriHub.Infrastructure/
COPY src/PatriHub.Api/PatriHub.Api.csproj src/PatriHub.Api/
RUN dotnet restore src/PatriHub.Api/PatriHub.Api.csproj

COPY src/ src/
RUN dotnet publish src/PatriHub.Api/PatriHub.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "PatriHub.Api.dll"]
