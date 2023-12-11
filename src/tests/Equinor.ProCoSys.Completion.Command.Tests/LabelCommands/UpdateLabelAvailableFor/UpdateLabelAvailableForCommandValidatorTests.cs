using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.LabelCommands.UpdateLabelAvailableFor;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.LabelCommands.UpdateLabelAvailableFor;

[TestClass]
public class UpdateLabelAvailableForCommandValidatorTests
{
    private UpdateLabelAvailableForCommandValidator _dut;
    private UpdateLabelAvailableForCommand _command;
    private ILabelValidator _labelValidatorMock;
    private ILabelEntityValidator _labelEntityValidatorMock;
    private readonly EntityTypeWithLabels _existingEntityTypeWithLabel = EntityTypeWithLabels.PunchComment;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UpdateLabelAvailableForCommand("A", new List<EntityTypeWithLabels> { _existingEntityTypeWithLabel });
        _labelValidatorMock = Substitute.For<ILabelValidator>();
        _labelValidatorMock.ExistsAsync(_command.Text, default).Returns(true);
        _labelEntityValidatorMock = Substitute.For<ILabelEntityValidator>();
        _labelEntityValidatorMock.ExistsAsync(_existingEntityTypeWithLabel, default).Returns(true);

        _dut = new UpdateLabelAvailableForCommandValidator(_labelValidatorMock, _labelEntityValidatorMock);
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
    public async Task Validate_ShouldFail_When_LabelNotExists()
    {
        // Arrange
        _labelValidatorMock.ExistsAsync(_command.Text, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Label does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_LabelEntityNotExists()
    {
        // Arrange
        _labelEntityValidatorMock.ExistsAsync(_existingEntityTypeWithLabel, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Label entity does not exist!"));
    }
}
