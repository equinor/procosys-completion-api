using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]
public class CommandReferencesServiceTests
{
    private CommandReferencesService _dut = null!;
    private ImportDataBundle _bundle = null!;

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
    
    [TestInitialize]
    public void Setup()
    {
        _bundle = new ImportDataBundle("TestPlant");
        _dut = new CommandReferencesService(_bundle);
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
        var references = _dut.GetAndValidatePunchItemReferencesForImport(message);

        // Assert
        Assert.IsNotNull(references);
        Assert.AreNotEqual(0, references.Errors.Length);
    }
}
