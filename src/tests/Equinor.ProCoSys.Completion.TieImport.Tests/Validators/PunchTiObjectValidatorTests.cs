using Equinor.ProCoSys.Completion.TieImport.Validators;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests.Validators;

[TestClass]
public sealed class PunchTiObjectValidatorTests
{
    private PunchTiObjectValidator _dut = null!;

    [TestInitialize]
    public void Setup() => _dut = new PunchTiObjectValidator();

    [TestMethod]
    public void Validate_ShouldFail_WhenMethodIsInvalid()
    {
        // Arrange
        var tiObject = new TIObject { Method = "DELETE", Project = "Project1" };
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains("Method must be either")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenProjectIsNullOrEmpty()
    {
        // Arrange
        var tiObject = new TIObject { Project = null };
        tiObject.Project = null;

        // Act
        var result = _dut.Validate(tiObject);

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
        var result = _dut.Validate(tiObject);

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
        var result = _dut.Validate(tiObject);

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
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.FormType)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenStatusNeitherPAOrPB()
    {
        // Arrange
        var tiObject = new TIObject();
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PD");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Status)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WithManyErrors()
    {
        // Arrange
        var tiObject = new TIObject();

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Project)));
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.TagNo)));
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ExternalPunchItemNo)));
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.FormType)));
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Status)));
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenAllRequiredAttributesArePresent_ForCreate()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "CREATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.Description, "Description");
        tiObject.AddAttribute(PunchObjectAttributes.RaisedByOrganization, "ORG");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedByOrganization, "ORG");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenClearedByIsSetForCreate()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "CREATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "user@example.com");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains("CLEAREDBY") && e.ErrorMessage.Contains("CREATE")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenExternalPunchItemNoIsMissingForUpdate()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "UPDATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        //tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "12345");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ExternalPunchItemNo) && e.ErrorMessage.Contains("missing")));
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenAllRequiredAttributesArePresent_ForUpdate()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "UPDATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "12345");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenClearedByIsSetForUpdate()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "UPDATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "12345");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "user@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "2025- 12 -15 ");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }
}
