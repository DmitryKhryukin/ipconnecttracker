FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IpConnectTracker.ReaderService.Api/IpConnectTracker.ReaderService.Api.csproj", "IpConnectTracker.ReaderService.Api/"]
COPY ["IpConnectTracker.ReaderService.DataAccess.Abstractions/IpConnectTracker.ReaderService.DataAccess.Abstractions.csproj", "IpConnectTracker.ReaderService.DataAccess.Abstractions/"]
COPY ["IpConnectTracker.ReaderService.DataAccess.PostgreSQL/IpConnectTracker.ReaderService.DataAccess.PostgreSQL.csproj", "IpConnectTracker.ReaderService.DataAccess.PostgreSQL/"]
RUN dotnet restore "IpConnectTracker.ReaderService.Api/IpConnectTracker.ReaderService.Api.csproj"
COPY . .
WORKDIR "/src/IpConnectTracker.ReaderService.Api"
RUN dotnet build "./IpConnectTracker.ReaderService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IpConnectTracker.ReaderService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IpConnectTracker.ReaderService.Api.dll"]
