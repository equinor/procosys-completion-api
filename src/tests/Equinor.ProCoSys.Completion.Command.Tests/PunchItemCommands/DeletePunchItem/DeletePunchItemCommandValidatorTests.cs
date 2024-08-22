using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.DeletePunchItem;

[TestClass]
public class DeletePunchItemCommandValidatorTests : PunchItemCommandTestsBase
{
    private DeletePunchItemCommandValidator _dut;
    private DeletePunchItemCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new DeletePunchItemCommand(_existingPunchItem[TestPlantA].Guid, "r")
        {
            PunchItem = _existingPunchItem[TestPlantA],
            CheckListDetailsDto = new CheckListDetailsDto(
                _existingPunchItem[TestPlantA].CheckListGuid,
                "R",
                false,
                _existingPunchItem[TestPlantA].Project.Guid)
        };

        _dut = new DeletePunchItemCommandValidator();
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
    public async Task Validate_ShouldFail_When_PunchItemIsCleared()
    {
        // Arrange
        _command.PunchItem.Clear(_currentPerson);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item is cleared!"));
    }
}
