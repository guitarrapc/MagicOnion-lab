#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MagicOnionLab.Shared/Directory.Build.props", "src/MagicOnionLab.Shared/"]
COPY ["src/MagicOnionLab.Shared/Directory.Build.targets", "src/MagicOnionLab.Shared/"]
COPY ["src/MagicOnionLab.Server/MagicOnionLab.Server.csproj", "src/MagicOnionLab.Server/"]
COPY ["src/MagicOnionLab.Shared/MagicOnionLab.Shared.csproj", "src/MagicOnionLab.Shared/"]
RUN dotnet restore "./src/MagicOnionLab.Server/MagicOnionLab.Server.csproj"
COPY . .
WORKDIR "/src/src/MagicOnionLab.Server"
RUN dotnet build "./MagicOnionLab.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MagicOnionLab.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MagicOnionLab.Server.dll"]