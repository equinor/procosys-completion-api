using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.WebApi.Validators.CheckListValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Validators;

[TestClass]
public class CheckListValidatorTests
{
    private readonly string _plant = "P";
    private CheckListValidator _dut = null!;
    private ICheckListCache _checkListCacheMock = null!;
    private IPlantProvider _plantProviderMock = null!;
    private static readonly Guid s_projectGuid = Guid.NewGuid();
    private readonly ProCoSys4CheckList _proCoSys4CheckList = new("RC", false, s_projectGuid);
    private readonly Guid _checkListGuid = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _checkListCacheMock = Substitute.For<ICheckListCache>();
        _checkListCacheMock.GetCheckListAsync(_plant, _checkListGuid).Returns(_proCoSys4CheckList);
        _plantProviderMock = Substitute.For<IPlantProvider>();
        _plantProviderMock.Plant.Returns(_plant);

        _dut = new CheckListValidator(_checkListCacheMock, _plantProviderMock);
    }

    #region ExistsAsync
    [TestMethod]
    public async Task ExistsAsync_ShouldReturnTrue_WhenCheckListExists()
    {
        // Act
        var result = await _dut.ExistsAsync(_checkListGuid);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_ShouldReturnFalse_WhenCheckListNotExist()
    {
        // Act
        var result = await _dut.ExistsAsync(Guid.Empty);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region TagOwningCheckListIsVoidedAsync
    [TestMethod]
    public async Task TagOwningCheckListIsVoidedAsync_ShouldReturnFalse_WhenCheckListExistsAndTagNotVoided()
    {
        // Act
        var result = await _dut.TagOwningCheckListIsVoidedAsync(_checkListGuid);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task TagOwningCheckListIsVoidedAsync_ShouldReturnTrue_WhenCheckListExistsAndTagIsVoided()
    {
        // Arrange
        _checkListCacheMock.GetCheckListAsync(_plant, _checkListGuid).Returns(new ProCoSys4CheckList("RC", true, Guid.NewGuid()));

        // Act
        var result = await _dut.TagOwningCheckListIsVoidedAsync(_checkListGuid);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task TagOwningCheckListIsVoidedAsync_ShouldReturnFalse_WhenCheckListNotExist()
    {
        // Act
        var result = await _dut.ExistsAsync(Guid.Empty);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region InProjectAsync
    [TestMethod]
    public async Task InProjectAsync_ShouldReturnTrue_WhenCheckListExistsAndProjectMatch()
    {
        // Act
        var result = await _dut.InProjectAsync(_checkListGuid, s_projectGuid);

        // Assert
        Assert.IsTrue(result);
    }
    [TestMethod]
    public async Task InProjectAsync_ShouldReturnFalse_WhenCheckListExistsAndProjectMismatch()
    {
        // Act
        var result = await _dut.InProjectAsync(_checkListGuid, Guid.NewGuid());

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task InProjectAsync_ShouldReturnFalse_WhenCheckListNotExist()
    {
        // Act
        var result = await _dut.ExistsAsync(Guid.Empty);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion
}
