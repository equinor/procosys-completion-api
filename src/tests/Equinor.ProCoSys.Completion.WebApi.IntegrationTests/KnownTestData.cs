using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class KnownTestData
{
    public KnownTestData(string plant) => Plant = plant;

    public string Plant { get; }

    public static Guid ProjectGuidA => new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static string ProjectNameA => "TestProject A";
    public static string ProjectDescriptionA => "Test - Project A";
    public static string PunchA => "PunchItem-A";
    public static Guid ProjectGuidB => new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static string ProjectNameB => "TestProject B";
    public static string ProjectDescriptionB => "Test - Project B";
    public static string PunchB => "PunchItem-B";

    public Guid PunchAGuid { get; set; }
    public Guid PunchBGuid { get; set; }
    public Guid LinkInPunchAGuid { get; set; }
    public Guid CommentInPunchAGuid { get; set; }
    public Guid AttachmentInPunchAGuid { get; set; }
}
