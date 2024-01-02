namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

public class TestObjectMappingConfig : ISourceObjectMappingConfig
{
    public string TargetTable { get; } = "TestTargetTable";

    public PropertyMapping PrimaryKey { get; } = new PropertyMapping("TestGuid", PropertyType.Guid, "TestGuid", null, null, false);

    public TestObjectMappingConfig() => PropertyMappings = new List<PropertyMapping>
        {
            new PropertyMapping("TestString",         PropertyType.String,   "TestString",        null, null, false),
            new PropertyMapping("TestDate",           PropertyType.DateTime, "TestDateWithTime",  null, null, false),
            new PropertyMapping("TestDate2",          PropertyType.DateTime, "TestDate",          null, null, false),
            new PropertyMapping("TestBool",           PropertyType.Bool,     "TestBool",          null, null, false),
            new PropertyMapping("TestInt",            PropertyType.Int,      "TestInt",           null, null, false),
            new PropertyMapping("NestedObject.Guid",  PropertyType.Guid,      "TestLibId",        ValueConversion.GuidToLibId, null, false),
            new PropertyMapping("WoGuid",             PropertyType.Guid,     "WoGuidLibId",       ValueConversion.GuidToWorkOrderId, null, false),
            new PropertyMapping("SwcrGuid",           PropertyType.Guid,     "SwcrLibId",         ValueConversion.GuidToSWCRId, null, false),
            new PropertyMapping("PersonOID",          PropertyType.Guid,     "PersonOid",         ValueConversion.OidToPersonId, null, false),
            new PropertyMapping("DocumentGuid",       PropertyType.Guid,     "DocumentId",        ValueConversion.GuidToDocumentId, null, false),
        };

    public List<PropertyMapping> PropertyMappings { get; }
}
