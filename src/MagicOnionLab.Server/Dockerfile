#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/MagicOnionLab.Shared/Directory.Build.props", "src/MagicOnionLab.Shared/"]
COPY ["src/MagicOnionLab.Server/MagicOnionLab.Server.csproj", "src/MagicOnionLab.Server/"]
COPY ["src/MagicOnionLab.Shared/MagicOnionLab.Shared.csproj", "src/MagicOnionLab.Shared/"]
RUN dotnet restore "src/MagicOnionLab.Server/MagicOnionLab.Server.csproj"
COPY . .
WORKDIR "/src/src/MagicOnionLab.Server"
RUN dotnet build "MagicOnionLab.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MagicOnionLab.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MagicOnionLab.Server.dll"]