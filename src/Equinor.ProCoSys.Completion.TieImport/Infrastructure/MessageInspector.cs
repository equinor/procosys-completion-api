using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;
public class MessageInspector : IMessageInspector
{
    private readonly ImportOptions _importOptionsDefault;
    private readonly ImportOptions _importOptionsIgnore;

    public MessageInspector()
    {
        _importOptionsDefault = ImportOptionsFromSetting("Statoil.TI.PCSNG.ProCoSysAdapter.ImportOptionsDefault");
        _importOptionsIgnore = ImportOptionsFromSetting("Statoil.TI.PCSNG.ProCoSysAdapter.ImportOptionsIgnore");
    }

    public string? ExtractPlant(TIInterfaceMessage message) => message.IsClass("MODEL") ? null : message.Site;

    public void CheckForScriptInjection(TIObject tieObject)
    {
        // Run through object attributes and make sure that no strings contains HTML Script tags.
        if (tieObject?.Attributes == null)
        {
            return;
        }

        foreach (var attributeValue in tieObject.Attributes
                     .Select(a => a.Value)
                     .Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            if (attributeValue.ToUpper().Contains("<SCRIPT "))
            {
                throw new Exception("Error: Security. Script injection attempted: " + attributeValue);
            }
        }
    }

    public void UpdateImportOptions(IPcsObjectIn pcsObject, TIInterfaceMessage message)
    {
        TIEPCSCommonConverters.UpdatePcsObjectImportOptionsFromTieMetadataAndConfiguration(
            pcsObject,
            message,
            _importOptionsDefault,
            _importOptionsIgnore);
    }

    private static ImportOptions ImportOptionsFromSetting(string key)
        => ImportOptions.FromSettings(
            ConfigurationHelper
                .SplitSettingOrNone(key)
                .Select(s => s.ToUpper())
                .ToList());
}
