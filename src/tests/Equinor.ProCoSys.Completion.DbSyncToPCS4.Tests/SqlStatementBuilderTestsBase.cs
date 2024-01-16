using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

public class SqlStatementBuilderTestsBase
{
    protected IPcs4Repository _pcs4Repository;

    protected readonly string SourceObjectNameMissingConfig = "NotInConfiguration";

    protected readonly Guid TestGuid = new("805519D7-0DB6-44B7-BF99-A0818CEA778E");
    protected readonly Guid TestGuid2 = new("11111111-2222-3333-4444-555555555555");

    protected readonly string TestOnlyForInsert = "testOnlyForInsert";
    protected readonly string TestString = "test";
    protected readonly DateTime TestDate = new(2023, 11, 29, 10, 20, 30);
    protected readonly DateTime TestDate2 = new(2023, 11, 30, 10, 20, 30);
    protected readonly bool TestBool = true;
    protected readonly int TestInt = 1234;
    protected NestedSourceTestObject _nestedObject;
    protected readonly Guid WoGuid = new("11111111-2222-3333-4444-555555555556");
    protected readonly Guid SwcrGuid = new("11111111-2222-3333-4444-555555555557");
    protected readonly Guid PersonOid = new("11111111-2222-3333-4444-555555555558");
    protected readonly Guid DocumentGuid = new("11111111-2222-3333-4444-555555555559");

    protected SourceTestObject _sourceTestObject;
    protected SourceTestObjectMissingPrimaryKey _sourceTestObjectMissingPrimaryKey;

    protected TestObjectMappingConfig _testObjectMappingConfig = new();
    protected TestObjectMissingPropMappingConfig _testObjectMissingPropMappingConfig = new();


    [TestInitialize]
    public void SqlStatementBuilderTestsBaseSetup()
    {
        _pcs4Repository = Substitute.For<IPcs4Repository>();
        _nestedObject = new NestedSourceTestObject(TestGuid2);
        _sourceTestObject = new SourceTestObject(TestOnlyForInsert, TestGuid, TestString, TestDate, TestDate2, TestBool, TestInt, _nestedObject, WoGuid, SwcrGuid, PersonOid, DocumentGuid);
        _sourceTestObjectMissingPrimaryKey = new SourceTestObjectMissingPrimaryKey(null, TestString, TestDate, TestDate2, TestBool, TestInt, _nestedObject, WoGuid, SwcrGuid, PersonOid, DocumentGuid);
    }
}
