# Use the .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory for the build context
WORKDIR /src

# Copy the entire repository contents into the Docker image
COPY . .

# Set the working directory to where the .csproj file is located
WORKDIR /src/WarriorsServer/WarriorsServer

# Run dotnet restore using the specific .csproj file
RUN dotnet restore WarriorsServer.csproj

# Build and publish the project to the /out directory
RUN dotnet publish WarriorsServer.csproj -c Release -o /out

# Use the .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "WarriorsServer.dll"]