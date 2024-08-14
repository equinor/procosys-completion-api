using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UnverifyPunchItem;

[TestClass]
public class UnverifyPunchItemCommandValidatorTests : PunchItemCommandTestsBase
{
    private UnverifyPunchItemCommandValidator _dut;
    private UnverifyPunchItemCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UnverifyPunchItemCommand(Guid.NewGuid(), "r")
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _command.PunchItem.Clear(_currentPerson);
        _command.PunchItem.Verify(_currentPerson);

        _dut = new UnverifyPunchItemCommandValidator(_checkListValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TagOwningPunchItemIsVoided()
    {
        // Arrange
        _checkListValidatorMock.TagOwningCheckListIsVoidedAsync(_command.PunchItem.CheckListGuid, default)
            .Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Tag owning punch item is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _command.PunchItem.Project.IsClosed = true;

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemNotVerified()
    {
        // Arrange
        _command.PunchItem.Unverify();

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item can not be unverified. The punch item is not verified!"));
    }
}
