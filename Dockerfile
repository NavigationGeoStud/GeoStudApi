FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY ["GeoStud.Api.csproj", "."]
RUN dotnet restore "GeoStud.Api.csproj"

# Копируем весь код и собираем
COPY . .
RUN dotnet build "GeoStud.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GeoStud.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GeoStud.Api.dll"]

