using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class KnownPlantData
{
    public static string PlantA => "PCS$PLANT_A";
    public static string PlantB => "PCS$PLANT_B";
    public static string PlantATitle => "Plant A";
    public static string PlantBTitle => "Plant B";

    public static Dictionary<string, Guid> ProjectGuidWithAccess = new()
    {
        { PlantA, new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa11") },
        { PlantB, new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa22") }
    };

    public static Dictionary<string, Guid> ProjectGuidWithoutAccess = new()
    {
        { PlantA, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbaa11") },
        { PlantB, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbaa22") }
    };

    public static Dictionary<string, Guid> RaisedByOrgGuid = new()
    {
        { PlantA, new("cccccccc-cccc-cccc-cccc-ccccccccaa11") },
        { PlantB, new("cccccccc-cccc-cccc-cccc-ccccccccaa22") }
    };

    public static Dictionary<string, Guid> ClearingByOrgGuid = new()
    {
        { PlantA, new("dddddddd-dddd-dddd-dddd-ddddddddaa11") },
        { PlantB, new("dddddddd-dddd-dddd-dddd-ddddddddaa22") }
    };
}
