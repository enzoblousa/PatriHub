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

# Containers com limite baixo de instâncias inotify (ex.: Render free tier) derrubam o app na
# inicialização: o ASP.NET Core tenta observar appsettings.json pra hot-reload por padrão, e
# WebApplication.CreateBuilder() estoura esse limite antes mesmo do nosso código rodar
# (System.IO.IOException: "configured user limit (128) on the number of inotify instances").
# Não precisamos de hot-reload em produção — config muda por variável de ambiente + redeploy,
# nunca editando o arquivo em runtime. Duas formas de desligar, pra garantir que uma pegue
# independente de qual prefixo de env var o bootstrap do host lê primeiro.
ENV DOTNET_hostBuilder__reloadConfigOnChange=false
ENTRYPOINT ["dotnet", "PatriHub.Api.dll", "--hostBuilder:reloadConfigOnChange=false"]
