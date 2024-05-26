#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MagicOnionLab.Shared/Directory.Build.props", "src/MagicOnionLab.Shared/"]
COPY ["src/MagicOnionLab.Shared/Directory.Build.targets", "src/MagicOnionLab.Shared/"]
COPY ["src/MagicOnionLab.Net.Client/MagicOnionLab.Net.Client.csproj", "src/MagicOnionLab.Net.Client/"]
COPY ["src/MagicOnionLab.Shared/MagicOnionLab.Shared.csproj", "src/MagicOnionLab.Shared/"]
RUN dotnet restore "./src/MagicOnionLab.Net.Client/MagicOnionLab.Net.Client.csproj"
COPY . .
WORKDIR "/src/src/MagicOnionLab.Net.Client"
RUN dotnet build "./MagicOnionLab.Net.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MagicOnionLab.Net.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MagicOnionLab.Net.Client.dll"]