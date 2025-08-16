#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CurrencyService.Api/CurrencyService.Api.csproj", "CurrencyService.Api/"]
COPY ["CurrencyService.Application/CurrencyService.Application.csproj", "CurrencyService.Application/"]
COPY ["CurrencyService.Domain/CurrencyService.Domain.csproj", "CurrencyService.Domain/"]
COPY ["CurrencyService.Infrasturucture/CurrencyService.Infrasturucture.csproj", "CurrencyService.Infrasturucture/"]
RUN dotnet restore "./CurrencyService.Api/CurrencyService.Api.csproj"
COPY . .
WORKDIR "/src/CurrencyService.Api"
RUN dotnet build "./CurrencyService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CurrencyService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "CurrencyService.Api.dll"]