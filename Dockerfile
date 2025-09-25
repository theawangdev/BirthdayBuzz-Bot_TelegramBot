# Use official .NET runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "./BirthdayBuzz-Bot-NETCore.csproj"
RUN dotnet publish "./BirthdayBuzz-Bot-NETCore.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BirthdayBuzz-Bot-NETCore.dll"]