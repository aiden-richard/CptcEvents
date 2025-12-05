# Multi-stage Dockerfile for building and running the CptcEvents Razor Pages app targeting .NET 10

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file and restore to leverage Docker layer caching
COPY ["CptcEvents/CptcEvents.csproj", "CptcEvents/"]
RUN dotnet restore "CptcEvents/CptcEvents.csproj"

# Copy the rest of the source and publish
COPY . .
WORKDIR /src/CptcEvents
RUN dotnet publish "CptcEvents.csproj" -c Release -r linux-x64 --self-contained false -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "CptcEvents.dll"]