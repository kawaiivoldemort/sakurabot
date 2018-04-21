############################################################
# Dockerfile to build ASP .NET Core Images
############################################################

FROM microsoft/aspnetcore-build AS builder
WORKDIR /source

# Copy the Project Metadata and download dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the App Source Code and build it
COPY . .
COPY appsettings*.json ./
RUN dotnet publish --output /app/ --configuration Release

FROM microsoft/aspnetcore
WORKDIR /app

COPY --from=builder /app .

EXPOSE 5000:80

ENTRYPOINT ["dotnet", "Sakura.dll"]