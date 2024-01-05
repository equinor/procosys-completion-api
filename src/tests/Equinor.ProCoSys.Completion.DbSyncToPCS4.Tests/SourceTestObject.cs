#nullable enable
namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

public class SourceTestObject(
    string? testOnlyForInsert,
    Guid testGuid,
    string? testString,
    DateTime? testDate,
    DateTime? testDate2,
    bool testBool,
    int? testInt,
    NestedSourceTestObject nestedObject,
    Guid? woGuid,
    Guid? swcrGuid,
    Guid? personOID,
    Guid? documentGuid)
{
    public string? TestOnlyForInsert { get; } = testOnlyForInsert;
    public Guid TestGuid { get; } = testGuid;
    public string? TestString { get; } = testString;
    public DateTime? TestDate { get; } = testDate;
    public DateTime? TestDate2 { get; } = testDate2;
    public bool TestBool { get; } = testBool;
    public int? TestInt { get; } = testInt;
    public NestedSourceTestObject NestedObject { get; } = nestedObject;
    public Guid? WoGuid { get; } = woGuid;
    public Guid? SwcrGuid { get; } = swcrGuid;
    public Guid? PersonOID { get; } = personOID;
    public Guid? DocumentGuid { get; } = documentGuid;
}
