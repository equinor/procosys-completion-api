using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.FormularTypes;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.Responsibles;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.TagFunctions;
using Equinor.ProCoSys.Completion.Query.CheckListQueries.GetDuplicateInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.CheckListQueries.GetDuplicateInfo;

[TestClass]
public class GetDuplicateInfoQueryHandlerTests
{
    private GetDuplicateInfoQuery _query;
    private GetDuplicateInfoQueryHandler _dut;
    private readonly Guid _checkListGuid = Guid.NewGuid();
    private readonly IPlantProvider _plantProviderMock = Substitute.For<IPlantProvider>();
    private readonly IFormularTypeApiService _formularTypeServiceMock = Substitute.For<IFormularTypeApiService>();
    private readonly IResponsibleApiService _responsibleServiceMock = Substitute.For<IResponsibleApiService>();
    private readonly ITagFunctionApiService _tagFunctionServiceMock = Substitute.For<ITagFunctionApiService>();
    private readonly string _testPlant = "PCS$P";

    [TestInitialize]
    public void Setup_OkState()
    {
        _query = new GetDuplicateInfoQuery(_checkListGuid)
        {
            CheckListDetailsDto = new(_checkListGuid, "FT", "RC", "TRC", "TRD", "TFC", "TFD", Guid.NewGuid())
        };

        _plantProviderMock.Plant.Returns(_testPlant);
        _formularTypeServiceMock.GetAllAsync(_testPlant, Arg.Any<CancellationToken>())
            .Returns([]);
        _responsibleServiceMock.GetAllAsync(_testPlant, Arg.Any<CancellationToken>())
            .Returns([]);
        _tagFunctionServiceMock.GetAllAsync(_testPlant, Arg.Any<CancellationToken>())
            .Returns([]);

        _dut = new GetDuplicateInfoQueryHandler(_plantProviderMock, _formularTypeServiceMock, _responsibleServiceMock, _tagFunctionServiceMock);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnCorrectInfo_WithoutResponsibles_WithoutTagFunctions()
    {
        // Act
        var result = await _dut.Handle(_query, default);

        // Assert
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.AreEqual(_query.CheckListDetailsDto, result.Data.CheckList);
        Assert.IsNotNull(result.Data.FormularTypes);
        Assert.IsNotNull(result.Data.Responsibles);
        Assert.IsNotNull(result.Data.TagFunctions);
        Assert.AreEqual(0, result.Data.FormularTypes.Count());
        Assert.AreEqual(0, result.Data.Responsibles.Count());
        Assert.AreEqual(0, result.Data.TagFunctions.Count());
    }

    [TestMethod]
    public async Task Handle_ShouldReturnCorrectInfo_WithResponsibles_WithTagFunctions()
    {
        var formularType1 = new ProCoSys4FormularType("T1", "R1", "G1");
        var formularType2 = new ProCoSys4FormularType("T2", "R2", "G2");
        _formularTypeServiceMock.GetAllAsync(_testPlant, Arg.Any<CancellationToken>())
            .Returns([formularType1, formularType2]);

        var responsible1 = new ProCoSys4Responsible("C1", "D1");
        var responsible2 = new ProCoSys4Responsible("C2", "D2");
        _responsibleServiceMock.GetAllAsync(_testPlant, Arg.Any<CancellationToken>())
            .Returns([responsible1, responsible2]);

        var tagFunction1 = new ProCoSys4TagFunction("TFC1", "TFD1", "RC1", "RD1");
        var tagFunction2 = new ProCoSys4TagFunction("TFC2", "TFD2", "RC2", "RD2");
        _tagFunctionServiceMock.GetAllAsync(_testPlant, Arg.Any<CancellationToken>())
            .Returns([tagFunction1, tagFunction2]);

        // Act
        var result = await _dut.Handle(_query, default);

        // Assert
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.AreEqual(_query.CheckListDetailsDto, result.Data.CheckList);
        Assert.IsNotNull(result.Data.FormularTypes);
        Assert.IsNotNull(result.Data.Responsibles);
        Assert.IsNotNull(result.Data.TagFunctions);
        Assert.AreEqual(2, result.Data.FormularTypes.Count());
        Assert.AreEqual(2, result.Data.Responsibles.Count());
        Assert.AreEqual(2, result.Data.TagFunctions.Count());

        AssertResponsible(responsible1, result.Data.Responsibles.ElementAt(0));
        AssertResponsible(responsible2, result.Data.Responsibles.ElementAt(1));

        AssertTagFunction(tagFunction1, result.Data.TagFunctions.ElementAt(0));
        AssertTagFunction(tagFunction2, result.Data.TagFunctions.ElementAt(1));
    }

    private void AssertResponsible(ProCoSys4Responsible expected, ResponsibleDto actual)
    {
        Assert.AreEqual(expected.Code, actual.Code);
        Assert.AreEqual(expected.Description, actual.Description);
    }

    private void AssertTagFunction(ProCoSys4TagFunction expected, TagFunctionDto actual)
    {
        Assert.AreEqual(expected.ToString(), actual.RegisterAndTagFunctionCode);
        Assert.AreEqual(expected.Description, actual.TagFunctionDescription);
    }
}
