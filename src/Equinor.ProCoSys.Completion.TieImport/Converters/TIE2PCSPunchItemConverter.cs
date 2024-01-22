using Equinor.ProCoSys.Completion.TieImport.Infrastructure;
using Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Converters;
public class TIE2PCSPunchItemConverter : ITIE2PCSConverter
{
    public static PcsPunchItemIn PopulateProCoSysPunchItemImportObject(TIObject tieObject)
    {
        var pcsObject = new PcsPunchItemIn();

        TIEPCSCommonConverters.UpdatePcsObjectFromTieAttributes(pcsObject, tieObject.Attributes);
        
        SetNameBasedOnTieObjectIfNotSet(tieObject, pcsObject);

        SetProjectBasedOnTieObjectIfNotSet(tieObject, pcsObject);

        return pcsObject;
    }

    private static void SetProjectBasedOnTieObjectIfNotSet(TIObject tieObject, PcsPunchItemIn pcsObject)
    {
        if (string.IsNullOrWhiteSpace(pcsObject.Project))
        {
            pcsObject.Project = tieObject.Project;
        }
    }

    private static void SetNameBasedOnTieObjectIfNotSet(TIObject tieObject, PcsPunchItemIn pcsObject)
    {
        if (string.IsNullOrWhiteSpace(pcsObject.Name))
        {
            pcsObject.Name = tieObject.ObjectName;
        }
    }
}
