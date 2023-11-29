using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItem;

[TestClass]
public class UpdatePunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private IPersonCache _personCacheMock;
    private UpdatePunchItemCommand _command;
    private UpdatePunchItemCommandHandler _dut;
    private readonly string _newDescription = Guid.NewGuid().ToString();
    private readonly DateTime _newDueTimeUtc = DateTime.UtcNow.AddDays(7);
    private readonly int _newEstimate = 7;
    private readonly string _newExternalItemNo = "1A";
    private readonly bool _newMaterialRequired = true;
    private readonly DateTime _newMaterialETAUtc = DateTime.UtcNow.AddDays(17);
    private readonly string _newMaterialExternalNo = "B7";
    private Person _personAddedToRepository;

    [TestInitialize]
    public void Setup()
    {
        _personRepositoryMock
            .When(x => x.Add(Arg.Any<Person>()))
            .Do(callInfo =>
            {
                _personAddedToRepository = callInfo.Arg<Person>();
            });

        _command = new UpdatePunchItemCommand(
            _existingPunchItem.Guid,
            new JsonPatchDocument<PatchablePunchItem>(),
            RowVersion);
        _command.PatchDocument.Replace(p => p.Description, _newDescription);
        _command.PatchDocument.Replace(p => p.RaisedByOrgGuid, _existingRaisedByOrg1.Guid);
        _command.PatchDocument.Replace(p => p.ClearingByOrgGuid, _existingClearingByOrg1.Guid);
        _command.PatchDocument.Replace(p => p.PriorityGuid, _existingPriority1.Guid);
        _command.PatchDocument.Replace(p => p.SortingGuid, _existingSorting1.Guid);
        _command.PatchDocument.Replace(p => p.TypeGuid, _existingType1.Guid);
        _command.PatchDocument.Replace(p => p.ActionByPersonOid, _existingPerson1.Guid);
        _command.PatchDocument.Replace(p => p.DueTimeUtc, _newDueTimeUtc);
        _command.PatchDocument.Replace(p => p.Estimate, _newEstimate);
        _command.PatchDocument.Replace(p => p.OriginalWorkOrderGuid, _existingWorkOrder1.Guid);
        _command.PatchDocument.Replace(p => p.WorkOrderGuid, _existingWorkOrder1.Guid);
        _command.PatchDocument.Replace(p => p.SWCRGuid, _existingSWCR1.Guid);
        _command.PatchDocument.Replace(p => p.DocumentGuid, _existingDocument1.Guid);
        _command.PatchDocument.Replace(p => p.ExternalItemNo, _newExternalItemNo);
        _command.PatchDocument.Replace(p => p.MaterialRequired, _newMaterialRequired);
        _command.PatchDocument.Replace(p => p.MaterialETAUtc, _newMaterialETAUtc);
        _command.PatchDocument.Replace(p => p.MaterialExternalNo, _newMaterialExternalNo);

        _command.EnsureValidInputValidation();

        _personCacheMock = Substitute.For<IPersonCache>();
        _dut = new UpdatePunchItemCommandHandler(
            _punchItemRepositoryMock,
            _libraryItemRepositoryMock,
            _personCacheMock,
            _personRepositoryMock,
            _workOrderRepositoryMock,
            _swcrRepositoryMock,
            _documentRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            Substitute.For<ILogger<UpdatePunchItemCommandHandler>>());

        _personCacheMock = Substitute.For<IPersonCache>();
    }

    #region test update when patchDocument contains operations
    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateMaterialRequired_OnPunchItem_WhenOperationsGiven()
    {
        // Arrange. Test MaterialRequired outside other tests since its neither required or can't be set null
        Assert.AreNotEqual(_newMaterialRequired, _existingPunchItem.MaterialRequired);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_newMaterialRequired, _existingPunchItem.MaterialRequired);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateRequiredProps_OnPunchItem_WhenOperationsGiven()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_newDescription, _existingPunchItem.Description);
        Assert.AreEqual(_existingRaisedByOrg1.Id, _existingPunchItem.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg1.Id, _existingPunchItem.ClearingByOrgId);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateOptionalProps_FromNullToValue_OnPunchItem_WhenOperationsGiven()
    {
        // Don't test MaterialRequired here. Can't be null
        // Arrange
        Assert.IsNull(_existingPunchItem.PriorityId);
        Assert.IsNull(_existingPunchItem.SortingId);
        Assert.IsNull(_existingPunchItem.TypeId);
        Assert.IsNull(_existingPunchItem.ActionById);
        Assert.IsNull(_existingPunchItem.DueTimeUtc);
        Assert.IsNull(_existingPunchItem.Estimate);
        Assert.IsNull(_existingPunchItem.OriginalWorkOrder);
        Assert.IsNull(_existingPunchItem.WorkOrder);
        Assert.IsNull(_existingPunchItem.SWCR);
        Assert.IsNull(_existingPunchItem.Document);
        Assert.IsNull(_existingPunchItem.ExternalItemNo);
        Assert.IsNull(_existingPunchItem.MaterialETAUtc);
        Assert.IsNull(_existingPunchItem.MaterialExternalNo);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_existingPriority1.Id, _existingPunchItem.PriorityId);
        Assert.AreEqual(_existingSorting1.Id, _existingPunchItem.SortingId);
        Assert.AreEqual(_existingType1.Id, _existingPunchItem.TypeId);
        Assert.AreEqual(_existingPerson1.Id, _existingPunchItem.ActionById);
        Assert.AreEqual(_newDueTimeUtc, _existingPunchItem.DueTimeUtc);
        Assert.AreEqual(_newEstimate, _existingPunchItem.Estimate);
        Assert.AreEqual(_existingWorkOrder1.Id, _existingPunchItem.OriginalWorkOrderId);
        Assert.AreEqual(_existingWorkOrder1.Id, _existingPunchItem.WorkOrderId);
        Assert.AreEqual(_existingSWCR1.Id, _existingPunchItem.SWCRId);
        Assert.AreEqual(_existingDocument1.Id, _existingPunchItem.DocumentId);
        Assert.AreEqual(_newExternalItemNo, _existingPunchItem.ExternalItemNo);
        Assert.AreEqual(_newMaterialETAUtc, _existingPunchItem.MaterialETAUtc);
        Assert.AreEqual(_newMaterialExternalNo, _existingPunchItem.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateOptionalProps_FromValueToNull_OnPunchItem_WhenOperationsGiven()
    {
        // Don't test MaterialRequired here. Can't be set null
        // Arrange
        _existingPunchItem.SetPriority(_existingPriority1);
        _existingPunchItem.SetSorting(_existingSorting1);
        _existingPunchItem.SetType(_existingType1);
        _existingPunchItem.SetActionBy(_existingPerson1);
        _existingPunchItem.DueTimeUtc = _newDueTimeUtc;
        _existingPunchItem.Estimate = _newEstimate;
        _existingPunchItem.SetOriginalWorkOrder(_existingWorkOrder1);
        _existingPunchItem.SetWorkOrder(_existingWorkOrder1);
        _existingPunchItem.SetSWCR(_existingSWCR1);
        _existingPunchItem.SetDocument(_existingDocument1);
        _existingPunchItem.ExternalItemNo = _newExternalItemNo;
        _existingPunchItem.MaterialETAUtc = _newMaterialETAUtc;
        _existingPunchItem.MaterialExternalNo = _newMaterialExternalNo;

        Assert.IsNotNull(_existingPunchItem.PriorityId);
        Assert.IsNotNull(_existingPunchItem.SortingId);
        Assert.IsNotNull(_existingPunchItem.TypeId);
        Assert.IsNotNull(_existingPunchItem.ActionById);
        Assert.IsNotNull(_existingPunchItem.DueTimeUtc);
        Assert.IsNotNull(_existingPunchItem.Estimate);
        Assert.IsNotNull(_existingPunchItem.OriginalWorkOrder);
        Assert.IsNotNull(_existingPunchItem.WorkOrder);
        Assert.IsNotNull(_existingPunchItem.SWCR);
        Assert.IsNotNull(_existingPunchItem.Document);
        Assert.IsNotNull(_existingPunchItem.ExternalItemNo);
        Assert.IsNotNull(_existingPunchItem.MaterialETAUtc);
        Assert.IsNotNull(_existingPunchItem.MaterialExternalNo);

        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.PriorityGuid, null);
        _command.PatchDocument.Replace(p => p.SortingGuid, null);
        _command.PatchDocument.Replace(p => p.TypeGuid, null);
        _command.PatchDocument.Replace(p => p.ActionByPersonOid, null);
        _command.PatchDocument.Replace(p => p.DueTimeUtc, null);
        _command.PatchDocument.Replace(p => p.Estimate, null);
        _command.PatchDocument.Replace(p => p.OriginalWorkOrderGuid, null);
        _command.PatchDocument.Replace(p => p.WorkOrderGuid, null);
        _command.PatchDocument.Replace(p => p.SWCRGuid, null);
        _command.PatchDocument.Replace(p => p.DocumentGuid, null);
        _command.PatchDocument.Replace(p => p.ExternalItemNo, null);
        _command.PatchDocument.Replace(p => p.MaterialETAUtc, null);
        _command.PatchDocument.Replace(p => p.MaterialExternalNo, null);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNull(_existingPunchItem.PriorityId);
        Assert.IsNull(_existingPunchItem.SortingId);
        Assert.IsNull(_existingPunchItem.TypeId);
        Assert.IsNull(_existingPunchItem.ActionById);
        Assert.IsNull(_existingPunchItem.DueTimeUtc);
        Assert.IsNull(_existingPunchItem.Estimate);
        Assert.IsNull(_existingPunchItem.OriginalWorkOrderId);
        Assert.IsNull(_existingPunchItem.WorkOrderId);
        Assert.IsNull(_existingPunchItem.SWCRId);
        Assert.IsNull(_existingPunchItem.DocumentId);
        Assert.IsNull(_existingPunchItem.ExternalItemNo);
        Assert.IsNull(_existingPunchItem.MaterialETAUtc);
        Assert.IsNull(_existingPunchItem.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateOptionalProps_FromValueToOtherValue_OnPunchItem_WhenOperationsGiven()
    {
        // Don't test MaterialRequired here. Covered in HandlingCommand_ShouldUpdateMaterialRequired_OnPunchItem_WhenOperationsGiven
        // Arrange
        _existingPunchItem.SetPriority(_existingPriority1);
        _existingPunchItem.SetSorting(_existingSorting1);
        _existingPunchItem.SetType(_existingType1);
        _existingPunchItem.SetActionBy(_existingPerson1);
        _existingPunchItem.DueTimeUtc = _newDueTimeUtc;
        _existingPunchItem.Estimate = _newEstimate;
        _existingPunchItem.SetOriginalWorkOrder(_existingWorkOrder1);
        _existingPunchItem.SetWorkOrder(_existingWorkOrder1);
        _existingPunchItem.SetSWCR(_existingSWCR1);
        _existingPunchItem.SetDocument(_existingDocument1);
        _existingPunchItem.ExternalItemNo = _newExternalItemNo;
        _existingPunchItem.MaterialETAUtc = _newMaterialETAUtc;
        _existingPunchItem.MaterialExternalNo = _newMaterialExternalNo;

        Assert.IsNotNull(_existingPunchItem.PriorityId);
        Assert.IsNotNull(_existingPunchItem.SortingId);
        Assert.IsNotNull(_existingPunchItem.TypeId);
        Assert.IsNotNull(_existingPunchItem.ActionById);
        Assert.IsNotNull(_existingPunchItem.DueTimeUtc);
        Assert.IsNotNull(_existingPunchItem.Estimate);
        Assert.IsNotNull(_existingPunchItem.OriginalWorkOrderId);
        Assert.IsNotNull(_existingPunchItem.WorkOrderId);
        Assert.IsNotNull(_existingPunchItem.SWCRId);
        Assert.IsNotNull(_existingPunchItem.DocumentId);
        Assert.IsNotNull(_existingPunchItem.ExternalItemNo);
        Assert.IsNotNull(_existingPunchItem.MaterialETAUtc);
        Assert.IsNotNull(_existingPunchItem.MaterialExternalNo);

        var dueTimeUtc = _newDueTimeUtc.AddDays(1);
        var estimate = _newEstimate * 2;
        var externalItemNo = $"{_newExternalItemNo}-2";
        var materialETAUtc = _newMaterialETAUtc.AddDays(2);
        var materialExternalNo = $"{_newMaterialExternalNo}-X1";

        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.PriorityGuid, _existingPriority2.Guid);
        _command.PatchDocument.Replace(p => p.SortingGuid, _existingSorting2.Guid);
        _command.PatchDocument.Replace(p => p.TypeGuid, _existingType2.Guid);
        _command.PatchDocument.Replace(p => p.ActionByPersonOid, _existingPerson2.Guid);
        _command.PatchDocument.Replace(p => p.DueTimeUtc, dueTimeUtc);
        _command.PatchDocument.Replace(p => p.Estimate, estimate);
        _command.PatchDocument.Replace(p => p.OriginalWorkOrderGuid, _existingWorkOrder2.Guid);
        _command.PatchDocument.Replace(p => p.WorkOrderGuid, _existingWorkOrder2.Guid);
        _command.PatchDocument.Replace(p => p.SWCRGuid, _existingSWCR2.Guid);
        _command.PatchDocument.Replace(p => p.DocumentGuid, _existingDocument2.Guid);
        _command.PatchDocument.Replace(p => p.ExternalItemNo, externalItemNo);
        _command.PatchDocument.Replace(p => p.MaterialETAUtc, materialETAUtc);
        _command.PatchDocument.Replace(p => p.MaterialExternalNo, materialExternalNo);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_existingPriority2.Id, _existingPunchItem.PriorityId);
        Assert.AreEqual(_existingSorting2.Id, _existingPunchItem.SortingId);
        Assert.AreEqual(_existingType2.Id, _existingPunchItem.TypeId);
        Assert.AreEqual(_existingPerson2.Id, _existingPunchItem.ActionById);
        Assert.AreEqual(dueTimeUtc, _existingPunchItem.DueTimeUtc);
        Assert.AreEqual(estimate, _existingPunchItem.Estimate);
        Assert.AreEqual(_existingWorkOrder2.Id, _existingPunchItem.OriginalWorkOrderId);
        Assert.AreEqual(_existingWorkOrder2.Id, _existingPunchItem.WorkOrderId);
        Assert.AreEqual(_existingSWCR2.Id, _existingPunchItem.SWCRId);
        Assert.AreEqual(_existingDocument2.Id, _existingPunchItem.DocumentId);
        Assert.AreEqual(externalItemNo, _existingPunchItem.ExternalItemNo);
        Assert.AreEqual(materialETAUtc, _existingPunchItem.MaterialETAUtc);
        Assert.AreEqual(materialExternalNo, _existingPunchItem.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave_WhenOperationsGiven()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAndReturnRowVersion_WhenOperationsGiven()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_command.RowVersion, result.Data);
        Assert.AreEqual(_command.RowVersion, _existingPunchItem.RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddPunchItemUpdatedEvent_WhenOperationsGiven()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItemUpdatedDomainEventAdded = _existingPunchItem.DomainEvents.Last();
        Assert.IsInstanceOfType(punchItemUpdatedDomainEventAdded, typeof(PunchItemUpdatedDomainEvent));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddChangesToPunchItemUpdatedEvent_WhenOperationsGiven()
    {
        // Arrange
        var oldDescription = _existingPunchItem.Description;
        var oldRaisedByCode = _existingPunchItem.RaisedByOrg.Code;
        var oldClearingByOrg = _existingPunchItem.ClearingByOrg.Code;
        var oldMaterialRequired = _existingPunchItem.MaterialRequired;

        //_syncToPCS4ServiceMock.When(x => x.SyncUpdatesAsync("punchitem", _existingPunchItem, default).IsCompletedSuccessfully);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItemUpdatedDomainEventAdded = _existingPunchItem.DomainEvents.Last() as PunchItemUpdatedDomainEvent;
        Assert.IsNotNull(punchItemUpdatedDomainEventAdded);
        Assert.IsNotNull(punchItemUpdatedDomainEventAdded.Changes);
        Assert.AreEqual(17, _command.PatchDocument.Operations.Count);
        Assert.AreEqual(_command.PatchDocument.Operations.Count, punchItemUpdatedDomainEventAdded.Changes.Count);

        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Description)),
            oldDescription,
            _newDescription);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.RaisedByOrg)),
            oldRaisedByCode,
            _existingRaisedByOrg1.Code);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ClearingByOrg)),
            oldClearingByOrg,
            _existingClearingByOrg1.Code);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Priority)),
            null,
            _existingPriority1.Code);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Sorting)),
            null,
            _existingSorting1.Code);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Type)),
            null,
            _existingType1.Code);
        AssertPersonChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ActionBy)),
            null,
            new User(_existingPerson1.Guid, _existingPerson1.GetFullName()));
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.DueTimeUtc)),
            null,
            _newDueTimeUtc);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Estimate)),
            null,
            _newEstimate);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.OriginalWorkOrder)),
            null,
            _existingWorkOrder1.No);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.WorkOrder)),
            null,
            _existingWorkOrder1.No);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.SWCR)),
            null,
            _existingSWCR1.No);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Document)),
            null,
            _existingDocument1.No);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ExternalItemNo)),
            null,
            _newExternalItemNo);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialRequired)),
            oldMaterialRequired,
            _newMaterialRequired);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialETAUtc)),
            null,
            _newMaterialETAUtc);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialExternalNo)),
            null,
            _newMaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddChangesToPunchItemUpdatedEvent_WhenOperationsWithNullGiven()
    {
        // Don't test MaterialRequired here. Can't be null
        // Arrange
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.PriorityGuid, null);
        _command.PatchDocument.Replace(p => p.SortingGuid, null);
        _command.PatchDocument.Replace(p => p.TypeGuid, null);
        _command.PatchDocument.Replace(p => p.ActionByPersonOid, null);
        _command.PatchDocument.Replace(p => p.DueTimeUtc, null);
        _command.PatchDocument.Replace(p => p.Estimate, null);
        _command.PatchDocument.Replace(p => p.OriginalWorkOrderGuid, null);
        _command.PatchDocument.Replace(p => p.WorkOrderGuid, null);
        _command.PatchDocument.Replace(p => p.SWCRGuid, null);
        _command.PatchDocument.Replace(p => p.DocumentGuid, null);
        _command.PatchDocument.Replace(p => p.ExternalItemNo, null);
        _command.PatchDocument.Replace(p => p.MaterialETAUtc, null);
        _command.PatchDocument.Replace(p => p.MaterialExternalNo, null);

        _existingPunchItem.SetPriority(_existingPriority1);
        _existingPunchItem.SetSorting(_existingSorting1);
        _existingPunchItem.SetType(_existingType1);
        _existingPunchItem.SetActionBy(_existingPerson1);
        _existingPunchItem.DueTimeUtc = _newDueTimeUtc;
        _existingPunchItem.Estimate = _newEstimate;
        _existingPunchItem.SetOriginalWorkOrder(_existingWorkOrder1);
        _existingPunchItem.SetWorkOrder(_existingWorkOrder1);
        _existingPunchItem.SetSWCR(_existingSWCR1);
        _existingPunchItem.SetDocument(_existingDocument1);
        _existingPunchItem.ExternalItemNo = _newExternalItemNo;
        _existingPunchItem.MaterialETAUtc = _newMaterialETAUtc;
        _existingPunchItem.MaterialExternalNo = _newMaterialExternalNo;

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItemUpdatedDomainEventAdded = _existingPunchItem.DomainEvents.Last() as PunchItemUpdatedDomainEvent;
        Assert.IsNotNull(punchItemUpdatedDomainEventAdded);
        Assert.IsNotNull(punchItemUpdatedDomainEventAdded.Changes);
        Assert.AreEqual(13, _command.PatchDocument.Operations.Count);
        Assert.AreEqual(_command.PatchDocument.Operations.Count, punchItemUpdatedDomainEventAdded.Changes.Count);

        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Priority)),
            _existingPriority1.Code,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Sorting)),
            _existingSorting1.Code,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Type)),
            _existingType1.Code,
            null);
        AssertPersonChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ActionBy)),
            new User(_existingPerson1.Guid, _existingPerson1.GetFullName()),
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.DueTimeUtc)),
            _newDueTimeUtc,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Estimate)),
            _newEstimate,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.OriginalWorkOrder)),
            _existingWorkOrder1.No,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.WorkOrder)),
            _existingWorkOrder1.No,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.SWCR)),
            _existingSWCR1.No,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Document)),
            _existingDocument1.No,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ExternalItemNo)),
            _newExternalItemNo,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialETAUtc)),
            _newMaterialETAUtc,
            null);
        AssertChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialExternalNo)),
            _newMaterialExternalNo,
            null);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchDescriptionWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.Description, _existingPunchItem.Description);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchRaisedByOrgWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.RaisedByOrgGuid, _existingPunchItem.RaisedByOrg.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchClearingByOrgWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.ClearingByOrgGuid, _existingPunchItem.ClearingByOrg.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchPriorityWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetPriority(_existingPriority1);
        _command.PatchDocument.Replace(p => p.PriorityGuid, _existingPunchItem.Priority!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchSortingWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetSorting(_existingSorting1);
        _command.PatchDocument.Replace(p => p.SortingGuid, _existingPunchItem.Sorting!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchTypeWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetType(_existingType1);
        _command.PatchDocument.Replace(p => p.TypeGuid, _existingPunchItem.Type!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchActionByWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetActionBy(_existingPerson1);
        _command.PatchDocument.Replace(p => p.ActionByPersonOid, _existingPunchItem.ActionBy!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchDueTimeWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.DueTimeUtc = _newDueTimeUtc;
        _command.PatchDocument.Replace(p => p.DueTimeUtc, _newDueTimeUtc);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchEstimateWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.Estimate = _newEstimate;
        _command.PatchDocument.Replace(p => p.Estimate, _newEstimate);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchOriginalWorkOrderWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetOriginalWorkOrder(_existingWorkOrder1);
        _command.PatchDocument.Replace(p => p.OriginalWorkOrderGuid, _existingWorkOrder1.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchWorkOrderWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetWorkOrder(_existingWorkOrder1);
        _command.PatchDocument.Replace(p => p.WorkOrderGuid, _existingWorkOrder1.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchSWCRWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetSWCR(_existingSWCR1);
        _command.PatchDocument.Replace(p => p.SWCRGuid, _existingSWCR1.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchDocumentWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.SetDocument(_existingDocument1);
        _command.PatchDocument.Replace(p => p.DocumentGuid, _existingDocument1.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchExternalItemNoWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.ExternalItemNo = _newExternalItemNo;
        _command.PatchDocument.Replace(p => p.ExternalItemNo, _newExternalItemNo);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchMaterialRequiredWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.MaterialRequired = _newMaterialRequired;
        _command.PatchDocument.Replace(p => p.MaterialRequired, _newMaterialRequired);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchMaterialETAWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.MaterialETAUtc = _newMaterialETAUtc;
        _command.PatchDocument.Replace(p => p.MaterialETAUtc, _newMaterialETAUtc);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenPatchMaterialExternalNoWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem.MaterialExternalNo = _newMaterialExternalNo;
        _command.PatchDocument.Replace(p => p.MaterialExternalNo, _newMaterialExternalNo);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }

    [TestMethod]
    public async Task HandlingCommand_WithNonExistingActionByPerson_ShouldAddActionByPerson_ToPersonRepository()
    {
        // Arrange
        var nonExistingPersonOid = Guid.NewGuid();
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.ActionByPersonOid, nonExistingPersonOid);
        var proCoSysPerson = new ProCoSysPerson
        {
            UserName = "YODA",
            FirstName = "YO",
            LastName = "DA",
            Email = "@",
            AzureOid = nonExistingPersonOid.ToString()
        };
        _personCacheMock.GetAsync(nonExistingPersonOid)
            .Returns(proCoSysPerson);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItemUpdatedDomainEventAdded = _existingPunchItem.DomainEvents.Last() as PunchItemUpdatedDomainEvent;
        Assert.IsNotNull(punchItemUpdatedDomainEventAdded);
        Assert.IsNotNull(punchItemUpdatedDomainEventAdded.Changes);
        Assert.AreEqual(1, _command.PatchDocument.Operations.Count);
        Assert.AreEqual(_command.PatchDocument.Operations.Count, punchItemUpdatedDomainEventAdded.Changes.Count);
        AssertPersonChange(
            punchItemUpdatedDomainEventAdded
                .Changes
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ActionBy)),
            null,
            new User(nonExistingPersonOid, $"{proCoSysPerson.FirstName} {proCoSysPerson.LastName}"));
        Assert.IsNotNull(_personAddedToRepository);
        Assert.AreEqual(nonExistingPersonOid, _existingPunchItem.ActionBy!.Guid);
        Assert.AreEqual(nonExistingPersonOid, _personAddedToRepository.Guid);
        Assert.AreEqual(proCoSysPerson.AzureOid, _personAddedToRepository.Guid.ToString());
        Assert.AreEqual(proCoSysPerson.UserName, _personAddedToRepository.UserName);
        Assert.AreEqual(proCoSysPerson.FirstName, _personAddedToRepository.FirstName);
        Assert.AreEqual(proCoSysPerson.LastName, _personAddedToRepository.LastName);
        Assert.AreEqual(proCoSysPerson.Email, _personAddedToRepository.Email);
    }

    #endregion

    #region test update when patchDocument don't contains any operations
    [TestMethod]
    public async Task HandlingCommand_ShouldNotUpdatePunchItem_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        var oldDescription = _existingPunchItem.Description;
        var oldRaisedByOrgId = _existingPunchItem.RaisedByOrgId;
        var oldClearingByOrgId = _existingPunchItem.ClearingByOrgId;
        var oldPriorityId = _existingPunchItem.PriorityId;
        var oldTypeId = _existingPunchItem.TypeId;
        var oldSortingId = _existingPunchItem.SortingId;
        var oldActionById = _existingPunchItem.ActionById;
        var oldDueTimeUtc = _existingPunchItem.DueTimeUtc;
        var oldEstimate = _existingPunchItem.Estimate;
        var oldOriginalWorkOrderId = _existingPunchItem.OriginalWorkOrderId;
        var oldWorkOrderId = _existingPunchItem.WorkOrder;
        var oldSWCRId = _existingPunchItem.SWCRId;
        var oldDocumentId = _existingPunchItem.DocumentId;
        var oldMaterialRequired = _existingPunchItem.MaterialRequired;
        var oldMaterialETAUtc = _existingPunchItem.MaterialETAUtc;
        var oldMaterialExternalNo = _existingPunchItem.MaterialExternalNo;

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(oldDescription, _existingPunchItem.Description);
        Assert.AreEqual(oldRaisedByOrgId, _existingPunchItem.RaisedByOrgId);
        Assert.AreEqual(oldClearingByOrgId, _existingPunchItem.ClearingByOrgId);
        Assert.AreEqual(oldPriorityId, _existingPunchItem.PriorityId);
        Assert.AreEqual(oldSortingId, _existingPunchItem.SortingId);
        Assert.AreEqual(oldTypeId, _existingPunchItem.TypeId);
        Assert.AreEqual(oldActionById, _existingPunchItem.ActionById);
        Assert.AreEqual(oldDueTimeUtc, _existingPunchItem.DueTimeUtc);
        Assert.AreEqual(oldEstimate, _existingPunchItem.Estimate);
        Assert.AreEqual(oldOriginalWorkOrderId, _existingPunchItem.OriginalWorkOrderId);
        Assert.AreEqual(oldWorkOrderId, _existingPunchItem.WorkOrder);
        Assert.AreEqual(oldSWCRId, _existingPunchItem.SWCRId);
        Assert.AreEqual(oldDocumentId, _existingPunchItem.DocumentId);
        Assert.AreEqual(oldMaterialExternalNo, _existingPunchItem.MaterialExternalNo);
        Assert.AreEqual(oldMaterialRequired, _existingPunchItem.MaterialRequired);
        Assert.AreEqual(oldMaterialETAUtc, _existingPunchItem.MaterialETAUtc);
        Assert.AreEqual(oldMaterialExternalNo, _existingPunchItem.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAndReturnRowVersion_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_command.RowVersion, result.Data);
        Assert.AreEqual(_command.RowVersion, _existingPunchItem.RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotAddPunchItemUpdatedEvent_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItemUpdatedDomainEventAdded =
            _existingPunchItem.DomainEvents.Any(e => e.GetType() == typeof(PunchItemUpdatedDomainEvent));
        Assert.IsFalse(punchItemUpdatedDomainEventAdded);
    }
    #endregion

    private void AssertChange(IProperty change, object oldValue, object newValue)
    {
        Assert.IsNotNull(change);
        Assert.AreEqual(oldValue, change.OldValue);
        Assert.AreEqual(newValue, change.NewValue);
    }

    private void AssertPersonChange(IProperty change, User oldValue, User newValue)
    {
        Assert.IsNotNull(change);
        if (change.OldValue is null)
        {
            Assert.IsNull(oldValue);
        }
        else
        {
            var user = change.OldValue as User;
            Assert.IsNotNull(user);
            Assert.AreEqual(oldValue.Oid, user.Oid);
            Assert.AreEqual(oldValue.FullName, user.FullName);
        }
        if (change.NewValue is null)
        {
            Assert.IsNull(newValue);
        }
        else
        {
            var user = change.NewValue as User;
            Assert.IsNotNull(user);
            Assert.AreEqual(newValue.Oid, user.Oid);
            Assert.AreEqual(newValue.FullName, user.FullName);
        }
    }
}
