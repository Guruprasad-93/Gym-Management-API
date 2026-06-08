# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Backend/GymManagementSystem.sln ./
COPY Backend/Gym.API/Gym.API.csproj Backend/Gym.API/
COPY Backend/Gym.Application/Gym.Application.csproj Backend/Gym.Application/
COPY Backend/Gym.Domain/Gym.Domain.csproj Backend/Gym.Domain/
COPY Backend/Gym.Infrastructure/Gym.Infrastructure.csproj Backend/Gym.Infrastructure/

RUN dotnet restore Backend/Gym.API/Gym.API.csproj

COPY Backend/ ./Backend/
RUN dotnet publish Backend/Gym.API/Gym.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Gym.API.dll"]
