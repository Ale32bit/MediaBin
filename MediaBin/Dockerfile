#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MediaBin/MediaBin.csproj", "MediaBin/"]
RUN dotnet restore "MediaBin/MediaBin.csproj"
COPY . .
WORKDIR "/src/MediaBin"
RUN dotnet build "MediaBin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MediaBin.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MediaBin.dll"]