using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.TieImport.Models;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.TieImport.Tests;

[TestClass]
public class CommandReferencesServiceTests
{
    private CommandReferencesService _dut = null!;
    private ImportDataBundle _bundle = null!;

    private IWorkOrderRepository _workOrderRepository = null!;
    private IDocumentRepository _documentRepository = null!;
    private ISWCRRepository _swcrRepository = null!;
    private IPersonRepository _personRepository = null!;

    private readonly PunchItemImportMessage _baseMessage = new(
        Guid.NewGuid(),
        "CREATE",
        "ProjectName",
        "TestPlant",
        "TagNo",
        new Optional<string?>("ExternalPunchItemNo"),
        "FormType",
        "EQ",
        new Optional<long?>(),
        new Optional<string?>("Test Description"),
        new Optional<string?>("BV"),
        Category.PA,
        new OptionalWithNull<string?>(),
        new OptionalWithNull<DateTime?>(),
        new Optional<DateTime?>(),
        new Optional<string?>(),
        new Optional<string?>("EQ"),
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

    [TestInitialize]
    public void Setup()
    {
        _bundle = new ImportDataBundle("TestPlant", new Project("TestPlant", Guid.NewGuid(), "ProjectName", "stuff"));
        _workOrderRepository = Substitute.For<IWorkOrderRepository>();
        _documentRepository = Substitute.For<IDocumentRepository>();
        _swcrRepository = Substitute.For<ISWCRRepository>();
        _personRepository = Substitute.For<IPersonRepository>();

        _dut = new CommandReferencesService(
            _bundle,
            _workOrderRepository,
            _documentRepository,
            _swcrRepository,
            _personRepository);
    }

    [TestMethod]
    public async Task GetAndValidatePunchItemReferencesForImport_ShouldHandleErrors()
    {
        // Arrange
        var message = _baseMessage with
        {
            RaisedByOrganization = new Optional<string?>(),
            ClearedByOrganization = new Optional<string?>(),
            ProjectName = "InvalidProjectName"
        };

        // Act
        var references = await _dut.GetAndValidatePunchItemReferencesForImportAsync(message, CancellationToken.None);

        // Assert
        Assert.IsNotNull(references);
        Assert.AreNotEqual(0, references.Errors.Length);
    }
}
