using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.TieImport.Mappers;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.References;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.TieImport.Tests.Mappers;

[TestClass]
public sealed class MapToJsonPatchDocumentTests
{
    private const string TestPlant = "TestPlant";

    private Project _project = null!;
    private LibraryItem _raisedByOrg = null!;
    private LibraryItem _clearedByOrg = null!;
    private LibraryItem _newRaisedByOrg = null!;
    private LibraryItem _newClearedByOrg = null!;
    private LibraryItem _punchListType = null!;
    private LibraryItem _priority = null!;
    private LibraryItem _sorting = null!;
    private WorkOrder _workOrder = null!;
    private WorkOrder _originalWorkOrder = null!;
    private Document _document = null!;
    private SWCR _swcr = null!;
    private Person _actionByPerson = null!;
    private PunchItem _punchItem = null!;
    private Guid _checkListGuid;

    [TestInitialize]
    public void Setup()
    {
        _checkListGuid = Guid.NewGuid();
        _project = new Project(TestPlant, Guid.NewGuid(), "ProjectName", "Description");

        // Create library items
        _raisedByOrg = new LibraryItem(TestPlant, Guid.NewGuid(), "RAISED_ORG", "Raised By Org", LibraryType.COMPLETION_ORGANIZATION);
        _clearedByOrg = new LibraryItem(TestPlant, Guid.NewGuid(), "CLEARED_ORG", "Cleared By Org", LibraryType.COMPLETION_ORGANIZATION);
        _newRaisedByOrg = new LibraryItem(TestPlant, Guid.NewGuid(), "NEW_RAISED_ORG", "New Raised By Org", LibraryType.COMPLETION_ORGANIZATION);
        _newClearedByOrg = new LibraryItem(TestPlant, Guid.NewGuid(), "NEW_CLEARED_ORG", "New Cleared By Org", LibraryType.COMPLETION_ORGANIZATION);
        _punchListType = new LibraryItem(TestPlant, Guid.NewGuid(), "TYPE1", "Punch Type 1", LibraryType.PUNCHLIST_TYPE);
        _priority = new LibraryItem(TestPlant, Guid.NewGuid(), "HIGH", "High Priority", LibraryType.COMM_PRIORITY);
        _sorting = new LibraryItem(TestPlant, Guid.NewGuid(), "SORT1", "Sorting 1", LibraryType.PUNCHLIST_SORTING);

        // Create work orders
        _workOrder = new WorkOrder(TestPlant, Guid.NewGuid(), "WO-001");
        _originalWorkOrder = new WorkOrder(TestPlant, Guid.NewGuid(), "WO-ORIG-001");

        // Create document
        _document = new Document(TestPlant, Guid.NewGuid(), "DOC-001");

        // Create SWCR
        _swcr = new SWCR(TestPlant, Guid.NewGuid(), 123);

        // Create person
        _actionByPerson = new Person(Guid.NewGuid(), "Action", "Person", "action@example.com", "action@example.com", false);

        // Create punch item
        _punchItem = new PunchItem(
            TestPlant,
            _project,
            _checkListGuid,
            Category.PA,
            "Original Description",
            _raisedByOrg,
            _clearedByOrg);
    }

    #region Helper Methods

    private PunchItemImportMessage CreateBaseMessage() => new(
        Guid.NewGuid(),
        "UPDATE",
        "ProjectName",
        TestPlant,
        "TagNo",
        "ExternalPunchItemNo",
        "FormType",
        "EQ",
        new Optional<long?>(),
        new Optional<string?>(),
        new Optional<string?>(),
        Category.PA,
        new OptionalWithNull<string?>(),
        new OptionalWithNull<DateTime?>(),
        new Optional<DateTime?>(),
        new Optional<string?>(),
        new Optional<string?>(),
        new Optional<DateTime?>(),
        new Optional<string?>(),
        new Optional<DateTime?>(),
        new Optional<string?>(),
        new Optional<bool?>(),
        new OptionalWithNull<DateTime?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<int?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<int?>(),
        new Optional<string?>()
    );

    private CommandReferences CreateBaseReferences() => new()
    {
        ProjectGuid = _project.Guid,
        CheckListGuid = _checkListGuid,
        RaisedByOrgGuid = _raisedByOrg.Guid,
        ClearedByOrgGuid = _clearedByOrg.Guid
    };

    private static bool HasOperation(Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<PatchablePunchItem> doc, string path)
        => doc.Operations.Any(o => o.path.Equals($"/{path}", StringComparison.OrdinalIgnoreCase));

    private static Operation<PatchablePunchItem>? GetOperation(Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<PatchablePunchItem> doc, string path)
        => doc.Operations.FirstOrDefault(o => o.path.Equals($"/{path}", StringComparison.OrdinalIgnoreCase));

    #endregion

    #region No Changes Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldReturnEmptyDocument_WhenNoChanges()
    {
        // Arrange
        var message = CreateBaseMessage();
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.AreEqual(0, result.Operations.Count);
    }

    #endregion

    #region Description Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddDescriptionPatch_WhenDescriptionChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { Description = new Optional<string?>("New Description") };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.Description)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.Description));
        Assert.AreEqual("New Description", operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddDescriptionPatch_WhenDescriptionSame()
    {
        // Arrange
        var message = CreateBaseMessage() with { Description = new Optional<string?>("Original Description") };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.Description)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddDescriptionPatch_WhenDescriptionNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // Description not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.Description)));
    }

    #endregion

    #region RaisedByOrg Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddRaisedByOrgPatch_WhenRaisedByOrgChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { RaisedByOrganization = new Optional<string?>("NEW_RAISED_ORG") };
        var references = CreateBaseReferences() with { RaisedByOrgGuid = _newRaisedByOrg.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.RaisedByOrgGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.RaisedByOrgGuid));
        Assert.AreEqual(_newRaisedByOrg.Guid, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddRaisedByOrgPatch_WhenRaisedByOrgSame()
    {
        // Arrange
        var message = CreateBaseMessage() with { RaisedByOrganization = new Optional<string?>(_raisedByOrg.Code) };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.RaisedByOrgGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddRaisedByOrgPatch_WhenRaisedByOrgNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // RaisedByOrganization not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.RaisedByOrgGuid)));
    }

    #endregion

    #region ClearedByOrg Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddClearedByOrgPatch_WhenClearedByOrgChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { ClearedByOrganization = new Optional<string?>("NEW_CLEARED_ORG") };
        var references = CreateBaseReferences() with { ClearedByOrgGuid = _newClearedByOrg.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.ClearingByOrgGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.ClearingByOrgGuid));
        Assert.AreEqual(_newClearedByOrg.Guid, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddClearedByOrgPatch_WhenClearedByOrgSame()
    {
        // Arrange
        var message = CreateBaseMessage() with { ClearedByOrganization = new Optional<string?>(_clearedByOrg.Code) };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.ClearingByOrgGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddClearedByOrgPatch_WhenClearedByOrgNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // ClearedByOrganization not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.ClearingByOrgGuid)));
    }

    #endregion

    #region DueDate Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddDueDatePatch_WhenDueDateChanged()
    {
        // Arrange
        var newDueDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var message = CreateBaseMessage() with { DueDate = new OptionalWithNull<DateTime?>(newDueDate) };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.DueTimeUtc)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.DueTimeUtc));
        Assert.AreEqual(newDueDate, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddDueDatePatch_WhenDueDateSame()
    {
        // Arrange
        var existingDate = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        _punchItem.DueTimeUtc = existingDate;
        var message = CreateBaseMessage() with { DueDate = new OptionalWithNull<DateTime?>(existingDate) };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.DueTimeUtc)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddDueDateNullPatch_WhenDueDateMarkedAsNull()
    {
        // Arrange
        _punchItem.DueTimeUtc = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var message = CreateBaseMessage() with { DueDate = OptionalWithNull<DateTime?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.DueTimeUtc)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.DueTimeUtc));
        Assert.IsNull(operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddDueDatePatch_WhenDueDateNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // DueDate not set
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.DueTimeUtc)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddDueDateNullPatch_WhenDueDateAlreadyNull()
    {
        // Arrange
        _punchItem.DueTimeUtc = null;
        var message = CreateBaseMessage() with { DueDate = OptionalWithNull<DateTime?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.DueTimeUtc)));
    }

    #endregion

    #region PunchListType Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddTypePatch_WhenTypeChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { PunchListType = new OptionalWithNull<string?>(_punchListType.Code) };
        var references = CreateBaseReferences() with { TypeGuid = _punchListType.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.TypeGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.TypeGuid));
        Assert.AreEqual(_punchListType.Guid, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddTypePatch_WhenTypeSame()
    {
        // Arrange
        _punchItem.SetType(_punchListType);
        var message = CreateBaseMessage() with { PunchListType = new OptionalWithNull<string?>(_punchListType.Code) };
        var references = CreateBaseReferences() with { TypeGuid = _punchListType.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.TypeGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddTypePatch_WhenTypeNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // PunchListType not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.TypeGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddTypeNullPatch_WhenTypeMarkedAsNull()
    {
        // Arrange
        _punchItem.SetType(_punchListType);
        var message = CreateBaseMessage() with { PunchListType = OptionalWithNull<string?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.TypeGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.TypeGuid));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region Priority Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddPriorityPatch_WhenPriorityChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { Priority = new OptionalWithNull<string?>(_priority.Code) };
        var references = CreateBaseReferences() with { PriorityGuid = _priority.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.PriorityGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.PriorityGuid));
        Assert.AreEqual(_priority.Guid, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddPriorityPatch_WhenPrioritySame()
    {
        // Arrange
        _punchItem.SetPriority(_priority);
        var message = CreateBaseMessage() with { Priority = new OptionalWithNull<string?>(_priority.Code) };
        var references = CreateBaseReferences() with { PriorityGuid = _priority.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.PriorityGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddPriorityPatch_WhenPriorityNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // Priority not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.PriorityGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddPriorityNullPatch_WhenPriorityMarkedAsNull()
    {
        // Arrange
        _punchItem.SetPriority(_priority);
        var message = CreateBaseMessage() with { Priority = OptionalWithNull<string?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.PriorityGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.PriorityGuid));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region Sorting Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddSortingPatch_WhenSortingChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { Sorting = new OptionalWithNull<string?>(_sorting.Code) };
        var references = CreateBaseReferences() with { SortingGuid = _sorting.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.SortingGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.SortingGuid));
        Assert.AreEqual(_sorting.Guid, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddSortingPatch_WhenSortingSame()
    {
        // Arrange
        _punchItem.SetSorting(_sorting);
        var message = CreateBaseMessage() with { Sorting = new OptionalWithNull<string?>(_sorting.Code) };
        var references = CreateBaseReferences() with { SortingGuid = _sorting.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.SortingGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddSortingPatch_WhenSortingNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // Sorting not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.SortingGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddSortingNullPatch_WhenSortingMarkedAsNull()
    {
        // Arrange
        _punchItem.SetSorting(_sorting);
        var message = CreateBaseMessage() with { Sorting = OptionalWithNull<string?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.SortingGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.SortingGuid));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region ActionBy Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddActionByPatch_WhenActionByChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { ActionBy = new OptionalWithNull<string?>(_actionByPerson.UserName) };
        var references = CreateBaseReferences() with { ActionByPersonOid = _actionByPerson.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.ActionByPersonOid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.ActionByPersonOid));
        Assert.AreEqual(_actionByPerson.Guid, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddActionByPatch_WhenActionBySame()
    {
        // Arrange
        _punchItem.SetActionBy(_actionByPerson);
        var message = CreateBaseMessage() with { ActionBy = new OptionalWithNull<string?>(_actionByPerson.UserName) };
        var references = CreateBaseReferences() with { ActionByPersonOid = _actionByPerson.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.ActionByPersonOid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddActionByPatch_WhenActionByNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // ActionBy not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.ActionByPersonOid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddActionByNullPatch_WhenActionByMarkedAsNull()
    {
        // Arrange
        _punchItem.SetActionBy(_actionByPerson);
        var message = CreateBaseMessage() with { ActionBy = OptionalWithNull<string?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.ActionByPersonOid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.ActionByPersonOid));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region Estimate Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddEstimatePatch_WhenEstimateChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { Estimate = new OptionalWithNull<int?>(100) };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.Estimate)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.Estimate));
        Assert.AreEqual(100, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddEstimatePatch_WhenEstimateSame()
    {
        // Arrange
        _punchItem.Estimate = 100;
        var message = CreateBaseMessage() with { Estimate = new OptionalWithNull<int?>(100) };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.Estimate)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddEstimatePatch_WhenEstimateNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // Estimate not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.Estimate)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddEstimateNullPatch_WhenEstimateMarkedAsNull()
    {
        // Arrange
        _punchItem.Estimate = 50;
        var message = CreateBaseMessage() with { Estimate = OptionalWithNull<int?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.Estimate)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.Estimate));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region MaterialEta Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddMaterialEtaPatch_WhenMaterialEtaChanged()
    {
        // Arrange
        var newMaterialEta = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var message = CreateBaseMessage() with { MaterialEta = new OptionalWithNull<DateTime?>(newMaterialEta) };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.MaterialETAUtc)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.MaterialETAUtc));
        Assert.AreEqual(newMaterialEta, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddMaterialEtaPatch_WhenMaterialEtaSame()
    {
        // Arrange
        var existingDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        _punchItem.MaterialETAUtc = existingDate;
        var message = CreateBaseMessage() with { MaterialEta = new OptionalWithNull<DateTime?>(existingDate) };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.MaterialETAUtc)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddMaterialEtaPatch_WhenMaterialEtaNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // MaterialEta not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.MaterialETAUtc)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddMaterialEtaNullPatch_WhenMaterialEtaMarkedAsNull()
    {
        // Arrange
        _punchItem.MaterialETAUtc = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var message = CreateBaseMessage() with { MaterialEta = OptionalWithNull<DateTime?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.MaterialETAUtc)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.MaterialETAUtc));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region MaterialNo Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddMaterialNoPatch_WhenMaterialNoChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { MaterialNo = new OptionalWithNull<string?>("MAT-001") };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.MaterialExternalNo)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.MaterialExternalNo));
        Assert.AreEqual("MAT-001", operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddMaterialNoPatch_WhenMaterialNoSame()
    {
        // Arrange
        _punchItem.MaterialExternalNo = "MAT-001";
        var message = CreateBaseMessage() with { MaterialNo = new OptionalWithNull<string?>("MAT-001") };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.MaterialExternalNo)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddMaterialNoPatch_WhenMaterialNoNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // MaterialNo not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.MaterialExternalNo)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddMaterialNoNullPatch_WhenMaterialNoMarkedAsNull()
    {
        // Arrange
        _punchItem.MaterialExternalNo = "OLD-MAT";
        var message = CreateBaseMessage() with { MaterialNo = OptionalWithNull<string?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.MaterialExternalNo)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.MaterialExternalNo));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region WorkOrder Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddWorkOrderPatch_WhenWorkOrderChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { WorkOrderNo = new OptionalWithNull<string?>(_workOrder.No) };
        var references = CreateBaseReferences() with { WorkOrderGuid = _workOrder.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.WorkOrderGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.WorkOrderGuid));
        Assert.AreEqual(_workOrder.Guid, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddWorkOrderPatch_WhenWorkOrderSame()
    {
        // Arrange
        _punchItem.SetWorkOrder(_workOrder);
        var message = CreateBaseMessage() with { WorkOrderNo = new OptionalWithNull<string?>(_workOrder.No) };
        var references = CreateBaseReferences() with { WorkOrderGuid = _workOrder.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.WorkOrderGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddWorkOrderPatch_WhenWorkOrderNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // WorkOrderNo not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.WorkOrderGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddWorkOrderNullPatch_WhenWorkOrderMarkedAsNull()
    {
        // Arrange
        _punchItem.SetWorkOrder(_workOrder);
        var message = CreateBaseMessage() with { WorkOrderNo = OptionalWithNull<string?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.WorkOrderGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.WorkOrderGuid));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region OriginalWorkOrder Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddOriginalWorkOrderPatch_WhenOriginalWorkOrderChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { OriginalWorkOrderNo = new OptionalWithNull<string?>(_originalWorkOrder.No) };
        var references = CreateBaseReferences() with { OriginalWorkOrderGuid = _originalWorkOrder.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.OriginalWorkOrderGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.OriginalWorkOrderGuid));
        Assert.AreEqual(_originalWorkOrder.Guid, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddOriginalWorkOrderPatch_WhenOriginalWorkOrderSame()
    {
        // Arrange
        _punchItem.SetOriginalWorkOrder(_originalWorkOrder);
        var message = CreateBaseMessage() with { OriginalWorkOrderNo = new OptionalWithNull<string?>(_originalWorkOrder.No) };
        var references = CreateBaseReferences() with { OriginalWorkOrderGuid = _originalWorkOrder.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.OriginalWorkOrderGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddOriginalWorkOrderPatch_WhenOriginalWorkOrderNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // OriginalWorkOrderNo not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.OriginalWorkOrderGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddOriginalWorkOrderNullPatch_WhenOriginalWorkOrderMarkedAsNull()
    {
        // Arrange
        _punchItem.SetOriginalWorkOrder(_originalWorkOrder);
        var message = CreateBaseMessage() with { OriginalWorkOrderNo = OptionalWithNull<string?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.OriginalWorkOrderGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.OriginalWorkOrderGuid));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region Document Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddDocumentPatch_WhenDocumentChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { DocumentNo = new OptionalWithNull<string?>(_document.No) };
        var references = CreateBaseReferences() with { DocumentGuid = _document.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.DocumentGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.DocumentGuid));
        Assert.AreEqual(_document.Guid, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddDocumentPatch_WhenDocumentSame()
    {
        // Arrange
        _punchItem.SetDocument(_document);
        var message = CreateBaseMessage() with { DocumentNo = new OptionalWithNull<string?>(_document.No) };
        var references = CreateBaseReferences() with { DocumentGuid = _document.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.DocumentGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddDocumentPatch_WhenDocumentNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // DocumentNo not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.DocumentGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddDocumentNullPatch_WhenDocumentMarkedAsNull()
    {
        // Arrange
        _punchItem.SetDocument(_document);
        var message = CreateBaseMessage() with { DocumentNo = OptionalWithNull<string?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.DocumentGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.DocumentGuid));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region SWCR Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddSwcrPatch_WhenSwcrChanged()
    {
        // Arrange
        var message = CreateBaseMessage() with { SwcrNo = new OptionalWithNull<int?>(_swcr.No) };
        var references = CreateBaseReferences() with { SWCRGuid = _swcr.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.SWCRGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.SWCRGuid));
        Assert.AreEqual(_swcr.Guid, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddSwcrPatch_WhenSwcrSame()
    {
        // Arrange
        _punchItem.SetSWCR(_swcr);
        var message = CreateBaseMessage() with { SwcrNo = new OptionalWithNull<int?>(_swcr.No) };
        var references = CreateBaseReferences() with { SWCRGuid = _swcr.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.SWCRGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddSwcrPatch_WhenSwcrNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // SwcrNo not set (HasValue = false)
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.SWCRGuid)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddSwcrNullPatch_WhenSwcrMarkedAsNull()
    {
        // Arrange
        _punchItem.SetSWCR(_swcr);
        var message = CreateBaseMessage() with { SwcrNo = OptionalWithNull<int?>.CreateNull() };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.SWCRGuid)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.SWCRGuid));
        Assert.IsNull(operation?.value);
    }

    #endregion

    #region MaterialRequired Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddMaterialRequiredPatch_WhenMaterialRequiredChanged()
    {
        // Arrange
        _punchItem.MaterialRequired = false;
        var message = CreateBaseMessage() with { MaterialRequired = new Optional<bool?>(true) };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.MaterialRequired)));
        var operation = GetOperation(result, nameof(PatchablePunchItem.MaterialRequired));
        Assert.AreEqual(true, operation?.value);
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddMaterialRequiredPatch_WhenMaterialRequiredSame()
    {
        // Arrange
        _punchItem.MaterialRequired = true;
        var message = CreateBaseMessage() with { MaterialRequired = new Optional<bool?>(true) };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.MaterialRequired)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldNotAddMaterialRequiredPatch_WhenMaterialRequiredNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();  // MaterialRequired not set
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.IsFalse(HasOperation(result, nameof(PatchablePunchItem.MaterialRequired)));
    }

    #endregion

    #region Multiple Changes Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldAddMultiplePatches_WhenMultipleFieldsChanged()
    {
        // Arrange
        var newDueDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var message = CreateBaseMessage() with
        {
            Description = new Optional<string?>("New Description"),
            DueDate = new OptionalWithNull<DateTime?>(newDueDate),
            Estimate = new OptionalWithNull<int?>(200),
            Priority = new OptionalWithNull<string?>(_priority.Code),
            MaterialRequired = new Optional<bool?>(true)
        };
        var references = CreateBaseReferences() with { PriorityGuid = _priority.Guid };

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.AreEqual(5, result.Operations.Count);
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.Description)));
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.DueTimeUtc)));
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.Estimate)));
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.PriorityGuid)));
        Assert.IsTrue(HasOperation(result, nameof(PatchablePunchItem.MaterialRequired)));
    }

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldHandleMixedNullAndValueChanges()
    {
        // Arrange
        _punchItem.DueTimeUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _punchItem.Estimate = 50;
        _punchItem.SetPriority(_priority);

        var message = CreateBaseMessage() with
        {
            Description = new Optional<string?>("New Description"),
            DueDate = OptionalWithNull<DateTime?>.CreateNull(),  // Clear
            Estimate = new OptionalWithNull<int?>(100),  // Update
            Priority = OptionalWithNull<string?>.CreateNull()  // Clear
        };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        Assert.AreEqual(4, result.Operations.Count);
        
        var descOperation = GetOperation(result, nameof(PatchablePunchItem.Description));
        Assert.AreEqual("New Description", descOperation?.value);
        
        var dueOperation = GetOperation(result, nameof(PatchablePunchItem.DueTimeUtc));
        Assert.IsNull(dueOperation?.value);
        
        var estimateOperation = GetOperation(result, nameof(PatchablePunchItem.Estimate));
        Assert.AreEqual(100, estimateOperation?.value);
        
        var priorityOperation = GetOperation(result, nameof(PatchablePunchItem.PriorityGuid));
        Assert.IsNull(priorityOperation?.value);
    }

    #endregion

    #region Replace Operation Type Tests

    [TestMethod]
    public void CreateJsonPatchDocument_ShouldUseReplaceOperation()
    {
        // Arrange
        var message = CreateBaseMessage() with { Description = new Optional<string?>("New Description") };
        var references = CreateBaseReferences();

        // Act
        var result = ImportUpdateHelper.CreateJsonPatchDocument(message, _punchItem, references);

        // Assert
        var operation = result.Operations.First();
        Assert.AreEqual(OperationType.Replace, operation.OperationType);
    }

    #endregion
}
