using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.ForeignApiTests.MainApi.CheckList;

[TestClass]
public class CheckListValidatorTests
{
    private ProCoSys4CheckListValidator _dut = null!;
    private ICheckListCache _checkListCacheMock = null!;
    private static readonly Guid s_projectGuid = Guid.NewGuid();
    private readonly ProCoSys4CheckList _proCoSys4CheckList = new("RC", false, s_projectGuid);
    private readonly Guid _checkListGuid = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _checkListCacheMock = Substitute.For<ICheckListCache>();
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, Arg.Any<CancellationToken>()).Returns(_proCoSys4CheckList);
        
        _dut = new ProCoSys4CheckListValidator(_checkListCacheMock);
    }

    [TestMethod]
    public async Task TagOwningCheckListIsVoidedAsync_ShouldReturnFalse_WhenCheckListExistsAndTagNotVoided()
    {
        // Act
        var result = await _dut.TagOwningCheckListIsVoidedAsync(_checkListGuid, default);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task TagOwningCheckListIsVoidedAsync_ShouldReturnTrue_WhenCheckListExistsAndTagIsVoided()
    {
        // Arrange
        _checkListCacheMock.GetCheckListAsync(_checkListGuid, Arg.Any<CancellationToken>()).Returns(new ProCoSys4CheckList("RC", true, Guid.NewGuid()));

        // Act
        var result = await _dut.TagOwningCheckListIsVoidedAsync(_checkListGuid, default);

        // Assert
        Assert.IsTrue(result);
    }
}
