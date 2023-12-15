namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

/**
 * This class is used to test missing property in source object
 */
public class TestObjectMissingPropMappingConfig : ISourceObjectMappingConfig
{
    public string TargetTable { get; } = "TestTargetObject";

    public PropertyMapping PrimaryKey { get; } = new PropertyMapping("TestGuid", "TestGuid", PropertyType.Guid, null);

    public TestObjectMissingPropMappingConfig() => PropertyMappings = new List<PropertyMapping>
        {
            new PropertyMapping("PropMissing", "PropMissing", PropertyType.String,   null)
        };

    public List<PropertyMapping> PropertyMappings { get; }
}
