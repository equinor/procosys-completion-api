using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]
public class CommandReferencesServiceShould
{
    private CommandReferencesService _service;
    private PlantScopedImportDataContext _context;
    private readonly Project _project = new("TestPlant", Guid.NewGuid(), "ProjectName", "ProjectName");

    private readonly LibraryItem _raisedByOrg =
        new("TestPlant", Guid.NewGuid(), "EQ", "EQ", LibraryType.COMPLETION_ORGANIZATION);

    private readonly PunchItemImportMessage _baseMessage = new(
        new TIObject{Guid = Guid.NewGuid(), Site = "TestPlant", Method = "Method", Project = "ProjectName"}, "TagNo", "ExternalPunchItemNo", "FormType",
        "EQ", new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
        new Optional<string?>("BV"),
        Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(),
        new Optional<string?>(), new Optional<string?>("EQ"),
        new Optional<DateTime?>(), new Optional<string?>(), new Optional<DateTime?>(), new Optional<string?>(),
        new Optional<string?>(),
        new Optional<DateTime?>(), new Optional<string?>()
    );

    private const string SksEquinorCom = "SKS@equinor.com";

    [TestInitialize]
    public void Setup()
    {
        _context = new PlantScopedImportDataContext("TestPlant");
        _service = new CommandReferencesService(_context);
    }

    private void AddProjects() =>
        _context.AddProjects(new[] { new Project("Project 1", Guid.NewGuid(), "Project 1", "Project 1"), _project, });

    private void AddCheckLists() => _context.AddCheckLists(new[]
    {
        new TagCheckList(1, 1, "Tag 1", "Form 1", Guid.NewGuid(), "TestPlant", "EQ"),
        new TagCheckList(2, 2, "TagNo", "FormType", Guid.NewGuid(), "TestPlant", "EQ"),
    });

    private void AddLibraryItems() => _context.AddLibraryItems(new[]
    {
        new LibraryItem("TestPlant", Guid.NewGuid(), "Lib1", "Lib1", LibraryType.COMPLETION_ORGANIZATION),
        new LibraryItem("TestPlant", Guid.NewGuid(), "EQ", "EQ", LibraryType.COMPLETION_ORGANIZATION),
        new LibraryItem("TestPlant", Guid.NewGuid(), "BV", "BV", LibraryType.COMPLETION_ORGANIZATION),
    });

    [TestMethod]
    public void GetCreatePunchItemReferences_ShouldPopulateReferencesCorrectly()
    {
        AddProjects();
        AddCheckLists();
        AddLibraryItems();

        // Arrange
        var message = _baseMessage;

        // Act
        var references = _service.GetCreatePunchItemReferences(message);

        // Assert
        Assert.IsNotNull(references);
        Assert.AreEqual(0, references.Errors.Length);
    }


    [TestMethod]
    public void GetUpdatePunchItemReferences_ShouldPopulateReferencesCorrectly()
    {
        AddProjects();
        AddCheckLists();
        AddLibraryItems();

        _context.AddPunchItems([
            new PunchItem("TestPlant", _project, _context.CheckLists[0].ProCoSysGuid, Category.PA, string.Empty, _raisedByOrg, _raisedByOrg,
                Guid.NewGuid())
            {
                ExternalItemNo = "WrongExternalPunchItemNo"
            },
            new PunchItem("TestPlant", _project, _context.CheckLists[1].ProCoSysGuid, Category.PA, string.Empty, _raisedByOrg, _raisedByOrg,
                Guid.NewGuid()) { ExternalItemNo = "ExternalPunchItemNo" }
        ]);

        // Arrange
        var message = _baseMessage;

        // Act
        var references = _service.GetUpdatePunchItemReferences(message);

        // Assert
        Assert.IsNotNull(references);
        Assert.AreEqual(0, references.Errors.Length);
    }

    [TestMethod]
    public void GetCreatePunchItemReferences_ShouldHandleErrors()
    {
        // Arrange
        var message = _baseMessage with
        {
            RaisedByOrganization = new Optional<string?>(),
            ClearedByOrganization = new Optional<string?>()
        };
        message.TiObject.Project = "InvalidProjectName";


        // Act
        var references = _service.GetCreatePunchItemReferences(message);

        // Assert
        Assert.IsNotNull(references);
        Assert.AreNotEqual(0, references.Errors.Length);
    }

    [TestMethod]
    public void GetCreatePunchItemReferences_ShouldMakeErrorsOnMissingDateForAction()
    {
        // Arrange
        var clearedByMessage = _baseMessage with { ClearedBy = new Optional<string?>(SksEquinorCom) };
        var verifiedByMessage = _baseMessage with { VerifiedBy = new Optional<string?>(SksEquinorCom) };
        var rejectedByMessage = _baseMessage with { RejectedBy = new Optional<string?>(SksEquinorCom) };

        // Act
        var clearByRefs = _service.GetCreatePunchItemReferences(clearedByMessage);
        var verifiedByRefs = _service.GetCreatePunchItemReferences(verifiedByMessage);
        var rejectedByRefs = _service.GetCreatePunchItemReferences(rejectedByMessage);

        // Assert
        Assert.IsNotNull(clearByRefs);
        Assert.AreNotEqual(0, clearByRefs.Errors.Length);
        Assert.IsTrue(clearByRefs.Errors.Any(e => e.Message.Contains(SksEquinorCom)));

        Assert.IsNotNull(verifiedByRefs);
        Assert.AreNotEqual(0, verifiedByRefs.Errors.Length);
        Assert.IsTrue(verifiedByRefs.Errors.Any(e => e.Message.Contains(SksEquinorCom)));

        Assert.IsNotNull(rejectedByRefs);
        Assert.AreNotEqual(0, rejectedByRefs.Errors.Length);
        Assert.IsTrue(rejectedByRefs.Errors.Any(e => e.Message.Contains(SksEquinorCom)));
    }

    [TestMethod]
    public void GetCreatePunchItemReferences_ShouldMakeErrorsOnMissingPersonForAction()
    {
        // Arrange

        var clearedByMessage = _baseMessage with { ClearedDate = new Optional<DateTime?>(DateTime.Now) };
        var verifiedByMessage = _baseMessage with { VerifiedDate = new Optional<DateTime?>(DateTime.Now) };
        var rejectedByMessage = _baseMessage with { RejectedDate = new Optional<DateTime?>(DateTime.Now) };

        // Act
        var clearByRefs = _service.GetCreatePunchItemReferences(clearedByMessage);
        var verifiedByRefs = _service.GetCreatePunchItemReferences(verifiedByMessage);
        var rejectedByRefs = _service.GetCreatePunchItemReferences(rejectedByMessage);

        // Assert
        Assert.IsNotNull(clearByRefs);
        Assert.AreNotEqual(0, clearByRefs.Errors.Length);
        var errorMessage = "Person is required for action (Clear, Verify, Reject) on date '";
        Assert.IsTrue(clearByRefs.Errors.Any(e => e.Message.Contains(errorMessage)));

        Assert.IsNotNull(verifiedByRefs);
        Assert.AreNotEqual(0, verifiedByRefs.Errors.Length);
        Assert.IsTrue(verifiedByRefs.Errors.Any(e => e.Message.Contains(errorMessage)));

        Assert.IsNotNull(rejectedByRefs);
        Assert.AreNotEqual(0, rejectedByRefs.Errors.Length);
        Assert.IsTrue(rejectedByRefs.Errors.Any(e => e.Message.Contains(errorMessage)));
    }

    [TestMethod]
    public void GetCreatePunchItemReferences_ShouldMakeErrorsOnPersonNotFound()
    {
        // Arrange

        var clearedByMessage = _baseMessage with
        {
            ClearedDate = new Optional<DateTime?>(DateTime.Now), ClearedBy = new Optional<string?>(SksEquinorCom)
        };
        var verifiedByMessage = _baseMessage with
        {
            VerifiedDate = new Optional<DateTime?>(DateTime.Now), VerifiedBy = new Optional<string?>(SksEquinorCom)
        };
        var rejectedByMessage = _baseMessage with
        {
            RejectedDate = new Optional<DateTime?>(DateTime.Now), RejectedBy = new Optional<string?>(SksEquinorCom)
        };

        // Act
        var clearByRefs = _service.GetCreatePunchItemReferences(clearedByMessage);
        var verifiedByRefs = _service.GetCreatePunchItemReferences(verifiedByMessage);
        var rejectedByRefs = _service.GetCreatePunchItemReferences(rejectedByMessage);

        // Assert
        Assert.IsNotNull(clearByRefs);
        Assert.AreNotEqual(0, clearByRefs.Errors.Length);
        Assert.IsTrue(clearByRefs.Errors.Any(e => e.Message.Contains(SksEquinorCom)));

        Assert.IsNotNull(verifiedByRefs);
        Assert.AreNotEqual(0, verifiedByRefs.Errors.Length);
        Assert.IsTrue(verifiedByRefs.Errors.Any(e => e.Message.Contains(SksEquinorCom)));

        Assert.IsNotNull(rejectedByRefs);
        Assert.AreNotEqual(0, rejectedByRefs.Errors.Length);
        Assert.IsTrue(rejectedByRefs.Errors.Any(e => e.Message.Contains(SksEquinorCom)));
    }

    [TestMethod]
    public void GetCreatePunchItemReferences_ShouldFindPerson()
    {
        // Arrange
        AddProjects();
        AddCheckLists();
        AddLibraryItems();
        _context.AddPersons(new[]
        {
            new Person(Guid.NewGuid(), "Person 1", "Person 1", "Person 1", "Person 1", false),
            new Person(Guid.NewGuid(), SksEquinorCom, SksEquinorCom, SksEquinorCom, SksEquinorCom, false)
        });
        var clearedByMessage = _baseMessage with
        {
            ClearedDate = new Optional<DateTime?>(DateTime.Now), ClearedBy = new Optional<string?>(SksEquinorCom)
        };
        var verifiedByMessage = _baseMessage with
        {
            VerifiedDate = new Optional<DateTime?>(DateTime.Now), VerifiedBy = new Optional<string?>(SksEquinorCom)
        };
        var rejectedByMessage = _baseMessage with
        {
            RejectedDate = new Optional<DateTime?>(DateTime.Now), RejectedBy = new Optional<string?>(SksEquinorCom)
        };

        // Act
        var clearByRefs = _service.GetCreatePunchItemReferences(clearedByMessage);
        var verifiedByRefs = _service.GetCreatePunchItemReferences(verifiedByMessage);
        var rejectedByRefs = _service.GetCreatePunchItemReferences(rejectedByMessage);

        // Assert
        Assert.IsNotNull(clearByRefs);
        Assert.AreEqual(0, clearByRefs.Errors.Length);

        Assert.IsNotNull(verifiedByRefs);
        Assert.AreEqual(0, verifiedByRefs.Errors.Length);

        Assert.IsNotNull(rejectedByRefs);
        Assert.AreEqual(0, rejectedByRefs.Errors.Length);
    }

    [TestMethod]
    public void GetUpdatePunchItemReferences_ShouldHandleErrors()
    {
        // Arrange
        var message = new PunchItemImportMessage(
            new TIObject{Guid = Guid.NewGuid(), Site = "Plant", Method = "Method", Project = "InvalidProjectName"},  "TagNo", "InvalidExternalPunchItemNo", "FormType",
            "EQ", new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<string?>(),
            Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(),
            new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>(), new Optional<DateTime?>(), new Optional<string?>(),
            new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>()
        );

        // Act
        var references = _service.GetUpdatePunchItemReferences(message);

        // Assert
        Assert.IsNotNull(references);
        Assert.AreNotEqual(0, references.Errors.Length);
    }
}
