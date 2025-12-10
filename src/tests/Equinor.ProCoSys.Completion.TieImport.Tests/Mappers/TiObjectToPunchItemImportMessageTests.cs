using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.TieImport.Mappers;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests.Mappers;

[TestClass]
public sealed class TiObjectToPunchItemImportMessageTests
{
    #region Helper Methods

    private static TIObject CreateValidTiObject()
    {
        var tiObject = new TIObject
        {
            Guid = Guid.NewGuid(),
            Method = "CREATE",
            Project = "TestProject",
            Site = "TestPlant"
        };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "FormType1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "EXT-001");
        return tiObject;
    }

    #endregion

    #region Basic Mapping Tests

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapMessageGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        var tiObject = CreateValidTiObject();
        tiObject.Guid = expectedGuid;

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual(expectedGuid, result.MessageGuid);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapMethod()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.Method = "UPDATE";

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual("UPDATE", result.Method);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapProjectName()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.Project = "MyProject";

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual("MyProject", result.ProjectName);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapPlant()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.Site = "PlantA";

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual("PlantA", result.Plant);
    }

    #endregion

    #region Required String Attributes Tests

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapTagNo()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.TagNo, "MyTag");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual("MyTag", result.TagNo);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapFormType()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.FormType, "MyFormType");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual("MyFormType", result.FormType);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapResponsible()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.Responsible, "MC");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual("MC", result.Responsible);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void ToPunchItemImportMessage_ShouldThrow_WhenTagNoIsMissing()
    {
        // Arrange
        var tiObject = new TIObject
        {
            Guid = Guid.NewGuid(),
            Method = "CREATE",
            Project = "TestProject",
            Site = "TestPlant"
        };
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "FormType1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void ToPunchItemImportMessage_ShouldThrow_WhenFormTypeIsMissing()
    {
        // Arrange
        var tiObject = new TIObject
        {
            Guid = Guid.NewGuid(),
            Method = "CREATE",
            Project = "TestProject",
            Site = "TestPlant"
        };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "EQ");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void ToPunchItemImportMessage_ShouldThrow_WhenResponsibleIsMissing()
    {
        // Arrange
        var tiObject = new TIObject
        {
            Guid = Guid.NewGuid(),
            Method = "CREATE",
            Project = "TestProject",
            Site = "TestPlant"
        };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "Tag1");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "FormType1");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);
    }

    #endregion

    #region Status/Category Mapping Tests

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapStatusPA_ToCategory()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.Status, "PA");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual(Category.PA, result.Category);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapStatusPB_ToCategory()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.Status, "PB");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual(Category.PB, result.Category);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNullCategory_WhenStatusIsInvalid()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.Status, "PC");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsNull(result.Category);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNullCategory_WhenStatusIsMissing()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.RemoveAttribute(PunchObjectAttributes.Status);
        // Status not added

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsNull(result.Category);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapStatusCaseInsensitive()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.Status, "pa");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual(Category.PA, result.Category);
    }

    #endregion

    #region Optional String Attributes Tests

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapExternalPunchItemNo()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddOrUpdateAttribute(PunchObjectAttributes.ExternalPunchItemNo, "EXT-123");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.ExternalPunchItemNo.HasValue);
        Assert.AreEqual("EXT-123", result.ExternalPunchItemNo.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapDescription()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.Description, "Test description");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.Description.HasValue);
        Assert.AreEqual("Test description", result.Description.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNoValue_WhenDescriptionIsMissing()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        // Description not added

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsFalse(result.Description.HasValue);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapRaisedByOrganization()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.RaisedByOrganization, "ORG1");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.RaisedByOrganization.HasValue);
        Assert.AreEqual("ORG1", result.RaisedByOrganization.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapClearedByOrganization()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedByOrganization, "ORG2");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.ClearedByOrganization.HasValue);
        Assert.AreEqual("ORG2", result.ClearedByOrganization.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapClearedBy()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "user@example.com");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.ClearedBy.HasValue);
        Assert.AreEqual("user@example.com", result.ClearedBy.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapVerifiedBy()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.VerifiedBy, "verifier@example.com");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.VerifiedBy.HasValue);
        Assert.AreEqual("verifier@example.com", result.VerifiedBy.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapRejectedBy()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.RejectedBy, "rejector@example.com");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.RejectedBy.HasValue);
        Assert.AreEqual("rejector@example.com", result.RejectedBy.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapIsVoided()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.IsVoided, "Y");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.IsVoided.HasValue);
        Assert.AreEqual("Y", result.IsVoided.Value);
    }

    #endregion

    #region PunchItemNo (Long) Mapping Tests

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapPunchItemNo()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "12345");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.PunchItemNo.HasValue);
        Assert.AreEqual(12345L, result.PunchItemNo.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNoValue_WhenPunchItemNoIsMissing()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        // PunchItemNo not added

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsFalse(result.PunchItemNo.HasValue);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNullValue_WhenPunchItemNoIsEmpty()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.PunchItemNo.HasValue);
        Assert.IsNull(result.PunchItemNo.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNullValue_WhenPunchItemNoIsInvalid()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "ABC");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.PunchItemNo.HasValue);
        Assert.IsNull(result.PunchItemNo.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldTrimPunchItemNo()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "  12345  ");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.PunchItemNo.HasValue);
        Assert.AreEqual(12345L, result.PunchItemNo.Value);
    }

    #endregion

    #region MaterialRequired (Bool) Mapping Tests

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapMaterialRequired_Y_ToTrue()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialRequired, "Y");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.MaterialRequired.HasValue);
        Assert.AreEqual(true, result.MaterialRequired.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapMaterialRequired_N_ToFalse()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialRequired, "N");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.MaterialRequired.HasValue);
        Assert.AreEqual(false, result.MaterialRequired.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapMaterialRequired_CaseInsensitive()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialRequired, "y");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.MaterialRequired.HasValue);
        Assert.AreEqual(true, result.MaterialRequired.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNoValue_WhenMaterialRequiredIsMissing()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        // MaterialRequired not added

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsFalse(result.MaterialRequired.HasValue);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldTrimMaterialRequired()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialRequired, "  Y  ");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.MaterialRequired.HasValue);
        Assert.AreEqual(true, result.MaterialRequired.Value);
    }

    #endregion

    #region Date Mapping Tests (Optional<DateTime?>)

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapClearedDate()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "2024-01-15");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.ClearedDate.HasValue);
        Assert.IsNotNull(result.ClearedDate.Value);
        Assert.AreEqual(new DateTime(2024, 1, 15), result.ClearedDate.Value!.Value.Date);
        Assert.AreEqual(DateTimeKind.Utc, result.ClearedDate.Value!.Value.Kind);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapVerifiedDate()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.VerifiedDate, "2024-02-20");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.VerifiedDate.HasValue);
        Assert.IsNotNull(result.VerifiedDate.Value);
        Assert.AreEqual(new DateTime(2024, 2, 20), result.VerifiedDate.Value!.Value.Date);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapRejectedDate()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.RejectedDate, "2024-03-25");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.RejectedDate.HasValue);
        Assert.IsNotNull(result.RejectedDate.Value);
        Assert.AreEqual(new DateTime(2024, 3, 25), result.RejectedDate.Value!.Value.Date);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNoValue_WhenClearedDateIsMissing()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        // ClearedDate not added

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsFalse(result.ClearedDate.HasValue);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNoValue_WhenClearedDateIsEmpty()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsFalse(result.ClearedDate.HasValue);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldHandleWhitespaceInDate()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "  2024-01-15  ");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.ClearedDate.HasValue);
        Assert.IsNotNull(result.ClearedDate.Value);
        Assert.AreEqual(new DateTime(2024, 1, 15), result.ClearedDate.Value!.Value.Date);
    }

    #endregion

    #region Date With Null Marker Mapping Tests (OptionalWithNull<DateTime?>)

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapDueDate()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.DueDate, "2024-06-30");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.DueDate.HasValue);
        Assert.IsFalse(result.DueDate.ShouldBeNull);
        Assert.IsNotNull(result.DueDate.Value);
        Assert.AreEqual(new DateTime(2024, 6, 30), result.DueDate.Value!.Value.Date);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapDueDate_WithNullMarker()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.DueDate, "{NULL}");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.DueDate.HasValue);
        Assert.IsTrue(result.DueDate.ShouldBeNull);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapMaterialEta()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialEta, "2024-12-31");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.MaterialEta.HasValue);
        Assert.IsFalse(result.MaterialEta.ShouldBeNull);
        Assert.IsNotNull(result.MaterialEta.Value);
        Assert.AreEqual(new DateTime(2024, 12, 31), result.MaterialEta.Value!.Value.Date);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapMaterialEta_WithNullMarker()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialEta, "{NULL}");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.MaterialEta.HasValue);
        Assert.IsTrue(result.MaterialEta.ShouldBeNull);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNoValue_WhenDueDateIsMissing()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        // DueDate not added

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsFalse(result.DueDate.HasValue);
    }

    #endregion

    #region String With Null Marker Mapping Tests (OptionalWithNull<string?>)

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapPunchListType()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.PunchListType, "TYPE1");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.PunchListType.HasValue);
        Assert.IsFalse(result.PunchListType.ShouldBeNull);
        Assert.AreEqual("TYPE1", result.PunchListType.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapPunchListType_WithNullMarker()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.PunchListType, "{NULL}");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.PunchListType.HasValue);
        Assert.IsTrue(result.PunchListType.ShouldBeNull);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapMaterialNo()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.MaterialNo, "MAT-001");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.MaterialNo.HasValue);
        Assert.AreEqual("MAT-001", result.MaterialNo.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapActionBy()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.ActionBy, "action@example.com");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.ActionBy.HasValue);
        Assert.AreEqual("action@example.com", result.ActionBy.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapDocumentNo()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.DocumentNo, "DOC-001");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.DocumentNo.HasValue);
        Assert.AreEqual("DOC-001", result.DocumentNo.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapOriginalWorkOrderNo()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.OriginalWorkOrderNo, "WO-ORIG-001");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.OriginalWorkOrderNo.HasValue);
        Assert.AreEqual("WO-ORIG-001", result.OriginalWorkOrderNo.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapWorkOrderNo()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.WorkOrderNo, "WO-001");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.WorkOrderNo.HasValue);
        Assert.AreEqual("WO-001", result.WorkOrderNo.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapPriority()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.Priority, "HIGH");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.Priority.HasValue);
        Assert.AreEqual("HIGH", result.Priority.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapSorting()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.Sorting, "SORT1");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.Sorting.HasValue);
        Assert.AreEqual("SORT1", result.Sorting.Value);
    }

    #endregion

    #region Int With Null Marker Mapping Tests (OptionalWithNull<int?>)

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapEstimate()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.Estimate, "100");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.Estimate.HasValue);
        Assert.IsFalse(result.Estimate.ShouldBeNull);
        Assert.AreEqual(100, result.Estimate.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapEstimate_WithNullMarker()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.Estimate, "{NULL}");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.Estimate.HasValue);
        Assert.IsTrue(result.Estimate.ShouldBeNull);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapSwcrNo()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.SwcrNo, "42");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.SwcrNo.HasValue);
        Assert.IsFalse(result.SwcrNo.ShouldBeNull);
        Assert.AreEqual(42, result.SwcrNo.Value);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapSwcrNo_WithNullMarker()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.SwcrNo, "{NULL}");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.SwcrNo.HasValue);
        Assert.IsTrue(result.SwcrNo.ShouldBeNull);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldReturnNoValue_WhenEstimateIsMissing()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        // Estimate not added

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsFalse(result.Estimate.HasValue);
    }

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldTrimEstimate()
    {
        // Arrange
        var tiObject = CreateValidTiObject();
        tiObject.AddAttribute(PunchObjectAttributes.Estimate, "  50  ");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.IsTrue(result.Estimate.HasValue);
        Assert.AreEqual(50, result.Estimate.Value);
    }

    #endregion

    #region Complete Mapping Test

    [TestMethod]
    public void ToPunchItemImportMessage_ShouldMapAllFields_WhenAllAreProvided()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var tiObject = new TIObject
        {
            Guid = guid,
            Method = "UPDATE",
            Project = "FullProject",
            Site = "FullPlant"
        };
        tiObject.AddAttribute(PunchObjectAttributes.TagNo, "FullTag");
        tiObject.AddAttribute(PunchObjectAttributes.FormType, "FullFormType");
        tiObject.AddAttribute(PunchObjectAttributes.Responsible, "FullResp");
        tiObject.AddAttribute(PunchObjectAttributes.Status, "PB");
        tiObject.AddAttribute(PunchObjectAttributes.ExternalPunchItemNo, "EXT-FULL");
        tiObject.AddAttribute(PunchObjectAttributes.PunchItemNo, "99999");
        tiObject.AddAttribute(PunchObjectAttributes.Description, "Full description");
        tiObject.AddAttribute(PunchObjectAttributes.RaisedByOrganization, "RaisedOrg");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedByOrganization, "ClearedOrg");
        tiObject.AddAttribute(PunchObjectAttributes.PunchListType, "TypeFull");
        tiObject.AddAttribute(PunchObjectAttributes.DueDate, "2024-12-31");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedDate, "2024-01-15");
        tiObject.AddAttribute(PunchObjectAttributes.ClearedBy, "cleared@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.VerifiedDate, "2024-02-20");
        tiObject.AddAttribute(PunchObjectAttributes.VerifiedBy, "verified@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.RejectedDate, "2024-03-25");
        tiObject.AddAttribute(PunchObjectAttributes.RejectedBy, "rejected@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.MaterialRequired, "Y");
        tiObject.AddAttribute(PunchObjectAttributes.MaterialEta, "2024-06-15");
        tiObject.AddAttribute(PunchObjectAttributes.MaterialNo, "MAT-FULL");
        tiObject.AddAttribute(PunchObjectAttributes.ActionBy, "action@example.com");
        tiObject.AddAttribute(PunchObjectAttributes.DocumentNo, "DOC-FULL");
        tiObject.AddAttribute(PunchObjectAttributes.Estimate, "500");
        tiObject.AddAttribute(PunchObjectAttributes.OriginalWorkOrderNo, "WO-ORIG-FULL");
        tiObject.AddAttribute(PunchObjectAttributes.WorkOrderNo, "WO-FULL");
        tiObject.AddAttribute(PunchObjectAttributes.Priority, "HIGH");
        tiObject.AddAttribute(PunchObjectAttributes.Sorting, "SORT-FULL");
        tiObject.AddAttribute(PunchObjectAttributes.SwcrNo, "123");
        tiObject.AddAttribute(PunchObjectAttributes.IsVoided, "N");

        // Act
        var result = TiObjectToPunchItemImportMessage.ToPunchItemImportMessage(tiObject);

        // Assert
        Assert.AreEqual(guid, result.MessageGuid);
        Assert.AreEqual("UPDATE", result.Method);
        Assert.AreEqual("FullProject", result.ProjectName);
        Assert.AreEqual("FullPlant", result.Plant);
        Assert.AreEqual("FullTag", result.TagNo);
        Assert.AreEqual("FullFormType", result.FormType);
        Assert.AreEqual("FullResp", result.Responsible);
        Assert.AreEqual(Category.PB, result.Category);
        Assert.AreEqual("EXT-FULL", result.ExternalPunchItemNo.Value);
        Assert.AreEqual(99999L, result.PunchItemNo.Value);
        Assert.AreEqual("Full description", result.Description.Value);
        Assert.AreEqual("RaisedOrg", result.RaisedByOrganization.Value);
        Assert.AreEqual("ClearedOrg", result.ClearedByOrganization.Value);
        Assert.AreEqual("TypeFull", result.PunchListType.Value);
        Assert.AreEqual(new DateTime(2024, 12, 31), result.DueDate.Value!.Value.Date);
        Assert.AreEqual(new DateTime(2024, 1, 15), result.ClearedDate.Value!.Value.Date);
        Assert.AreEqual("cleared@example.com", result.ClearedBy.Value);
        Assert.AreEqual(new DateTime(2024, 2, 20), result.VerifiedDate.Value!.Value.Date);
        Assert.AreEqual("verified@example.com", result.VerifiedBy.Value);
        Assert.AreEqual(new DateTime(2024, 3, 25), result.RejectedDate.Value!.Value.Date);
        Assert.AreEqual("rejected@example.com", result.RejectedBy.Value);
        Assert.AreEqual(true, result.MaterialRequired.Value);
        Assert.AreEqual(new DateTime(2024, 6, 15), result.MaterialEta.Value!.Value.Date);
        Assert.AreEqual("MAT-FULL", result.MaterialNo.Value);
        Assert.AreEqual("action@example.com", result.ActionBy.Value);
        Assert.AreEqual("DOC-FULL", result.DocumentNo.Value);
        Assert.AreEqual(500, result.Estimate.Value);
        Assert.AreEqual("WO-ORIG-FULL", result.OriginalWorkOrderNo.Value);
        Assert.AreEqual("WO-FULL", result.WorkOrderNo.Value);
        Assert.AreEqual("HIGH", result.Priority.Value);
        Assert.AreEqual("SORT-FULL", result.Sorting.Value);
        Assert.AreEqual(123, result.SwcrNo.Value);
        Assert.AreEqual("N", result.IsVoided.Value);
    }

    #endregion
}
