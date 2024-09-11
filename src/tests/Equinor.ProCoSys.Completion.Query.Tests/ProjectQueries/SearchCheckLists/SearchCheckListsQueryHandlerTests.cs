using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Query.ProjectQueries.SearchCheckLists;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.ProjectQueries.SearchCheckLists;

[TestClass]
public class SearchCheckListsQueryHandlerTests
{
    private SearchCheckListsQueryHandler _dut;
    private readonly ICheckListApiService _checkListServiceMock = Substitute.For<ICheckListApiService>();
    private SearchCheckListsQuery _query;
    private readonly Guid _projectGuid = Guid.NewGuid();
    private ProCoSys4CheckListSearchResult _psc4SearchResultDto;

    [TestInitialize]
    public void Setup()
    {
        IEnumerable<ProCoSys4CheckListSearchDto> items = new []
        {
            new ProCoSys4CheckListSearchDto(Guid.NewGuid(), "Tag1", "CommPkg1", "McPkg1", "FT1", "FG1", "PA", "RC1", "TRC1", "TRD1", "TFC1", "TFD1", 1, 1, 1),
            new ProCoSys4CheckListSearchDto(Guid.NewGuid(), "Tag2", "CommPkg2", "McPkg2", "FT2", "FG2", "PB", "RC2", "TRC2", "TRD2", "TFC2", "TFD2", 2, 2, 2)
        };
        _psc4SearchResultDto = new ProCoSys4CheckListSearchResult(items, 45);
        _dut = new SearchCheckListsQueryHandler(_checkListServiceMock);
    }

    [TestMethod]
    public async Task Handle_Should_ReturnSearchResult_WithNoneFilters()
    {
        // Arrange
        _query = new SearchCheckListsQuery(_projectGuid, null, null, null, null, null, null);
        _checkListServiceMock
            .SearchCheckListsAsync(_projectGuid, null, null, null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(_psc4SearchResultDto);

        // Act
        var result = await _dut.Handle(_query, CancellationToken.None);

        // Assert
        AssertResultDto(result);
    }

    [TestMethod]
    public async Task Handle_Should_ReturnSearchResult_WithFiltering()
    {
        // Arrange
        _query = new SearchCheckListsQuery(_projectGuid, "X", "RC", "REG/TF", "FT", 2, 10);
        _checkListServiceMock
            .SearchCheckListsAsync(_projectGuid, "X", "RC", "REG", "TF", "FT", 2, 10, Arg.Any<CancellationToken>())
            .Returns(_psc4SearchResultDto);

        // Act
        var result = await _dut.Handle(_query, CancellationToken.None);

        // Assert
        AssertResultDto(result);
    }

    private void AssertResultDto(SearchResultDto searchResultDto)
    {
        Assert.IsNotNull(searchResultDto);
        Assert.AreEqual(_psc4SearchResultDto.MaxAvailable, searchResultDto.MaxAvailable);
        Assert.AreEqual(_psc4SearchResultDto.Items.Count(), searchResultDto.Items.Count());
        AssertSearchDto(_psc4SearchResultDto.Items.ElementAt(0), searchResultDto.Items.ElementAt(0));
        AssertSearchDto(_psc4SearchResultDto.Items.ElementAt(1), searchResultDto.Items.ElementAt(1));
    }

    private void AssertSearchDto(ProCoSys4CheckListSearchDto expteced, CheckListDto actual)
    {
        Assert.AreEqual(expteced.ResponsibleCode, actual.ResponsibleCode);
        Assert.AreEqual(expteced.RevisionNo, actual.RevisionNo);
        Assert.AreEqual(expteced.SheetNo, actual.SheetNo);
        Assert.AreEqual(expteced.Status, actual.Status);
        Assert.AreEqual(expteced.SubSheetNo, actual.SubSheetNo);
        Assert.AreEqual(expteced.TagFunctionCode, actual.TagFunctionCode);
        Assert.AreEqual(expteced.TagFunctionDescription, actual.TagFunctionDescription);
        Assert.AreEqual(expteced.TagRegisterCode, actual.TagRegisterCode);
        Assert.AreEqual(expteced.TagRegisterDescription, actual.TagRegisterDescription);
        Assert.AreEqual(expteced.CheckListGuid, actual.CheckListGuid);
        Assert.AreEqual(expteced.CommPkgNo, actual.CommPkgNo);
        Assert.AreEqual(expteced.McPkgNo, actual.McPkgNo);
        Assert.AreEqual(expteced.TagNo, actual.TagNo);
    }
}
