FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Equinor.ProCoSys.Completion.WebApi/Equinor.ProCoSys.Completion.WebApi.csproj", "Equinor.ProCoSys.Completion.WebApi/"]
COPY ["Equinor.ProCoSys.Completion.Command/Equinor.ProCoSys.Completion.Command.csproj", "Equinor.ProCoSys.Completion.Command/"]
COPY ["Equinor.ProCoSys.Completion.Domain/Equinor.ProCoSys.Completion.Domain.csproj", "Equinor.ProCoSys.Completion.Domain/"]
COPY ["Equinor.ProCoSys.Completion.MessageContracts/Equinor.ProCoSys.Completion.MessageContracts.csproj", "Equinor.ProCoSys.Completion.MessageContracts/"]
COPY ["Equinor.ProCoSys.Completion.Infrastructure/Equinor.ProCoSys.Completion.Infrastructure.csproj", "Equinor.ProCoSys.Completion.Infrastructure/"]
COPY ["Equinor.ProCoSys.Completion.Query/Equinor.ProCoSys.Completion.Query.csproj", "Equinor.ProCoSys.Completion.Query/"]
COPY nuget.config .
COPY . .
WORKDIR "/src/Equinor.ProCoSys.Completion.WebApi"

# Build
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ARG ENV_TYPE
ARG FEED_TOKEN

RUN if [ "$ENV_TYPE" = "local" ]; then \
            FEED_ACCESSTOKEN_DECODED=$FEED_TOKEN; \
        else \
            FEED_ACCESSTOKEN_DECODED=$(echo $FEED_TOKEN | base64 -d); \
        fi && \
    export VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://statoildeveloper.pkgs.visualstudio.com/_packaging/ProCoSysOfficial/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN_DECODED}\"}]}" && \
    wget -qO- https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | bash && \
    dotnet build "Equinor.ProCoSys.Completion.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Equinor.ProCoSys.Completion.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN adduser \
    --uid 1001 \
    --home /app \
    --gecos '' app \
    --disabled-password \
    && chown -R app /app

USER 1001

ENTRYPOINT ["dotnet", "Equinor.ProCoSys.Completion.WebApi.dll"]
