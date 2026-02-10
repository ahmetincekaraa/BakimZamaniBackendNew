# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5103
ENV ASPNETCORE_HTTP_PORTS=5103


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/KuaforSepeti.API/KuaforSepeti.API.csproj", "src/KuaforSepeti.API/"]
COPY ["src/KuaforSepeti.Application/KuaforSepeti.Application.csproj", "src/KuaforSepeti.Application/"]
COPY ["src/KuaforSepeti.Domain/KuaforSepeti.Domain.csproj", "src/KuaforSepeti.Domain/"]
COPY ["src/KuaforSepeti.Infrastructure/KuaforSepeti.Infrastructure.csproj", "src/KuaforSepeti.Infrastructure/"]
RUN dotnet restore "./src/KuaforSepeti.API/KuaforSepeti.API.csproj"
COPY . .
WORKDIR "/src/src/KuaforSepeti.API"
RUN dotnet build "./KuaforSepeti.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./KuaforSepeti.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Explicitly copy configuration to ensure it exists
COPY ["src/KuaforSepeti.API/appsettings.Production.json", "."]
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "KuaforSepeti.API.dll"]