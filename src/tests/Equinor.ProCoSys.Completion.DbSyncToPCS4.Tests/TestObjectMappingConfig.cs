namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

public class TestObjectMappingConfig : ISourceObjectMappingConfig
{
    public string TargetTable { get; } = "TestTargetTable";

    public PropertyMapping PrimaryKey { get; } = new PropertyMapping("TestGuid", "TestGuid", PropertyType.Guid, null);

    public TestObjectMappingConfig() => PropertyMappings = new List<PropertyMapping>
        {
            new PropertyMapping("TestString",       "TestString",       PropertyType.String,    null),
            new PropertyMapping("TestDate",         "TestDateWithTime", PropertyType.DateTime,  null),
            new PropertyMapping("TestDate2",        "TestDate",         PropertyType.DateTime,  ValueConversion.DateTimeToDate),
            new PropertyMapping("TestBool",         "TestBool",         PropertyType.Bool,      null),
            new PropertyMapping("TestInt",          "TestInt",          PropertyType.Int,       null),
            new PropertyMapping("NestedObject.Guid", "TestLibId",       PropertyType.Guid,      ValueConversion.GuidToLibId),
            new PropertyMapping("WoGuid",           "WoGuidLibId",      PropertyType.Guid,      ValueConversion.GuidToWorkOrderId),
            new PropertyMapping("SwcrGuid",         "SwcrLibId",        PropertyType.Guid,      ValueConversion.GuidToSWCRId),
            new PropertyMapping("PersonOID",        "PersonOid",        PropertyType.Guid,      ValueConversion.OidToPersonId),
            new PropertyMapping("DocumentGuid",     "DocumentId",       PropertyType.Guid,      ValueConversion.GuidToDocumentId),
        };

    public List<PropertyMapping> PropertyMappings { get; }
}
