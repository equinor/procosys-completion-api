using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Equinor.TI.CommonLibrary.Mapper.Legacy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public class ImportSchemaMapper : IImportSchemaMapper
{
    private readonly ILogger<ImportSchemaMapper> _logger;
    private readonly LegacySchemaMapper _legacySchemaMapper;

    public ImportSchemaMapper(ILogger<ImportSchemaMapper> logger, IOptionsMonitor<CommonLibOptions> mapperOptions)
    {
        _logger = logger;
        _legacySchemaMapper = CreateLegacySchemaMapper(mapperOptions);
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
            _logger.LogError(ex, "Message not mapped: {ExceptionMessage}{MessageAsXmlString}", 
                ex.Message + Environment.NewLine, message.AsXmlString());
            return new MappingResult(ex.ToMessageResult());
        }
    }


    /// <summary>
    /// Creates schema mapper which handle instances of TIInterfaceMessage (legacy message class).
    /// </summary>
    /// TODO: 106837 Do we still need to use a legacy mapper, or can we use a different mapper?
    private LegacySchemaMapper CreateLegacySchemaMapper(IOptionsMonitor<CommonLibOptions> settings)
    {
        _logger.LogInformation( "Initializing CommonLib LegacySchemaMapper.");

        var appId = settings.CurrentValue.ClientId;
        var tenantId = settings.CurrentValue.TenantId;
        var appKey = settings.CurrentValue.ClientSecret;

        // Create a source for retrieving schema data from the API. A token provider connection string is needed.
        ISchemaSource source = new ApiSource(new ApiSourceOptions
        {
            TokenProviderConnectionString = $"RunAs=App;AppId={appId};TenantId={tenantId};AppKey={appKey}"
        });

        // Add caching functionality
        source = new CacheWrapper(source, maxCacheAge: TimeSpan.FromDays(settings.CurrentValue.CacheDurationDays));

        // Create an instance of a mapper that will map messages between [many] to [1] schema.
        var mapper = new SourceSchemaSelector
        {
            SchemaFromList = settings.CurrentValue.SchemaFrom,
            SchemaTo = settings.CurrentValue.SchemaTo,
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

        _logger.LogWarning("Mapping of message with GUID={MessageGuid} produced {WarningsCount} warnings: {Warnings}",
                message.Guid, mapResult.Warnings.Count + Environment.NewLine, string.Join(Environment.NewLine, mapResult.Warnings));
    }
}
