# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5103

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/BakimZamani.API/BakimZamani.API.csproj", "src/BakimZamani.API/"]
COPY ["src/BakimZamani.Application/BakimZamani.Application.csproj", "src/BakimZamani.Application/"]
COPY ["src/BakimZamani.Domain/BakimZamani.Domain.csproj", "src/BakimZamani.Domain/"]
COPY ["src/BakimZamani.Infrastructure/BakimZamani.Infrastructure.csproj", "src/BakimZamani.Infrastructure/"]
RUN dotnet restore "./src/BakimZamani.API/BakimZamani.API.csproj"
COPY . .
WORKDIR "/src/src/BakimZamani.API"
RUN dotnet build "./BakimZamani.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BakimZamani.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_HTTP_PORTS=5103
ENTRYPOINT ["dotnet", "BakimZamani.API.dll"]
