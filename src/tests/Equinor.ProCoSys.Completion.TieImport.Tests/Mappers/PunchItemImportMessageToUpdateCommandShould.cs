using Equinor.ProCoSys.Completion.TieImport.Mappers;
using Equinor.ProCoSys.Completion.TieImport.Models;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.TieImport.Tests.Mappers;

[TestClass]
public sealed class PunchItemImportMessageToUpdateCommandShould
{
    private PlantScopedImportDataContext _scopedContext;
    private PunchItemImportMessageToUpdateCommand _mapper;

    private readonly PunchItemImportMessage _baseMessage = new(
        Guid.NewGuid(), "TestPlant", "Method", "ProjectName", "TagNo", "ExternalPunchItemNo", "FormType",
        "EQ", new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
        new Optional<string?>("BV"),
        Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(),
        new Optional<string?>(), new Optional<string?>("EQ"),
        new Optional<DateTime?>(), new Optional<string?>(), new Optional<DateTime?>(), new Optional<string?>(),
        new Optional<string?>(),
        new Optional<DateTime?>(), new Optional<string?>()
    );

    private readonly Project _project = new("TestPlant", Guid.NewGuid(), "ProjectName", "ProjectNameTestPlant");
    private readonly TagCheckList _tagCheckList = new(1, 1, "TagNo", "FormType", Guid.NewGuid(), "TestPlant", "EQ");

    private readonly LibraryItem _libraryItemRaisedBy =
        new("TestPlant", Guid.NewGuid(), "BV", "BV", LibraryType.COMPLETION_ORGANIZATION);

    private readonly LibraryItem _libraryItemClearing =
        new("TestPlant", Guid.NewGuid(), "EQ", "EQ", LibraryType.COMPLETION_ORGANIZATION);

    [TestInitialize]
    public void Setup()
    {
        _scopedContext = new PlantScopedImportDataContext("TestPlant");
        _mapper = new PunchItemImportMessageToUpdateCommand(_scopedContext);
    }

    [TestMethod]
    public void MapMessage()
    {
        // Arrange
        var message = _baseMessage with
        {
            Description = new Optional<string?>("Hello World"),
            ClearedBy = new Optional<string?>("SKS@equinor.com"),
            ClearedDate = new Optional<DateTime?>(DateTime.UtcNow),
            Method = "UPDATE"
        };

        _scopedContext.AddProjects([_project]);
        _scopedContext.AddCheckLists([_tagCheckList]);
        _scopedContext.AddLibraryItems([_libraryItemClearing, _libraryItemRaisedBy]);
        _scopedContext.AddPersons([new Person(Guid.NewGuid(), "Sindre", "Smistad", "SKS@equinor.com", "SKS@equinor.com", false)]);
        _scopedContext.AddPunchItems([
            new PunchItem("TestPlant", _project, _tagCheckList.ProCoSysGuid, Category.PA, "Punch Item 1",
                _libraryItemRaisedBy, _libraryItemClearing, Guid.NewGuid())
            {
                ExternalItemNo = "ExternalPunchItemNo",
            }
        ]);


        // Act
        var importResults = _mapper.SetCommandToImportResult(new ImportResult(default!, message, default!, []));

        // Assert
        Assert.AreEqual(0, importResults.Errors.Length);
        Assert.IsNotNull(importResults.Command);
    }
}
