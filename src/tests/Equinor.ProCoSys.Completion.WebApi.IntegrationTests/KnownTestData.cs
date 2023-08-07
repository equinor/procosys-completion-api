using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class KnownTestData
{
    public KnownTestData(string plant) => Plant = plant;

    public string Plant { get; }

    public static Guid ProjectGuidA => new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static string ProjectNameA => "TestProject A";
    public static string ProjectDescriptionA => "Test - Project A";
    public static string PunchItemA => "PunchItem-A";
    public static Guid ProjectGuidB => new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static string ProjectNameB => "TestProject B";
    public static string ProjectDescriptionB => "Test - Project B";
    public static string PunchItemB => "PunchItem-B";

    public static string RaisedByOrgCode => "COM";
    public static Guid RaisedByOrgGuid => new("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static string ClearingByOrgCode => "ENG";
    public static Guid ClearingByOrgGuid => new("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public Guid PunchItemAGuid { get; set; }
    public Guid PunchItemBGuid { get; set; }
    public Guid LinkInPunchItemAGuid { get; set; }
    public Guid CommentInPunchItemAGuid { get; set; }
    public Guid AttachmentInPunchItemAGuid { get; set; }
}
