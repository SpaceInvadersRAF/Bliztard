FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

RUN mkdir -p /bliztard && \
    chown -R $APP_UID:$APP_UID /bliztard && \
    chmod -R 755 /bliztard

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bliztard.Application/Bliztard.Application.csproj", "Bliztard.Application/"]
COPY ["Bliztard.Contract/Bliztard.Contract.csproj", "Bliztard.Contract/"]
COPY ["Bliztard.Persistence/Bliztard.Persistence.csproj", "Bliztard.Persistence/"]
COPY ["Bliztard.Master/Bliztard.Master.csproj", "Bliztard.Master/"]
RUN dotnet restore "Bliztard.Master/Bliztard.Master.csproj"
COPY . .
WORKDIR "/src/Bliztard.Master"
RUN dotnet build "Bliztard.Master.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Bliztard.Master.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Bliztard.Master.dll"]
