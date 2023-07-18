FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Equinor.ProCoSys.Completion.WebApi/Equinor.ProCoSys.Completion.WebApi.csproj", "Equinor.ProCoSys.Completion.WebApi/"]
COPY ["Equinor.ProCoSys.Completion.Command/Equinor.ProCoSys.Completion.Command.csproj", "Equinor.ProCoSys.Completion.Command/"]
COPY ["Equinor.ProCoSys.Completion.Domain/Equinor.ProCoSys.Completion.Domain.csproj", "Equinor.ProCoSys.Completion.Domain/"]
COPY ["Equinor.ProCoSys.Completion.MessageContracts/Equinor.ProCoSys.Completion.MessageContracts.csproj", "Equinor.ProCoSys.Completion.MessageContracts/"]
COPY ["Equinor.ProCoSys.Completion.Infrastructure/Equinor.ProCoSys.Completion.Infrastructure.csproj", "Equinor.ProCoSys.Completion.Infrastructure/"]
COPY ["Equinor.ProCoSys.Completion.Query/Equinor.ProCoSys.Completion.Query.csproj", "Equinor.ProCoSys.Completion.Query/"]
COPY nuget.config .
#RUN dotnet restore "Equinor.ProCoSys.Completion.WebApi/Equinor.ProCoSys.Completion.WebApi.csproj"
COPY . .
WORKDIR "/src/Equinor.ProCoSys.Completion.WebApi"

# Build
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED true
ARG FEED_TOKEN
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://statoildeveloper.pkgs.visualstudio.com/_packaging/ProCoSysOfficial/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_TOKEN}\"}]}"
RUN wget -qO- https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | bash
RUN echo $VSS_NUGET_EXTERNAL_FEED_ENDPOINTS
#RUN dotnet restore "Equinor.ProCoSys.Completion.WebApi.csproj"
RUN dotnet build "Equinor.ProCoSys.Completion.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Equinor.ProCoSys.Completion.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Equinor.ProCoSys.Completion.WebApi.dll"]
