using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

public class SqlStatementBuilderTestsBase
{
    protected IPcs4Repository _oracleDBExecutorMock;

    protected readonly string _sourceObjectNameMissingConfig = "NotInConfiguration";

    protected readonly Guid _testGuid = new Guid("805519D7-0DB6-44B7-BF99-A0818CEA778E");
    protected readonly Guid _testGuid2 = new Guid("11111111-2222-3333-4444-555555555555");

    protected readonly string _testOnlyForInsert = "testOnlyForInsert";
    protected readonly string _testString = "test";
    protected readonly DateTime _testDate = new DateTime(2023, 11, 29, 10, 20, 30);
    protected readonly DateTime _testDate2 = new DateTime(2023, 11, 30, 10, 20, 30);
    protected readonly bool _testBool = true;
    protected readonly int _testInt = 1234;
    protected NestedSourceTestObject _nestedObject;
    protected readonly Guid _woGuid = new Guid("11111111-2222-3333-4444-555555555556");
    protected readonly Guid _swcrGuid = new Guid("11111111-2222-3333-4444-555555555557");
    protected readonly Guid _personOid = new Guid("11111111-2222-3333-4444-555555555558");
    protected readonly Guid _documentGuid = new Guid("11111111-2222-3333-4444-555555555559");

    protected SourceTestObject _sourceTestObject;
    protected SourceTestObjectMissingPrimaryKey _sourceTestObjectMissingPrimaryKey;

    protected TestObjectMappingConfig _testObjectMappingConfig = new();
    protected TestObjectMissingPropMappingConfig _testObjectMissingPropMappingConfig = new();


    [TestInitialize]
    public void SqlStatementBuilderTestsBaseSetup()
    {
        _oracleDBExecutorMock = Substitute.For<IPcs4Repository>();
        _nestedObject = new NestedSourceTestObject(_testGuid2);
        _sourceTestObject = new SourceTestObject(_testOnlyForInsert, _testGuid, _testString, _testDate, _testDate2, _testBool, _testInt, _nestedObject, _woGuid, _swcrGuid, _personOid, _documentGuid);
        _sourceTestObjectMissingPrimaryKey = new SourceTestObjectMissingPrimaryKey(null, _testString, _testDate, _testDate2, _testBool, _testInt, _nestedObject, _woGuid, _swcrGuid, _personOid, _documentGuid);
    }
}
