# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Questlog.Api/Questlog.Api.csproj", "src/Questlog.Api/"]
COPY ["src/Questlog.Application/Questlog.Application.csproj", "src/Questlog.Application/"]
COPY ["src/Questlog.Domain/Questlog.Domain.csproj", "src/Questlog.Domain/"]
COPY ["src/Questlog.Infrastructure/Questlog.Infrastructure.csproj", "src/Questlog.Infrastructure/"]
RUN dotnet restore "src/Questlog.Api/Questlog.Api.csproj"

COPY . .
RUN dotnet publish "src/Questlog.Api/Questlog.Api.csproj" -c Release -o /app/publish

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Questlog.Api.dll"]
