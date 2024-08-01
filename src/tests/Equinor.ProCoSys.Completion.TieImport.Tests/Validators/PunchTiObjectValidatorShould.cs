using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Validators;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests.Validators;

[TestClass]
public sealed class PunchTiObjectValidatorShould
{
    private PunchTiObjectValidator _validator;

    [TestInitialize]
    public void Setup() => _validator = new PunchTiObjectValidator();

    [TestMethod]
    public void Validate_ShouldFail_WhenProjectIsNullOrEmpty()
    {
        // Arrange
        var tiObject = new TIObject { Project = null };
        tiObject.Project = null;

        // Act
        var result = _validator.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Project)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenTagNoIsNullOrEmpty()
    {
        // Arrange
        var tiObject = new TIObject();
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, null);

        // Act
        var result = _validator.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.TagNo)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenExternalPunchItemNoIsNullOrEmpty()
    {
        // Arrange
        var tiObject = new TIObject();
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, null);

        // Act
        var result = _validator.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ExternalPunchItemNo)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenFormTypeIsNullOrEmpty()
    {
        // Arrange
        var tiObject = new TIObject();
        tiObject.AddAttribute(PunchObjectAttributes.FormType, null);

        // Act
        var result = _validator.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.FormType)));
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenAllRequiredAttributesArePresent()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");

        // Act
        var result = _validator.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }
}
