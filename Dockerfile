# Use the official .NET SDK as the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file
COPY *.csproj ./

# Restore NuGet packages
RUN dotnet restore

# Copy the remaining files
COPY . ./

# Build the application
RUN dotnet publish -c Release -o out

# Use the official .NET runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out .

# Expose the port that the application listens on
EXPOSE 5000

# Set the entry point for the container
ENTRYPOINT ["dotnet", "RecipeGenerator.dll", "--urls=http://+:5000"]
