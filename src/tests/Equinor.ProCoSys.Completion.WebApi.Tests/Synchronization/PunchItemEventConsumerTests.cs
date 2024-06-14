using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class PunchItemEventConsumerTests
{
    private readonly IPunchItemRepository _punchItemRepoMock = Substitute.For<IPunchItemRepository>();
    private readonly IPersonRepository _personRepoMock = Substitute.For<IPersonRepository>();
    private readonly IProjectRepository _projectRepoMock = Substitute.For<IProjectRepository>();
    private readonly ILibraryItemRepository _libraryItemRepoMock = Substitute.For<ILibraryItemRepository>();
    private readonly IDocumentRepository _documentRepoMock = Substitute.For<IDocumentRepository>();
    private readonly ISWCRRepository _swcrRepoMock = Substitute.For<ISWCRRepository>();
    private readonly IWorkOrderRepository _workOrderRepoMock = Substitute.For<IWorkOrderRepository>();

    private readonly IPlantSetter _plantSetter = Substitute.For<IPlantSetter>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly PunchItemEventConsumer _dut;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
    private readonly ConsumeContext<PunchItemEvent> _contextMock = Substitute.For<ConsumeContext<PunchItemEvent>>();
    private PunchItem? _punchItemAddedToRepository;

    private const string Plant = "PCS$OSEBERG_C";
    private static readonly Guid s_projectGuid = Guid.NewGuid();
    private readonly Project _project = new(Plant, s_projectGuid, "ProjectTitan", "Description");
    private static readonly Guid s_raisedByOrgGuid = Guid.NewGuid();
    private static readonly Guid s_clearingByOrgGuid = Guid.NewGuid();

    private readonly LibraryItem _raisedByOrg = new(Plant, s_raisedByOrgGuid, "COM", "COMMISSIONING",
        LibraryType.COMPLETION_ORGANIZATION);
    private readonly LibraryItem _clearingByOrg = new(Plant, s_clearingByOrgGuid, "COM", "COMMISSIONING",
        LibraryType.COMPLETION_ORGANIZATION);

    private readonly Guid _punchListPriorityGuid = Guid.NewGuid();
    private readonly Guid _punchListSortingGuid = Guid.NewGuid();
    private readonly Guid _punchListTypeGuid = Guid.NewGuid();

    private readonly Guid _woGuid = Guid.NewGuid();
    private readonly Guid _originalWoGuid = Guid.NewGuid();

    private readonly Guid _swcrGuid = Guid.NewGuid();

    private readonly Guid _documentGuid = Guid.NewGuid();

    public PunchItemEventConsumerTests() =>
        _dut = new PunchItemEventConsumer(
            Substitute.For<ILogger<PunchItemEventConsumer>>(),
            _plantSetter,
            _personRepoMock,
            _punchItemRepoMock,
            _projectRepoMock,
            _libraryItemRepoMock,
            _documentRepoMock,
            _swcrRepoMock,
            _workOrderRepoMock,
            _unitOfWorkMock
            );

    [TestInitialize]
    public void Setup()
    {
        _applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { ObjectId = new Guid() });

        _punchItemRepoMock
            .When(x => x.Add(Arg.Any<PunchItem>()))
            .Do(callInfo =>
            {
                _punchItemAddedToRepository = callInfo.Arg<PunchItem>();
            });
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewPunchItem_WhenPunchItemDoesNotExist()
    {
        //Arrange
        var guid = Guid.NewGuid();
        var actionByGuid = Guid.NewGuid();
        var createdByGuid = Guid.NewGuid();

        var bEvent = GetTestEvent(guid, Plant, s_projectGuid,
            "description",
            Guid.NewGuid(),
            Category.PA,
            s_raisedByOrgGuid,
            s_clearingByOrgGuid,
            _punchListSortingGuid,
            _punchListTypeGuid,
            _punchListPriorityGuid,
            "55",
            _originalWoGuid,
            _woGuid,
            _swcrGuid,
            _documentGuid,
            false,
            false,
            "NO123",
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            createdByGuid,
            actionByGuid
            );
        _contextMock.Message.Returns(bEvent);

        _punchItemRepoMock.ExistsAsync(guid, default).Returns(false);
        _projectRepoMock.GetAsync(s_projectGuid, default).Returns(_project);
        _libraryItemRepoMock.GetAsync(s_raisedByOrgGuid, Arg.Any<CancellationToken>()).Returns(_raisedByOrg);
        _libraryItemRepoMock.GetAsync(s_clearingByOrgGuid, Arg.Any<CancellationToken>()).Returns(_clearingByOrg);

        _libraryItemRepoMock.GetByGuidAndTypeAsync(_punchListPriorityGuid, LibraryType.PUNCHLIST_PRIORITY, Arg.Any<CancellationToken>())
            .Returns(new LibraryItem(Plant, _punchListPriorityGuid, "COM", "?", LibraryType.PUNCHLIST_PRIORITY));
        _libraryItemRepoMock.GetByGuidAndTypeAsync(_punchListSortingGuid, LibraryType.PUNCHLIST_SORTING, Arg.Any<CancellationToken>())
            .Returns(new LibraryItem(Plant, _punchListSortingGuid, "COM", "?", LibraryType.PUNCHLIST_SORTING));
        _libraryItemRepoMock.GetByGuidAndTypeAsync(_punchListTypeGuid, LibraryType.PUNCHLIST_TYPE, Arg.Any<CancellationToken>())
            .Returns(new LibraryItem(Plant, _punchListTypeGuid, "COM", "?", LibraryType.PUNCHLIST_TYPE));

        _workOrderRepoMock.GetAsync(_originalWoGuid, Arg.Any<CancellationToken>())
            .Returns(new WorkOrder(Plant, _originalWoGuid, "NO123"));
        _workOrderRepoMock.GetAsync(_woGuid, Arg.Any<CancellationToken>())
            .Returns(new WorkOrder(Plant, _woGuid, "NO321"));

        _swcrRepoMock.GetAsync(_swcrGuid, Arg.Any<CancellationToken>()).Returns(new SWCR(Plant, _swcrGuid, 123));
        _personRepoMock.GetOrCreateAsync(actionByGuid, Arg.Any<CancellationToken>()).Returns(
            new Person(actionByGuid, "Ola", "Hansen", "oh", "oh@eqn.com", false));
        _personRepoMock.GetOrCreateAsync(createdByGuid, Arg.Any<CancellationToken>()).Returns(
            new Person(createdByGuid, "Hans", "Olsen", "ho", "ho@eqn.com", false));
        _documentRepoMock.GetAsync(_documentGuid, Arg.Any<CancellationToken>())
            .Returns(new Document(Plant, _documentGuid, "AS987"));

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_punchItemAddedToRepository);
        Assert.AreEqual(guid, _punchItemAddedToRepository.Guid);
        Assert.AreEqual(Plant, _punchItemAddedToRepository.Plant);
        _punchItemRepoMock.Received(1).Add(Arg.Any<PunchItem>());
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var bEvent = GetBusEvent(Guid.Empty, Plant, null);

        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.Consume(_contextMock), "Message is missing ProCoSysGuid");
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoPlant()
    {
        //Arrange
        var bEvent = GetBusEvent(Guid.NewGuid(), "", null);
        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.Consume(_contextMock), "Message is missing Plant");
    }
    
    private static PunchItemEvent GetBusEvent(Guid guid, string plant, string? behavior) =>
        GetTestEvent(guid, plant, s_projectGuid,
            "description",
            Guid.Empty,
            Category.PA,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            "55",
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            false,
            false,
            "NO123",
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            Guid.Empty);

    private static PunchItemEvent GetTestEvent(
        Guid guid,
        string plant,
        Guid projectGuid,
        string description,
        Guid checklistGuid,
        Category category,
        Guid raisedByOrgGuid,
        Guid clearingByOrgGuid,
        Guid punchListSortingGuid,
        Guid punchListTypeGuid,
        Guid punchPriorityGuid,
        string estimate,
        Guid originalWoGuid,
        Guid woGuid,
        Guid swcrGuid,
        Guid documentGuid,
        bool materialRequired,
        bool isVoided,
        string materialExternalNo,
        Guid modifiedByGuid,
        Guid clearedByGuid,
        Guid rejectedByGuid,
        Guid verifiedByGuid,
        Guid createdByGuid,
        Guid actionByGuid
        ) => new(
        string.Empty,
        plant,
        guid,
        string.Empty,
        projectGuid,
        DateTime.UtcNow,
        long.MinValue,
        description,
        long.MinValue,
        checklistGuid,
        category.ToString(),
        raisedByOrgGuid,
        string.Empty,
        string.Empty,
        clearingByOrgGuid,
        DateTime.UtcNow,
        string.Empty,
        punchListSortingGuid,
        string.Empty,
        punchListTypeGuid,
        string.Empty,
        punchPriorityGuid,
        estimate,
        string.Empty,
        originalWoGuid,
        string.Empty,
        woGuid,
        string.Empty,
        swcrGuid,
        string.Empty,
        documentGuid,
        string.Empty,
        materialRequired,
        isVoided,
        DateTime.UtcNow,
        materialExternalNo,
        DateTime.UtcNow,
        DateTime.UtcNow,
        DateTime.UtcNow,
        DateTime.UtcNow,
        modifiedByGuid,
        clearedByGuid,
        rejectedByGuid,
        verifiedByGuid,
        createdByGuid,
        actionByGuid
    );
}
