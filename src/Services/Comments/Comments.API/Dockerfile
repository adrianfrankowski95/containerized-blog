# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
WORKDIR /src
COPY ["Services/Comments/Comments.API/Comments.API.csproj", "Services/Comments/Comments.API/"]
RUN dotnet restore "Services/Comments/Comments.API/Comments.API.csproj"

# copy everything else and build app
COPY . .
WORKDIR "/src/Services/Comments/Comments.API"
RUN dotnet publish "Comments.API.csproj" -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .

ENV DOTNET_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "Comments.API.dll"]
