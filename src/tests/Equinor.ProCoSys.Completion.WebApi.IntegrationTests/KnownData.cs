using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public static class KnownData
{
    public static string LabelA => "A";
    public static string LabelB => "B";
    public static string MailTemplateA => "A";
    public static string MailTemplateB => "B";

    // A label with this text must be seeded and correspond to the label configured via
    // appsettings.json, key Application.RejectLabel
    public static string LabelReject => "Reject";

    public static EntityTypeWithLabel EntityTypeWithLabels => EntityTypeWithLabel.PunchComment;

    public static string PlantA => "PCS$PLANT_A";
    public static string PlantB => "PCS$PLANT_B";
    public static string PlantATitle => "Plant A";
    public static string PlantBTitle => "Plant B";

    public static string OriginalWorkOrderNo4 => "004";
    public static string OriginalWorkOrderNo5 => "005";

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

    public static Dictionary<string, Guid> OriginalWorkOrderGuid = new()
    {
        { PlantA, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb33") },
        { PlantB, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb44") }
    };

    public static Dictionary<string, Guid> WorkOrderGuid = new()
    {
        { PlantA, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb55") },
        { PlantB, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb66") }
    };

    public static Dictionary<string, Guid> SWCRGuid = new()
    {
        { PlantA, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb77") },
        { PlantB, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb88") }
    };

    public static Dictionary<string, Guid> DocumentGuid = new()
    {
        { PlantA, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb99") },
        { PlantB, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb00") }
    };

    public static Dictionary<string, Guid> CheckListGuidA = new()
    {
        { PlantA, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbAA") },
        { PlantB, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbBB") }
    };

    public static Dictionary<string, Guid> CheckListGuidB = new()
    {
        { PlantA, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbCC") },
        { PlantB, new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbDD") }
    };
}
