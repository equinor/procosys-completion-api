# syntax=docker/dockerfile:1.4
# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files separately and restore NuGet packages to create layers. Skip test projects!
COPY ["Equinor.ProCoSys.Completion.WebApi/Equinor.ProCoSys.Completion.WebApi.csproj", "Equinor.ProCoSys.Completion.WebApi/"]
COPY ["Equinor.ProCoSys.Completion.DbSyncToPCS4/Equinor.ProCoSys.Completion.DbSyncToPCS4.csproj", "Equinor.ProCoSys.Completion.DbSyncToPCS4/"]
COPY ["Equinor.ProCoSys.Completion.Command/Equinor.ProCoSys.Completion.Command.csproj", "Equinor.ProCoSys.Completion.Command/"]
COPY ["Equinor.ProCoSys.Completion.Domain/Equinor.ProCoSys.Completion.Domain.csproj", "Equinor.ProCoSys.Completion.Domain/"]
COPY ["Equinor.ProCoSys.Completion.MessageContracts/Equinor.ProCoSys.Completion.MessageContracts.csproj", "Equinor.ProCoSys.Completion.MessageContracts/"]
COPY ["Equinor.ProCoSys.Completion.Infrastructure/Equinor.ProCoSys.Completion.Infrastructure.csproj", "Equinor.ProCoSys.Completion.Infrastructure/"]
COPY ["Equinor.ProCoSys.Completion.Query/Equinor.ProCoSys.Completion.Query.csproj", "Equinor.ProCoSys.Completion.Query/"]
COPY nuget.config .

# Copy full solution
COPY . .

# Set workdir where main project is
WORKDIR "/src/Equinor.ProCoSys.Completion.WebApi"

# Build
ARG AZURE_CLIENT_ID
ARG AZURE_TENANT_ID 
ARG FEED_ACCESSTOKEN

ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED=true
env VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://statoildeveloper.pkgs.visualstudio.com/_packaging/ProCoSysOfficial/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"$(az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv)\"}]}"

# ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://statoildeveloper.pkgs.visualstudio.com/_packaging/ProCoSysOfficial/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"
RUN wget -qO- https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | bash
RUN dotnet build "Equinor.ProCoSys.Completion.WebApi.csproj" -c Release

# Install Azure CLI in the Docker container
RUN apt-get update && apt-get install -y curl lsb-release apt-transport-https ca-certificates
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Login to Azure using the service principal
RUN --mount=type=secret,id=AZURE_CLIENT_SECRET az login --service-principal -u $AZURE_CLIENT_ID -p $(cat /run/secrets/AZURE_CLIENT_SECRET) --tenant $AZURE_TENANT_ID

# Publish the application
FROM build AS publish
#WORKDIR "/src/Equinor.ProCoSys.Completion.WebApi"

# Create the migrations bundle
ENV Global__UseAzureAppConfig="false"
ENV FEED_ACCESSTOKEN=${FEED_ACCESSTOKEN}
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR "/src/Equinor.ProCoSys.Completion.Infrastructure"
RUN dotnet ef migrations bundle --self-contained --runtime linux-x64 --context CompletionContext -s ../Equinor.ProCoSys.Completion.WebApi -o /app/migrations/bundle.exe

# Copy necessary configuration files for running the migration bundle
# Assuming that 'appsettings.dev.json' is in the WebApi project directory
COPY ["Equinor.ProCoSys.Completion.WebApi/appsettings.json", "/app/migrations/appsettings.json"]

# Publish the web application
WORKDIR "/src/Equinor.ProCoSys.Completion.WebApi"
RUN dotnet publish "Equinor.ProCoSys.Completion.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Define the image used for the final result
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Create non-root user. Set uid to 9999 to avoid conflicts with host OS just in case
RUN adduser --disabled-password --uid 9999 --gecos "" apprunner

# Create the folder and set the non-root as owner
RUN mkdir /app && chown apprunner.apprunner /app

# Change the user from root to non-root- From now on, all Docker commands are run as non-root user (except for COPY)
USER 9999

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
WORKDIR /app

# Copy the published files from the build image into this one
# Copy defaults to copying files as root, specify the user that should be the owner
COPY --chown=apprunner:apprunner --from=publish /app/publish .

# Copy the migration bundle and appsettings.json
COPY --chown=apprunner:apprunner --from=publish /app/migrations/bundle.exe /app/migrations/
COPY --chown=apprunner:apprunner --from=publish /app/migrations/appsettings.json /app/migrations/

# Copy entrypoint script
COPY --chmod=755 startup.sh /app/startup.sh

# Define the entrypoint for the application
ENTRYPOINT ["/app/startup.sh"]