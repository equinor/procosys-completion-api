﻿using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Mappers;
public static class TIEProCoSysMapperCustomMapper
{
    public static void CustomMap(TIObject tieObject, TIInterfaceMessage message)
    {
        //Fill in eventual custom mappings here.
        MAP_STID_Project_Methods(tieObject, message);

    }

    public static void MapRelationsUntilTieMapperGetsFixed(TIObject message)
    {
        //Nothing for Punch
    }

    private static void MAP_STID_Project_Methods(TIObject tieObject, TIInterfaceMessage message)
    {
        //If Project is imported from STID methods should be changed to APPEND
        //2014-Jan-15:[KRS]Added.
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