############################################################
# Dockerfile to build ASP .NET Core Images
############################################################

FROM microsoft/aspnetcore-build AS builder
WORKDIR /SakuraBot

# Copy the Source Files and download dependencies
COPY ./SakuraBot .
COPY ./OpenDotaApi /OpenDotaApi
RUN dotnet restore

# Build it
RUN dotnet publish --output /app/ --configuration Release

FROM microsoft/aspnetcore
WORKDIR /app

COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Sakura.dll"]