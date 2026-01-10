# Use auth config
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["SakaeELearning.WebAPI.csproj", "."]
RUN dotnet restore "./SakaeELearning.WebAPI.csproj"

# Copy everything else and build
COPY . .
WORKDIR /app
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port 8080 (default for .NET 8)
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV GEMINI_API_KEY=""

ENTRYPOINT ["dotnet", "SakaeELearning.WebAPI_V3.dll"]
