using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.RejectPunchItem;

[TestClass]
public class RejectPunchItemCommandValidatorTests : PunchItemCommandTestsBase
{
    private RejectPunchItemCommandValidator _dut;
    private ILabelValidator _labelValidatorMock;
    private RejectPunchItemCommand _command;
    private readonly string _rejectLabelText = "Reject";

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new RejectPunchItemCommand(_existingPunchItem[TestPlantA].Guid, "c", [], "r")
        {
            PunchItem = _existingPunchItem[TestPlantA],
            CheckListDetailsDto = new CheckListDetailsDto(
                _existingPunchItem[TestPlantA].CheckListGuid,
                "R",
                false,
                _existingPunchItem[TestPlantA].Project.Guid)
        };

        _command.PunchItem.Clear(_currentPerson);

        _labelValidatorMock = Substitute.For<ILabelValidator>();
        _labelValidatorMock.ExistsAsync(_rejectLabelText, default).Returns(true);

        var optionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        optionsMock.CurrentValue.Returns(
            new ApplicationOptions
            {
                RejectLabel = _rejectLabelText
            });

        _dut = new RejectPunchItemCommandValidator(_labelValidatorMock, optionsMock);
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
    public async Task Validate_ShouldFail_When_PunchItemNotCleared()
    {
        // Arrange
        _command.PunchItem.Unclear();

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item can not be rejected. The punch item is not cleared!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemIsVerified()
    {
        // Arrange
        _command.PunchItem.Verify(_currentPerson);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item can not be rejected. The punch item is verified!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_RejectLabelNotExists()
    {
        // Arrange
        _labelValidatorMock.ExistsAsync(_rejectLabelText, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"The required Label '{_rejectLabelText}' is not available"));
    }
}
