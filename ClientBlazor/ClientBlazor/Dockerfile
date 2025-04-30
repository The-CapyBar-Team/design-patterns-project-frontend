# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY ["ClientBlazor.sln", "./"]
COPY ["ClientBlazor/ClientBlazor.csproj", "ClientBlazor/"]

# Restore NuGet packages
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish the app
WORKDIR "/src/ClientBlazor"
RUN dotnet publish "ClientBlazor.csproj" -c Release -o /app/publish

# Runtime stage
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
EXPOSE 8080

# Copy the nginx configuration file
COPY ClientBlazor/nginx.conf /etc/nginx/nginx.conf

# Copy the published files to the nginx html directory
COPY --from=build /app/publish/wwwroot/ .