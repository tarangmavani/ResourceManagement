FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/ResourceManagement.API/ResourceManagement.API.csproj", "src/ResourceManagement.API/"]
COPY ["src/ResourceManagement.Application/ResourceManagement.Application.csproj", "src/ResourceManagement.Application/"]
COPY ["src/ResourceManagement.Infrastructure/ResourceManagement.Infrastructure.csproj", "src/ResourceManagement.Infrastructure/"]
COPY ["src/ResourceManagement.Domain/ResourceManagement.Domain.csproj", "src/ResourceManagement.Domain/"]

RUN dotnet restore "src/ResourceManagement.API/ResourceManagement.API.csproj"

# Copy all source
COPY . .
WORKDIR "/src/src/ResourceManagement.API"
RUN dotnet build "ResourceManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ResourceManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ResourceManagement.API.dll"]
