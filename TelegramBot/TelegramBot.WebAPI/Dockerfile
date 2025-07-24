FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY TelegramBot.Domain/TelegramBot.Domain.csproj TelegramBot.Domain/
COPY TelegramBot.Application/TelegramBot.Application.csproj TelegramBot.Application/
COPY TelegramBot.Infrastructure/TelegramBot.Infrastructure.csproj TelegramBot.Infrastructure/
COPY TelegramBot.WebAPI/TelegramBot.WebAPI.csproj TelegramBot.WebAPI/

RUN dotnet restore TelegramBot.WebAPI/TelegramBot.WebAPI.csproj

COPY . .

WORKDIR /src/TelegramBot.WebAPI

ARG BUILD_CONFIGURATION=Release
RUN dotnet build "TelegramBot.WebAPI.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TelegramBot.WebAPI.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramBot.WebAPI.dll"]
