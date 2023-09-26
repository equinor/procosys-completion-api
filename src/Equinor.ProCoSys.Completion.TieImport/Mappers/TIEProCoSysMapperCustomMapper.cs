using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;
public static class TIEProCoSysMapperCustomMapper
{
    public static void CustomMap(TIObject tieObject, TIInterfaceMessage message)
    {
        //TODO: 106699 Implement other custom mapping functionality
        SetActionAndMethodToAppendForProjectObjectsFromStid(tieObject, message);
    }

    private static void SetActionAndMethodToAppendForProjectObjectsFromStid(TIObject tieObject, TIInterfaceMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.SourceSystem))
        {
            return;
        }

        if (message.SourceSystem.StartsWith("STID") &&
            tieObject.ObjectClass.Equals("PROJECT", StringComparison.InvariantCultureIgnoreCase) &&
            (message.Action == "CREATE" || message.Action == "UPDATE"))
        {
            message.Action = "APPEND";
            tieObject.Method = "APPEND";
        }
    }
}
