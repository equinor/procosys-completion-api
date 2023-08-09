using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class KnownPlantData
{
    public static string PlantA => "PCS$PLANT_A";
    public static string PlantB => "PCS$PLANT_B";
    public static string PlantATitle => "Plant A";
    public static string PlantBTitle => "Plant B";

    public static Dictionary<string, Guid> ProjectGuidA = new()
    {
        { PlantA, new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa11") },
        { PlantB, new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa22") }
    };

    public static Dictionary<string, Guid> ProjectGuidB = new()
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

    public static Dictionary<string, Guid> PriorityGuid = new()
    {
        { PlantA, new("eeeeeeee-eeee-eeee-eeee-eeeeeeeeaa11") },
        { PlantB, new("eeeeeeee-eeee-eeee-eeee-eeeeeeeeaa22") }
    };

    public static Dictionary<string, Guid> SortingGuid = new()
    {
        { PlantA, new("ffffffff-ffff-ffff-ffff-ffffffffaa11") },
        { PlantB, new("ffffffff-ffff-ffff-ffff-ffffffffaa22") }
    };

    public static Dictionary<string, Guid> TypeGuid = new()
    {
        { PlantA, new("aaaaaaaa-aaaa-aaaa-aaaa-bbbbbbbbbb11") },
        { PlantB, new("aaaaaaaa-aaaa-aaaa-aaaa-bbbbbbbbbb22") }
    };

    public static Dictionary<string, Guid> ChecklistGuid = new()
    {
        { PlantA, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb11") },
        { PlantB, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb22") }
    };
}
