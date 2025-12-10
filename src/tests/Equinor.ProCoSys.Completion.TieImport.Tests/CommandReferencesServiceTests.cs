using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.TieImport.References;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]
public class CommandReferencesServiceTests
{
    private const string TestPlant = "TestPlant";
    private const string ProjectName = "ProjectName";
    
    private ICommandReferencesService _dut = null!;
    private ICommandReferencesServiceFactory _factory = null!;
    private ImportDataBundle _bundle = null!;
    private Project _project = null!;

    private IWorkOrderRepository _workOrderRepository = null!;
    private IDocumentRepository _documentRepository = null!;
    private ISWCRRepository _swcrRepository = null!;
    private IPersonRepository _personRepository = null!;

    // Test entities
    private LibraryItem _raisedByOrg = null!;
    private LibraryItem _clearedByOrg = null!;
    private LibraryItem _punchListType = null!;
    private LibraryItem _priority = null!;
    private LibraryItem _sorting = null!;
    private WorkOrder _workOrder = null!;
    private WorkOrder _originalWorkOrder = null!;
    private Document _document = null!;
    private SWCR _swcr = null!;
    private Person _actionByPerson = null!;
    private Person _clearedByPerson = null!;
    private Person _verifiedByPerson = null!;
    private Person _rejectedByPerson = null!;
    private Guid _checkListGuid;

    [TestInitialize]
    public void Setup()
    {
        _checkListGuid = Guid.NewGuid();
        _project = new Project(TestPlant, Guid.NewGuid(), ProjectName, "Description");

        // Create library items
        _raisedByOrg = new LibraryItem(TestPlant, Guid.NewGuid(), "RAISED_ORG", "Raised By Org", LibraryType.COMPLETION_ORGANIZATION);
        _clearedByOrg = new LibraryItem(TestPlant, Guid.NewGuid(), "CLEARED_ORG", "Cleared By Org", LibraryType.COMPLETION_ORGANIZATION);
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

        // Create persons
        _actionByPerson = new Person(Guid.NewGuid(), "Action", "Person", "action@example.com", "action@example.com", false);
        _clearedByPerson = new Person(Guid.NewGuid(), "Cleared", "Person", "cleared@example.com", "cleared@example.com", false);
        _verifiedByPerson = new Person(Guid.NewGuid(), "Verified", "Person", "verified@example.com", "verified@example.com", false);
        _rejectedByPerson = new Person(Guid.NewGuid(), "Rejected", "Person", "rejected@example.com", "rejected@example.com", false);

        // Setup bundle
        _bundle = new ImportDataBundle(TestPlant, _project);
        _bundle.CheckListGuid = _checkListGuid;
        _bundle.AddLibraryItems([_raisedByOrg, _clearedByOrg, _punchListType, _priority, _sorting]);

        // Setup repositories
        _workOrderRepository = Substitute.For<IWorkOrderRepository>();
        _documentRepository = Substitute.For<IDocumentRepository>();
        _swcrRepository = Substitute.For<ISWCRRepository>();
        _personRepository = Substitute.For<IPersonRepository>();

        _factory = new CommandReferencesServiceFactory(
            _workOrderRepository,
            _documentRepository,
            _swcrRepository,
            _personRepository);

        _dut = _factory.Create(_bundle);
    }

    #region Helper Methods

    private PunchItemImportMessage CreateBaseMessage() => new(
        Guid.NewGuid(),
        "CREATE",
        ProjectName,
        TestPlant,
        "TagNo",
        "ExternalPunchItemNo",
        "FormType",
        "EQ",
        new Optional<long?>(),
        new Optional<string?>("Test Description"),
        new Optional<string?>(_raisedByOrg.Code),
        Category.PA,
        new OptionalWithNull<string?>(),
        new OptionalWithNull<DateTime?>(),
        new Optional<DateTime?>(),
        new Optional<string?>(),
        new Optional<string?>(_clearedByOrg.Code),
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

    #endregion

    #region Basic Validation Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnProjectGuid()
    {
        // Arrange
        var message = CreateBaseMessage();

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_project.Guid, references.ProjectGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnCheckListGuid()
    {
        // Arrange
        var message = CreateBaseMessage();

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_checkListGuid, references.CheckListGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenCheckListGuidIsMissing()
    {
        // Arrange
        _bundle.CheckListGuid = null;
        var message = CreateBaseMessage();

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("CheckList")));
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnNoErrors_WhenAllRequiredFieldsAreValid()
    {
        // Arrange
        var message = CreateBaseMessage();

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, references.Errors.Length);
    }

    #endregion

    #region RaisedByOrg Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnRaisedByOrgGuid()
    {
        // Arrange
        var message = CreateBaseMessage();

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_raisedByOrg.Guid, references.RaisedByOrgGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenRaisedByOrgNotFound()
    {
        // Arrange
        var message = CreateBaseMessage() with { RaisedByOrganization = new Optional<string?>("INVALID_ORG") };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("RaisedByOrganization")));
    }

    #endregion

    #region ClearedByOrg Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnClearedByOrgGuid()
    {
        // Arrange
        var message = CreateBaseMessage();

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_clearedByOrg.Guid, references.ClearedByOrgGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenClearedByOrgNotFound()
    {
        // Arrange
        var message = CreateBaseMessage() with { ClearedByOrganization = new Optional<string?>("INVALID_ORG") };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("ClearedByOrganization")));
    }

    #endregion

    #region PunchListType Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnTypeGuid()
    {
        // Arrange
        var message = CreateBaseMessage() with { PunchListType = new OptionalWithNull<string?>(_punchListType.Code) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_punchListType.Guid, references.TypeGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnNullTypeGuid_WhenNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsNull(references.TypeGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnNullTypeGuid_WhenMarkedAsNull()
    {
        // Arrange
        var message = CreateBaseMessage() with { PunchListType = OptionalWithNull<string?>.CreateNull() };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsNull(references.TypeGuid);
        Assert.AreEqual(0, references.Errors.Length);
    }

    #endregion

    #region Priority Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnPriorityGuid()
    {
        // Arrange
        var message = CreateBaseMessage() with { Priority = new OptionalWithNull<string?>(_priority.Code) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_priority.Guid, references.PriorityGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnNullPriorityGuid_WhenNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsNull(references.PriorityGuid);
    }

    #endregion

    #region Sorting Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnSortingGuid()
    {
        // Arrange
        var message = CreateBaseMessage() with { Sorting = new OptionalWithNull<string?>(_sorting.Code) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_sorting.Guid, references.SortingGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnNullSortingGuid_WhenNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsNull(references.SortingGuid);
    }

    #endregion

    #region WorkOrder Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnWorkOrderGuid()
    {
        // Arrange
        _workOrderRepository.GetByNoAsync(_workOrder.No, Arg.Any<CancellationToken>()).Returns(_workOrder);
        var message = CreateBaseMessage() with { WorkOrderNo = new OptionalWithNull<string?>(_workOrder.No) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_workOrder.Guid, references.WorkOrderGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenWorkOrderNotFound()
    {
        // Arrange
        _workOrderRepository.GetByNoAsync("INVALID", Arg.Any<CancellationToken>()).Returns((WorkOrder?)null);
        var message = CreateBaseMessage() with { WorkOrderNo = new OptionalWithNull<string?>("INVALID") };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("WorkOrder") && e.Message.Contains("not found")));
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenWorkOrderIsVoided()
    {
        // Arrange
        var voidedWorkOrder = new WorkOrder(TestPlant, Guid.NewGuid(), "WO-VOID") { IsVoided = true };
        _workOrderRepository.GetByNoAsync(voidedWorkOrder.No, Arg.Any<CancellationToken>()).Returns(voidedWorkOrder);
        var message = CreateBaseMessage() with { WorkOrderNo = new OptionalWithNull<string?>(voidedWorkOrder.No) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("WorkOrder") && e.Message.Contains("voided")));
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnNullWorkOrderGuid_WhenNotProvided()
    {
        // Arrange
        var message = CreateBaseMessage();

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsNull(references.WorkOrderGuid);
    }

    #endregion

    #region OriginalWorkOrder Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnOriginalWorkOrderGuid()
    {
        // Arrange
        _workOrderRepository.GetByNoAsync(_originalWorkOrder.No, Arg.Any<CancellationToken>()).Returns(_originalWorkOrder);
        var message = CreateBaseMessage() with { OriginalWorkOrderNo = new OptionalWithNull<string?>(_originalWorkOrder.No) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_originalWorkOrder.Guid, references.OriginalWorkOrderGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenOriginalWorkOrderNotFound()
    {
        // Arrange
        _workOrderRepository.GetByNoAsync("INVALID", Arg.Any<CancellationToken>()).Returns((WorkOrder?)null);
        var message = CreateBaseMessage() with { OriginalWorkOrderNo = new OptionalWithNull<string?>("INVALID") };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("OriginalWorkOrder") && e.Message.Contains("not found")));
    }

    #endregion

    #region Document Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnDocumentGuid()
    {
        // Arrange
        _documentRepository.GetByNoAsync(_document.No, Arg.Any<CancellationToken>()).Returns(_document);
        var message = CreateBaseMessage() with { DocumentNo = new OptionalWithNull<string?>(_document.No) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_document.Guid, references.DocumentGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenDocumentNotFound()
    {
        // Arrange
        _documentRepository.GetByNoAsync("INVALID", Arg.Any<CancellationToken>()).Returns((Document?)null);
        var message = CreateBaseMessage() with { DocumentNo = new OptionalWithNull<string?>("INVALID") };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("Document") && e.Message.Contains("not found")));
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenDocumentIsVoided()
    {
        // Arrange
        var voidedDocument = new Document(TestPlant, Guid.NewGuid(), "DOC-VOID") { IsVoided = true };
        _documentRepository.GetByNoAsync(voidedDocument.No, Arg.Any<CancellationToken>()).Returns(voidedDocument);
        var message = CreateBaseMessage() with { DocumentNo = new OptionalWithNull<string?>(voidedDocument.No) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("Document") && e.Message.Contains("voided")));
    }

    #endregion

    #region SWCR Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnSWCRGuid()
    {
        // Arrange
        _swcrRepository.GetByNoAsync(_swcr.No, Arg.Any<CancellationToken>()).Returns(_swcr);
        var message = CreateBaseMessage() with { SwcrNo = new OptionalWithNull<int?>(_swcr.No) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_swcr.Guid, references.SWCRGuid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenSWCRNotFound()
    {
        // Arrange
        _swcrRepository.GetByNoAsync(999, Arg.Any<CancellationToken>()).Returns((SWCR?)null);
        var message = CreateBaseMessage() with { SwcrNo = new OptionalWithNull<int?>(999) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("SWCR") && e.Message.Contains("not found")));
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenSWCRIsVoided()
    {
        // Arrange
        var voidedSwcr = new SWCR(TestPlant, Guid.NewGuid(), 999) { IsVoided = true };
        _swcrRepository.GetByNoAsync(voidedSwcr.No, Arg.Any<CancellationToken>()).Returns(voidedSwcr);
        var message = CreateBaseMessage() with { SwcrNo = new OptionalWithNull<int?>(voidedSwcr.No) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("SWCR") && e.Message.Contains("voided")));
    }

    #endregion

    #region ActionBy Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnActionByPersonOid()
    {
        // Arrange
        _personRepository.GetByUserNameAsync(_actionByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_actionByPerson);
        var message = CreateBaseMessage() with { ActionBy = new OptionalWithNull<string?>(_actionByPerson.UserName) };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.AreEqual(_actionByPerson.Guid, references.ActionByPersonOid);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenActionByPersonNotFound()
    {
        // Arrange
        _personRepository.GetByUserNameAsync("invalid@example.com", Arg.Any<CancellationToken>()).Returns((Person?)null);
        var message = CreateBaseMessage() with { ActionBy = new OptionalWithNull<string?>("invalid@example.com") };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("ActionBy") && e.Message.Contains("not found")));
    }

    #endregion

    #region ClearedBy Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnClearedByPerson()
    {
        // Arrange
        var clearedDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        _personRepository.GetByUserNameAsync(_clearedByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_clearedByPerson);
        var message = CreateBaseMessage() with
        {
            ClearedBy = new Optional<string?>(_clearedByPerson.UserName),
            ClearedDate = new Optional<DateTime?>(clearedDate)
        };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsNotNull(references.ClearedBy);
        Assert.AreEqual(_clearedByPerson.Guid, references.ClearedBy.PersonOid);
        Assert.AreEqual(clearedDate, references.ClearedBy.ActionDate);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenClearedByWithoutClearedDate()
    {
        // Arrange
        _personRepository.GetByUserNameAsync(_clearedByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_clearedByPerson);
        var message = CreateBaseMessage() with
        {
            ClearedBy = new Optional<string?>(_clearedByPerson.UserName),
            ClearedDate = new Optional<DateTime?>()  // No date
        };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("ClearedDate") && e.Message.Contains("required")));
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenClearedByPersonNotFound()
    {
        // Arrange
        var clearedDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        _personRepository.GetByUserNameAsync("invalid@example.com", Arg.Any<CancellationToken>()).Returns((Person?)null);
        var message = CreateBaseMessage() with
        {
            ClearedBy = new Optional<string?>("invalid@example.com"),
            ClearedDate = new Optional<DateTime?>(clearedDate)
        };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("ClearedBy") && e.Message.Contains("not found")));
    }

    #endregion

    #region VerifiedBy Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnVerifiedByPerson()
    {
        // Arrange
        var verifiedDate = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc);
        _personRepository.GetByUserNameAsync(_verifiedByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_verifiedByPerson);
        var message = CreateBaseMessage() with
        {
            VerifiedBy = new Optional<string?>(_verifiedByPerson.UserName),
            VerifiedDate = new Optional<DateTime?>(verifiedDate)
        };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsNotNull(references.VerifiedBy);
        Assert.AreEqual(_verifiedByPerson.Guid, references.VerifiedBy.PersonOid);
        Assert.AreEqual(verifiedDate, references.VerifiedBy.ActionDate);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenVerifiedByWithoutVerifiedDate()
    {
        // Arrange
        _personRepository.GetByUserNameAsync(_verifiedByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_verifiedByPerson);
        var message = CreateBaseMessage() with
        {
            VerifiedBy = new Optional<string?>(_verifiedByPerson.UserName),
            VerifiedDate = new Optional<DateTime?>()  // No date
        };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("VerifiedDate") && e.Message.Contains("required")));
    }

    #endregion

    #region RejectedBy Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnRejectedByPerson()
    {
        // Arrange
        var rejectedDate = new DateTime(2024, 3, 25, 0, 0, 0, DateTimeKind.Utc);
        _personRepository.GetByUserNameAsync(_rejectedByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_rejectedByPerson);
        var message = CreateBaseMessage() with
        {
            RejectedBy = new Optional<string?>(_rejectedByPerson.UserName),
            RejectedDate = new Optional<DateTime?>(rejectedDate)
        };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsNotNull(references.RejectedBy);
        Assert.AreEqual(_rejectedByPerson.Guid, references.RejectedBy.PersonOid);
        Assert.AreEqual(rejectedDate, references.RejectedBy.ActionDate);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldReturnError_WhenRejectedByWithoutRejectedDate()
    {
        // Arrange
        _personRepository.GetByUserNameAsync(_rejectedByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_rejectedByPerson);
        var message = CreateBaseMessage() with
        {
            RejectedBy = new Optional<string?>(_rejectedByPerson.UserName),
            RejectedDate = new Optional<DateTime?>()  // No date
        };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("RejectedDate") && e.Message.Contains("required")));
    }

    #endregion

    #region Multiple Errors Tests

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldCollectAllErrors()
    {
        // Arrange
        _bundle.CheckListGuid = null;
        var message = CreateBaseMessage() with
        {
            RaisedByOrganization = new Optional<string?>("INVALID_RAISED"),
            ClearedByOrganization = new Optional<string?>("INVALID_CLEARED")
        };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsTrue(references.Errors.Length >= 3);
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("CheckList")));
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("RaisedByOrganization")));
        Assert.IsTrue(references.Errors.Any(e => e.Message.Contains("ClearedByOrganization")));
    }

    #endregion

    #region Complete Reference Resolution Test

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldResolveAllReferences()
    {
        // Arrange
        var clearedDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var verifiedDate = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc);
        var rejectedDate = new DateTime(2024, 3, 25, 0, 0, 0, DateTimeKind.Utc);

        _workOrderRepository.GetByNoAsync(_workOrder.No, Arg.Any<CancellationToken>()).Returns(_workOrder);
        _workOrderRepository.GetByNoAsync(_originalWorkOrder.No, Arg.Any<CancellationToken>()).Returns(_originalWorkOrder);
        _documentRepository.GetByNoAsync(_document.No, Arg.Any<CancellationToken>()).Returns(_document);
        _swcrRepository.GetByNoAsync(_swcr.No, Arg.Any<CancellationToken>()).Returns(_swcr);
        _personRepository.GetByUserNameAsync(_actionByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_actionByPerson);
        _personRepository.GetByUserNameAsync(_clearedByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_clearedByPerson);
        _personRepository.GetByUserNameAsync(_verifiedByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_verifiedByPerson);
        _personRepository.GetByUserNameAsync(_rejectedByPerson.UserName, Arg.Any<CancellationToken>()).Returns(_rejectedByPerson);

        var message = CreateBaseMessage() with
        {
            PunchListType = new OptionalWithNull<string?>(_punchListType.Code),
            Priority = new OptionalWithNull<string?>(_priority.Code),
            Sorting = new OptionalWithNull<string?>(_sorting.Code),
            WorkOrderNo = new OptionalWithNull<string?>(_workOrder.No),
            OriginalWorkOrderNo = new OptionalWithNull<string?>(_originalWorkOrder.No),
            DocumentNo = new OptionalWithNull<string?>(_document.No),
            SwcrNo = new OptionalWithNull<int?>(_swcr.No),
            ActionBy = new OptionalWithNull<string?>(_actionByPerson.UserName),
            ClearedBy = new Optional<string?>(_clearedByPerson.UserName),
            ClearedDate = new Optional<DateTime?>(clearedDate),
            VerifiedBy = new Optional<string?>(_verifiedByPerson.UserName),
            VerifiedDate = new Optional<DateTime?>(verifiedDate),
            RejectedBy = new Optional<string?>(_rejectedByPerson.UserName),
            RejectedDate = new Optional<DateTime?>(rejectedDate)
        };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert - No errors
        Assert.AreEqual(0, references.Errors.Length, $"Errors: {string.Join(", ", references.Errors.Select(e => e.Message))}");
        
        // Assert - Project and CheckList
        Assert.AreEqual(_project.Guid, references.ProjectGuid);
        Assert.AreEqual(_checkListGuid, references.CheckListGuid);
        
        // Assert - Organizations
        Assert.AreEqual(_raisedByOrg.Guid, references.RaisedByOrgGuid);
        Assert.AreEqual(_clearedByOrg.Guid, references.ClearedByOrgGuid);
        
        // Assert - Library items
        Assert.AreEqual(_punchListType.Guid, references.TypeGuid);
        Assert.AreEqual(_priority.Guid, references.PriorityGuid);
        Assert.AreEqual(_sorting.Guid, references.SortingGuid);
        
        // Assert - Related entities
        Assert.AreEqual(_workOrder.Guid, references.WorkOrderGuid);
        Assert.AreEqual(_originalWorkOrder.Guid, references.OriginalWorkOrderGuid);
        Assert.AreEqual(_document.Guid, references.DocumentGuid);
        Assert.AreEqual(_swcr.Guid, references.SWCRGuid);
        
        // Assert - ActionBy person
        Assert.AreEqual(_actionByPerson.Guid, references.ActionByPersonOid);
        
        // Assert - ClearedBy with date
        Assert.IsNotNull(references.ClearedBy);
        Assert.AreEqual(_clearedByPerson.Guid, references.ClearedBy.PersonOid);
        Assert.AreEqual(clearedDate, references.ClearedBy.ActionDate);
        
        // Assert - VerifiedBy with date
        Assert.IsNotNull(references.VerifiedBy);
        Assert.AreEqual(_verifiedByPerson.Guid, references.VerifiedBy.PersonOid);
        Assert.AreEqual(verifiedDate, references.VerifiedBy.ActionDate);

        // Assert - RejectedBy with date
        Assert.IsNotNull(references.RejectedBy);
        Assert.AreEqual(_rejectedByPerson.Guid, references.RejectedBy.PersonOid);
        Assert.AreEqual(rejectedDate, references.RejectedBy.ActionDate);
    }

    #endregion
}
