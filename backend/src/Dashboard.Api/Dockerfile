FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Dashboard.Domain/Dashboard.Domain.csproj src/Dashboard.Domain/
COPY src/Dashboard.Application/Dashboard.Application.csproj src/Dashboard.Application/
COPY src/Dashboard.Infrastructure/Dashboard.Infrastructure.csproj src/Dashboard.Infrastructure/
COPY src/Dashboard.Api/Dashboard.Api.csproj src/Dashboard.Api/
RUN dotnet restore src/Dashboard.Api/Dashboard.Api.csproj

COPY src/ src/
RUN dotnet publish src/Dashboard.Api/Dashboard.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Dashboard.Api.dll"]
