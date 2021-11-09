FROM mcr.microsoft.com/dotnet/sdk:5.0.402 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY RanTodd/*.csproj ./
RUN dotnet restore

# Copy source code and build
COPY RanTodd ./
RUN dotnet build -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0.11
WORKDIR /app
COPY RanTodd/Fonts .
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "RanTodd.dll"]
