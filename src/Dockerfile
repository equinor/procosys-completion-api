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

#### Restore dependencies
RUN dotnet restore

# Set workdir where main project is
WORKDIR "/src/Equinor.ProCoSys.Completion.WebApi"

# Build
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ARG FEED_ACCESSTOKEN
ARG DB_CONNECTION_STRING
RUN echo $FEED_ACCESSTOKEN
RUN echo $DB_CONNECTION_STRING
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://statoildeveloper.pkgs.visualstudio.com/_packaging/ProCoSysOfficial/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"
RUN echo $VSS_NUGET_EXTERNAL_FEED_ENDPOINTS
RUN wget -qO- https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | bash
####RUN dotnet build "Equinor.ProCoSys.Completion.WebApi.csproj" -c Release
RUN dotnet publish ./Equinor.ProCoSys.Completion.WebApi -c Release -o /app/publish


# Publish the application
FROM build AS publish
WORKDIR "/src/Equinor.ProCoSys.Completion.WebApi"

# Create the migrations bundle
#ENV Global__UseAzureAppConfig="false"
#ENV FEED_ACCESSTOKEN=${FEED_ACCESSTOKEN}
#RUN dotnet tool install --global dotnet-ef
#ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR "/src/Equinor.ProCoSys.Completion.Infrastructure"
#RUN dotnet ef migrations bundle --self-contained --runtime linux-x64 --context CompletionContext -s ../Equinor.ProCoSys.Completion.WebApi -o /app/migrations/bundle.exe
RUN dotnet ef migrations bundle --self-contained --runtime linux-x64 --context CompletionContext -s ../Equinor.ProCoSys.Completion.WebApi -o /app/migrations/bundle.exe


WORKDIR /app

# Copy migration bundle and application from the build stage
COPY --from=build /app/migrations/bundle.exe /app/migrations/
COPY --from=build /app/publish /app/



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

ENV ASPNETCORE_URLS http://+:5000
EXPOSE 5000
WORKDIR /app

# Copy the published files from the build image into this one
# Copy defaults to copying files as root, specify the user that should be the owner
COPY --chown=apprunner:apprunner --from=publish /app/publish .

# Copy the migration bundle and appsettings.json
COPY --chown=apprunner:apprunner --from=publish /app/migrations/bundle.exe /app/migrations/
COPY --chown=apprunner:apprunner --from=publish /app/migrations/appsettings.json /app/migrations/

# Set workdir to /app/migrations to run the migration bundle
WORKDIR /app/migrations

# Run the migration bundle 
CMD ["./bundle.exe", "--connection", "${DB_CONNECTION_STRING}"]

# Set workdir back to /app for running the application
WORKDIR /app

# Define the entrypoint for the application
ENTRYPOINT ["dotnet", "Equinor.ProCoSys.Completion.WebApi.dll"]