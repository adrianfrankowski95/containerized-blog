# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# serves only documentation purposes, indicates which port should be exposed
EXPOSE 7001

# copy csproj and restore as distinct layers
WORKDIR /src
COPY ["Services/Discovery/Discovery.API/Discovery.API.csproj", "Services/Discovery/Discovery.API/"]
RUN dotnet restore "Services/Discovery/Discovery.API/Discovery.API.csproj"

# copy everything else and build app
COPY . .
WORKDIR "/src/Services/Discovery/Discovery.API"
RUN dotnet publish "Discovery.API.csproj" -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .

ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "Discovery.API.dll"]