FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/IpConnectTracker.WriterService/IpConnectTracker.WriterService.csproj", "IpConnectTracker.WriterService/"]
COPY ["src/IpConnectTracker.WriterService.DataAccess.PostgreSQL/IpConnectTracker.WriterService.DataAccess.PostgreSQL.csproj", "IpConnectTracker.WriterService.DataAccess.PostgreSQL/"]
COPY ["src/IpConnectTracker.WriterService.DataAccess.Abstractions/IpConnectTracker.WriterService.DataAccess.Abstractions.csproj", "IpConnectTracker.WriterService.DataAccess.Abstractions/"]
RUN dotnet restore "IpConnectTracker.WriterService/IpConnectTracker.WriterService.csproj"
COPY src/. .
WORKDIR "/src/IpConnectTracker.WriterService"
RUN dotnet build "./IpConnectTracker.WriterService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IpConnectTracker.WriterService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IpConnectTracker.WriterService.dll"]
