using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]
public class CommandReferencesServiceShould
{
    private CommandReferencesService _service = null!;
    private ImportDataBundle _bundle = null!;
    private readonly Project _project = new("TestPlant", Guid.NewGuid(), "ProjectName", "ProjectName");

    private readonly LibraryItem _raisedByOrg =
        new("TestPlant", Guid.NewGuid(), "EQ", "EQ", LibraryType.COMPLETION_ORGANIZATION);

    private readonly PunchItemImportMessage _baseMessage = new(
        Guid.NewGuid(), "TestPlant",  "ProjectName", "TagNo", "ExternalPunchItemNo", "FormType",
        "EQ", new Optional<string?>(), new Optional<string?>(), 
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
        _bundle = new ImportDataBundle("TestPlant");
        _service = new CommandReferencesService(_bundle);
    }
    
    [TestMethod]
    public void GetAndValidatePunchItemReferencesForImport_ShouldHandleErrors()
    {
        // Arrange
        var message = _baseMessage with
        {
            RaisedByOrganization = new Optional<string?>(),
            ClearedByOrganization = new Optional<string?>(),
            ProjectName = "InvalidProjectName"
        };

        // Act
        var references = _service.GetAndValidatePunchItemReferencesForImport(message);

        // Assert
        Assert.IsNotNull(references);
        Assert.AreNotEqual(0, references.Errors.Length);
    }
}
