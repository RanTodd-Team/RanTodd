FROM mcr.microsoft.com/dotnet/sdk:5.0.402 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY Ranker/*.csproj ./
RUN dotnet restore

# Copy source code and build
COPY Ranker ./
RUN dotnet build -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0.11-alpine3.13
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Ranker.dll"]
