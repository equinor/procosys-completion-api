using Equinor.ProCoSys.Completion.TieImport.Validators;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests.Validators;

[TestClass]
public sealed class PunchTiObjectValidatorTests
{
    private PunchTiObjectValidator _dut = null!;

    [TestInitialize]
    public void Setup() => _dut = new PunchTiObjectValidator();

    #region Helper Methods

    private static TIObject CreateValidCreateTiObject()
    {
        var tiObject = new TIObject { Project = "Project1", Method = "CREATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.Description, "Description");
        tiObject.AddAttribute(PunchObjectAttributes.RaisedByOrganization, "ORG");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedByOrganization, "ORG");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");
        return tiObject;
    }

    private static TIObject CreateValidUpdateTiObject()
    {
        var tiObject = new TIObject { Project = "Project1", Method = "UPDATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");
        return tiObject;
    }

    private static TIObject CreateValidAppendTiObject()
    {
        var tiObject = new TIObject { Project = "Project1", Method = "APPEND" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");
        return tiObject;
    }

    #endregion

    #region Method Validation Tests

    [TestMethod]
    public void Validate_ShouldPass_WhenMethodIsCreate()
    {
        // Arrange
        var tiObject = CreateValidCreateTiObject();

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenMethodIsUpdate()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenMethodIsAppend()
    {
        // Arrange
        var tiObject = CreateValidAppendTiObject();

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenMethodIsDelete()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.Method = "DELETE";

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains("Method must be either")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenMethodIsInvalid()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.Method = "INVALID";

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains("Method must be either")));
    }

    #endregion

    #region Required Attributes Tests

    [TestMethod]
    public void Validate_ShouldFail_WhenProjectIsNull()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.Project = null;

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Project)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenProjectIsEmpty()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.Project = "";

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Project)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenTagNoIsMissing()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "UPDATE" };
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.TagNo)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenExternalPunchItemNoIsMissing()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "UPDATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ExternalPunchItemNo)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenFormTypeIsMissing()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "UPDATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.FormType)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenResponsibleIsMissing()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "UPDATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Responsible)));
    }

    #endregion

    #region Status Validation Tests

    [TestMethod]
    public void Validate_ShouldPass_WhenStatusIsPA()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenStatusIsPB()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.Status, "PB");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenStatusIsInvalid()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.Status, "PC");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Status)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenStatusIsMissing()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "UPDATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Status)));
    }

    #endregion

    #region CREATE-Specific Validation Tests

    [TestMethod]
    public void Validate_ShouldFail_WhenDescriptionIsMissingForCreate()
    {
        // Arrange
        var tiObject = CreateValidCreateTiObject();
        // Remove Description by creating without it
        tiObject = new TIObject { Project = "Project1", Method = "CREATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.RaisedByOrganization, "ORG");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedByOrganization, "ORG");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Description) && e.ErrorMessage.Contains("CREATE")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenRaisedByOrganizationIsMissingForCreate()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "CREATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.Description, "Description");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedByOrganization, "ORG");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.RaisedByOrganization) && e.ErrorMessage.Contains("CREATE")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenClearedByOrganizationIsMissingForCreate()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "CREATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.Description, "Description");
        tiObject.AddAttribute(PunchObjectAttributes.RaisedByOrganization, "ORG");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ClearedByOrganization) && e.ErrorMessage.Contains("CREATE")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenClearedByIsSetForCreate()
    {
        // Arrange
        var tiObject = CreateValidCreateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "user@example.com");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ClearedBy) && e.ErrorMessage.Contains("CREATE")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenClearedDateIsSetForCreate()
    {
        // Arrange
        var tiObject = CreateValidCreateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "2024-01-15");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ClearedDate) && e.ErrorMessage.Contains("CREATE")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenVerifiedByIsSetForCreate()
    {
        // Arrange
        var tiObject = CreateValidCreateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.VerifiedBy, "user@example.com");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.VerifiedBy) && e.ErrorMessage.Contains("CREATE")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenVerifiedDateIsSetForCreate()
    {
        // Arrange
        var tiObject = CreateValidCreateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.VerifiedDate, "2024-01-15");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.VerifiedDate) && e.ErrorMessage.Contains("CREATE")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenRejectedByIsSetForCreate()
    {
        // Arrange
        var tiObject = CreateValidCreateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.RejectedBy, "user@example.com");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.RejectedBy) && e.ErrorMessage.Contains("CREATE")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenRejectedDateIsSetForCreate()
    {
        // Arrange
        var tiObject = CreateValidCreateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.RejectedDate, "2024-01-15");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.RejectedDate) && e.ErrorMessage.Contains("CREATE")));
    }

    #endregion

    #region PunchItemNo Validation Tests

    [TestMethod]
    public void Validate_ShouldPass_WhenPunchItemNoIsValidNumber()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "12345");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenPunchItemNoIsNotProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        // PunchItemNo not added

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenPunchItemNoHasWhitespace()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "  12345  ");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenPunchItemNoIsNotANumber()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "ABC");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.PunchItemNo)));
    }

    #endregion

    #region MaterialRequired Validation Tests

    [TestMethod]
    public void Validate_ShouldPass_WhenMaterialRequiredIsY()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialRequired, "Y");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenMaterialRequiredIsN()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialRequired, "N");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenMaterialRequiredIsLowerCase()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialRequired, "y");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenMaterialRequiredIsNotProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenMaterialRequiredIsInvalid()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialRequired, "Yes");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.MaterialRequired)));
    }

    #endregion

    #region Date Format Validation Tests

    [TestMethod]
    public void Validate_ShouldPass_WhenDueDateIsValidFormat()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.DueDate, "2024-01-15");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenDueDateIsNullMarker()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.DueDate, "{NULL}");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenDueDateIsNotProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenDueDateHasWhitespace()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.DueDate, "  2024-01-15  ");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenDueDateIsInvalidFormat()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.DueDate, "15-01-2024");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.DueDate)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenDueDateIsIsoFormat()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.DueDate, "2024-01-15T10:30:00");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.DueDate)));
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenMaterialEtaIsValidFormat()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialEta, "2024-06-30");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenMaterialEtaIsNullMarker()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialEta, "{NULL}");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenMaterialEtaIsInvalidFormat()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialEta, "30/06/2024");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.MaterialEta)));
    }

    #endregion

    #region ClearedBy/ClearedDate Pair Validation Tests

    [TestMethod]
    public void Validate_ShouldPass_WhenBothClearedByAndClearedDateAreProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "user@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "2024-01-15");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenNeitherClearedByNorClearedDateAreProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenClearedByIsProvidedWithoutClearedDate()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "user@example.com");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ClearedDate) && e.ErrorMessage.Contains("required")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenClearedDateIsProvidedWithoutClearedBy()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "2024-01-15");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ClearedBy) && e.ErrorMessage.Contains("required")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenClearedDateIsInvalidFormat()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "user@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "15-01-2024");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ClearedDate) && e.ErrorMessage.Contains("format")));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenClearedDateIsNullMarker()
    {
        // Arrange - ClearedDate should NOT allow {NULL} when ClearedBy is provided
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "user@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "{NULL}");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ClearedDate)));
    }

    #endregion

    #region VerifiedBy and RejectedBy Mutual Exclusion Tests

    [TestMethod]
    public void Validate_ShouldFail_WhenBothVerifiedByAndRejectedByAreProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.VerifiedBy, "verified@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.VerifiedDate, "2024-01-15");
        tiObject.AddAttribute(PunchObjectAttributes.RejectedBy, "rejected@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.RejectedDate, "2024-01-16");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.VerifiedBy) && e.ErrorMessage.Contains(PunchObjectAttributes.RejectedBy) && e.ErrorMessage.Contains("cannot both be set")));
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenOnlyVerifiedByIsProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.VerifiedBy, "verified@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.VerifiedDate, "2024-01-15");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenOnlyRejectedByIsProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.RejectedBy, "rejected@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.RejectedDate, "2024-01-16");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    #endregion

    #region Multiple Errors Tests

    [TestMethod]
    public void Validate_ShouldCollectAllErrors_WhenMultipleValidationsFail()
    {
        // Arrange
        var tiObject = new TIObject { Method = "INVALID" };

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Count > 1, "Should have multiple validation errors");
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains("Method")));
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Project)));
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Status)));
    }

    [TestMethod]
    public void Validate_ShouldCollectAllCreateErrors_WhenMultipleCreateValidationsFail()
    {
        // Arrange
        var tiObject = new TIObject { Project = "Project1", Method = "CREATE" };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "Punch1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "Type1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");
        // Missing: Description, RaisedByOrganization, ClearedByOrganization

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Description)));
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.RaisedByOrganization)));
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.ClearedByOrganization)));
    }

    #endregion

    #region Whitespace Handling Tests

    [TestMethod]
    public void Validate_ShouldPass_WhenClearedDateHasWhitespace()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "user@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "  2024-01-15  ");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenDateHasInnerWhitespace()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "user@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "2024 - 01 - 15");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    #endregion

    #region Description Length Validation Tests

    [TestMethod]
    public void Validate_ShouldPass_WhenDescriptionIsWithinMaxLength()
    {
        // Arrange
        var tiObject = CreateValidCreateTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.Description, new string('a', 3500));

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var tiObject = CreateValidCreateTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.Description, new string('a', 3501));

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Description) && e.ErrorMessage.Contains("3500")));
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenDescriptionIsNotProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        // Description not added (valid for UPDATE)

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    #endregion

    #region SWCR Number Validation Tests

    [TestMethod]
    public void Validate_ShouldPass_WhenSwcrNoIsValidInteger()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.SwcrNo, "12345");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenSwcrNoIsNullMarker()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.SwcrNo, "{NULL}");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenSwcrNoIsNotProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        // SwcrNo not added

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenSwcrNoHasWhitespace()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.SwcrNo, "  123  ");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenSwcrNoIsNotAValidInteger()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.SwcrNo, "ABC");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.SwcrNo)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenSwcrNoIsDecimal()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.SwcrNo, "123.45");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.SwcrNo)));
    }

    #endregion

    #region Estimate Validation Tests

    [TestMethod]
    public void Validate_ShouldPass_WhenEstimateIsValidInteger()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.Estimate, "100");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenEstimateIsNullMarker()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.Estimate, "{NULL}");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenEstimateIsNotProvided()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        // Estimate not added

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenEstimateIsNotAValidInteger()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.Estimate, "ABC");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Estimate)));
    }

    [TestMethod]
    public void Validate_ShouldFail_WhenEstimateIsDecimal()
    {
        // Arrange
        var tiObject = CreateValidUpdateTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.Estimate, "10.5");

        // Act
        var result = _dut.Validate(tiObject);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.ErrorMessage.Contains(PunchObjectAttributes.Estimate)));
    }

    #endregion
}
