# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
WORKDIR /src
COPY ["Services/Emailing/Emailing.API/Emailing.API.csproj", "Services/Emailing/Emailing.API/"]
RUN dotnet restore "Services/Emailing/Emailing.API/Emailing.API.csproj"

# copy everything else and build app
COPY . .
WORKDIR "/src/Services/Emailing/Emailing.API"
RUN dotnet publish "Emailing.API.csproj" -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .

ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "Emailing.API.dll"]
