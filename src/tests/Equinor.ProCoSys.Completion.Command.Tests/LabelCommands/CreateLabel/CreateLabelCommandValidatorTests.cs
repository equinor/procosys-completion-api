using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.LabelCommands.CreateLabel;

[TestClass]
public class CreateLabelCommandValidatorTests
{
    private CreateLabelCommandValidator _dut;
    private CreateLabelCommand _command;
    private ILabelValidator _labelValidatorMock;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new CreateLabelCommand("A");
        _labelValidatorMock = Substitute.For<ILabelValidator>();

        _dut = new CreateLabelCommandValidator(_labelValidatorMock);
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
    public async Task Validate_ShouldFail_When_LabelAlreadyExists()
    {
        // Arrange
        _labelValidatorMock.ExistsAsync(_command.Text, default).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Label already exist!"));
    }
}
