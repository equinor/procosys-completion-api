using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DuplicatePunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.DuplicatePunchItem;

[TestClass]
public class DuplicatePunchItemCommandValidatorTests : PunchItemCommandTestsBase
{
    private DuplicatePunchItemCommandValidator _dut;
    private DuplicatePunchItemCommand _command;
    private readonly Guid _checkListGuid1 = Guid.NewGuid();
    private readonly Guid _checkListGuid2 = Guid.NewGuid();

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new DuplicatePunchItemCommand(_existingPunchItem[TestPlantA].Guid, [_checkListGuid1, _checkListGuid2], false)
        {
            PunchItem = _existingPunchItem[TestPlantA],
            CheckListDetailsDtoList = [new CheckListDetailsDto(_checkListGuid1, "R", false, _existingPunchItem[TestPlantA].Project.Guid),
                new CheckListDetailsDto(_checkListGuid2, "R", false, _existingPunchItem[TestPlantA].Project.Guid)]
        };

        _dut = new DuplicatePunchItemCommandValidator();
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
    public async Task Validate_ShouldFail_When_TagOwningACheckListIsVoided()
    {
        // Arrange
        _command.CheckListDetailsDtoList =
        [
            new CheckListDetailsDto(_checkListGuid1, "R", false, _existingPunchItem[TestPlantA].Project.Guid),
            new CheckListDetailsDto(_checkListGuid2, "R", true, _existingPunchItem[TestPlantA].Project.Guid)
        ];

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Check lists to copy to can not belong to a voided tag!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectForPunchItemAndCheckListDiffer()
    {
        // Arrange
        _command.CheckListDetailsDtoList =
        [
            new CheckListDetailsDto(_checkListGuid1, "R", false, _existingPunchItem[TestPlantA].Project.Guid),
            new CheckListDetailsDto(_checkListGuid2, "R", false, Guid.NewGuid())
        ];

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item to duplicate and all check lists to copy to must be in same project!"));
    }
}
