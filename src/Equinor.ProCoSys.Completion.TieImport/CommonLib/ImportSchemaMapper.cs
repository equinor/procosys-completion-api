using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Equinor.TI.CommonLibrary.Mapper.Legacy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public sealed class ImportSchemaMapper : IImportSchemaMapper
{
    private readonly ILogger<ImportSchemaMapper> _logger;
    private readonly LegacySchemaMapper _legacySchemaMapper;

    public ImportSchemaMapper(
        ILogger<ImportSchemaMapper> logger,
        ISchemaSource schemaSource,
        IOptionsMonitor<CommonLibOptions> commonLibOptions)
    {
        _logger = logger;
        _legacySchemaMapper = CreateLegacySchemaMapper(schemaSource, commonLibOptions);
    }

    public MappingResult Map(TIInterfaceMessage message)
    {
        try
        {
            var mapResult = _legacySchemaMapper.Map(message);
            mapResult.Message.Project = message.Project;

            foreach (var messageObject in mapResult.Message.Objects)
            {
                messageObject.Project ??= message.Objects.FirstOrDefault(x => x.Guid == messageObject.Guid)?.Project;
            }

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
    private LegacySchemaMapper CreateLegacySchemaMapper(
        ISchemaSource source, 
        IOptionsMonitor<CommonLibOptions> commonLibOptions)
    {
        _logger.LogInformation("Initializing CommonLib LegacySchemaMapper");
        var mapper = new SourceSchemaSelector
        {
            SchemaFromList = commonLibOptions.CurrentValue.SchemaFrom,
            SchemaTo = commonLibOptions.CurrentValue.SchemaTo,
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
            message.Guid, mapResult.Warnings.Count + Environment.NewLine,
            string.Join(Environment.NewLine, mapResult.Warnings));
    }
}
