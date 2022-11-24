# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# serves only documentation purposes, indicates which port should be exposed
EXPOSE 7000

# copy csproj and restore as distinct layers
WORKDIR /src
COPY ["Gateways/WebGateway.API/WebGateway.API.csproj", "Gateways/WebGateway.API/"]
RUN dotnet restore "Gateways/WebGateway.API/WebGateway.API.csproj"

# copy everything else and build app
COPY . .
WORKDIR "/src/Gateways/WebGateway.API"
RUN dotnet publish "WebGateway.API.csproj" -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .

ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "WebGateway.API.dll"]
