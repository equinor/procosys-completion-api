using Equinor.ProCoSys.Completion.TieImport.Infrastructure;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Equinor.TI.CommonLibrary.Mapper.Legacy;
using Microsoft.Extensions.Logging;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public class ImportSchemaMapper : IImportSchemaMapper
{
    private readonly ILogger<ImportSchemaMapper> _logger;
    private readonly LegacySchemaMapper _legacySchemaMapper;

    public ImportSchemaMapper(ILogger<ImportSchemaMapper> logger)
    {
        _logger = logger;
        _legacySchemaMapper = CreateLegacySchemaMapper(new MapperSettings());
    }

    public MappingResult Map(TIInterfaceMessage message)
    {
        try
        {
            var mapResult = _legacySchemaMapper.Map(message);

            LogMapResult(message, mapResult);

            return new MappingResult(mapResult.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Message not mapped: {ex.Message}" +
                $"{Environment.NewLine}" +
                $"{message.AsXmlString()}",
                ex);

            return new MappingResult(ex.ToMessageResult());
        }
    }

    /// <summary>
    /// Creates schema mapper which handle instances of TIInterfaceMessage (legacy message class).
    /// </summary>
    private LegacySchemaMapper CreateLegacySchemaMapper(MapperSettings settings)
    {
        _logger.LogInformation( "Initializing CommonLib LegacySchemaMapper.");

        var appId = settings.ClientId;
        var tenantId = settings.TenantId;
        var appKey = settings.ClientSecret; // YOUR_CLIENT_SECRET - todo: from Key vault

        // Create a source for retrieving schema data from the API. A token provider connection string is needed.
        ISchemaSource source = new ApiSource(new ApiSourceOptions
        {
            TokenProviderConnectionString = $"RunAs=App;AppId={appId};TenantId={tenantId};AppKey={appKey}"
        });

        // Add caching functionality
        source = new CacheWrapper(source, maxCacheAge: TimeSpan.FromDays(settings.CacheDurationDays));

        // Create an instance of a mapper that will map messages between [many] to [1] schema.
        var mapper = new SourceSchemaSelector
        {
            SchemaFromList = settings.SchemaFrom,
            SchemaTo = settings.SchemaTo,
            Source = source
        };

        return new LegacySchemaMapper(mapper);
    }

    /// <summary>
    /// Logs warnings from mapping result, if any.
    /// </summary>
    private void LogMapResult(TIInterfaceMessage message, MapResult<TIInterfaceMessage> mapResult)
    {
        if (!mapResult.HasWarnings)
        {
            return;
        }

        _logger.LogWarning(
            $"Mapping of message with GUID={message.Guid} produced {mapResult.Warnings.Count} warnings: " +
            $"{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, mapResult.Warnings)}");
    }
}
