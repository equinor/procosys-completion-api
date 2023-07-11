FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Equinor.ProCoSys.Completion.WebApi/Equinor.ProCoSys.Completion.WebApi.csproj", "./"]
COPY ["Equinor.ProCoSys.Completion.Command/Equinor.ProCoSys.Completion.Command.csproj", "./"]
COPY ["Equinor.ProCoSys.Completion.Domain/Equinor.ProCoSys.Completion.Domain.csproj", "./"]
COPY ["Equinor.ProCoSys.Completion.MessageContracts/Equinor.ProCoSys.Completion.MessageContracts.csproj", "./"]
COPY ["Equinor.ProCoSys.Completion.Infrastructure/Equinor.ProCoSys.Completion.Infrastructure.csproj", "./"]
COPY ["Equinor.ProCoSys.Completion.Query/Equinor.ProCoSys.Completion.Query.csproj", "./"]
RUN dotnet restore "Equinor.ProCoSys.Completion.WebApi/Equinor.ProCoSys.Completion.WebApi.csproj"
COPY . .
WORKDIR "/src/Equinor.ProCoSys.Completion.WebApi"
RUN dotnet build "Equinor.ProCoSys.Completion.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Equinor.ProCoSys.Completion.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Equinor.ProCoSys.Completion.WebApi.dll"]
