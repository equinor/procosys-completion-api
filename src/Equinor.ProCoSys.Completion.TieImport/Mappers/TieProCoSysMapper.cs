using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;
public class TieProCoSysMapper
{

    public TIObject Map(TIObject tiObject, TIInterfaceMessage message)
    {
        // CUSTOM MAPPING not supported by the TIMapper.
        // All written to be called after the TIEImportMapper call.
        try
        {
            // Common Relations mapping.
            TIEProCoSysMapperCustomMapper.MapRelationsUntilTieMapperGetsFixed(tiObject);
            TIEProCoSysMapperCustomMapper.CustomMap(tiObject, message);
        }
        catch (Exception ex)
        {
            //TODO: JSOI
            //Logger.Debug("Custom Mapper error.", ex);
        }

        return tiObject;
    }
}
