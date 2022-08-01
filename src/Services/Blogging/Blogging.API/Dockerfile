# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
WORKDIR /src
COPY ["Services/Blogging/Blogging.API/Blogging.API.csproj", "Services/Blogging/Blogging.API/"]
COPY ["Services/Blogging/Blogging.Domain/Blogging.Domain.csproj", "Services/Blogging/Blogging.Domain/"]
COPY ["Services/Blogging/Blogging.Infrastructure/Blogging.Infrastructure.csproj", "Services/Blogging/Blogging.Infrastructure/"]
RUN dotnet restore "Services/Blogging/Blogging.API/Blogging.API.csproj"

# copy everything else and build app
COPY . .
WORKDIR "/src/Services/Blogging/Blogging.API"
RUN dotnet publish "Blogging.API.csproj" -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .

ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "Blogging.API.dll"]
