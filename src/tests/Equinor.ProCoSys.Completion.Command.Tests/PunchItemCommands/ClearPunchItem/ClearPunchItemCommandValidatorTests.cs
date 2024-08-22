using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.ClearPunchItem;

[TestClass]
public class ClearPunchItemCommandValidatorTests : PunchItemCommandTestsBase
{
    private ClearPunchItemCommandValidator _dut;
    private ClearPunchItemCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new ClearPunchItemCommand(_existingPunchItem[TestPlantA].Guid, "r")
        {
            PunchItem = _existingPunchItem[TestPlantA],
            CheckListDetailsDto = new CheckListDetailsDto(
                _existingPunchItem[TestPlantA].CheckListGuid,
                "R",
                false,
                _existingPunchItem[TestPlantA].Project.Guid)
        };

        _dut = new ClearPunchItemCommandValidator();
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
        _command.CheckListDetailsDto = new CheckListDetailsDto(
            _existingPunchItem[TestPlantA].CheckListGuid,
            "R",
            true,
            _existingPunchItem[TestPlantA].Project.Guid);

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
    public async Task Validate_ShouldFail_When_PunchItemIsAlreadyCleared()
    {
        // Arrange
        _command.PunchItem.Clear(_currentPerson);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item is already cleared!"));
    }
}
