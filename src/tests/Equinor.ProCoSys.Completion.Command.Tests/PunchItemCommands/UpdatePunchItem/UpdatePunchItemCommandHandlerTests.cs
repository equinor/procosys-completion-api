using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItem;

[TestClass]
public class UpdatePunchItemCommandHandlerTests : PunchItemCommandTestsBase
{
    private UpdatePunchItemCommand _command;
    private UpdatePunchItemCommandHandler _dut;
    private readonly string _newDescription = Guid.NewGuid().ToString();
    private readonly DateTime _newDueTimeUtc = DateTime.UtcNow.AddDays(7);
    private readonly int _newEstimate = 7;
    private readonly bool _newMaterialRequired = true;
    private readonly DateTime _newMaterialETAUtc = DateTime.UtcNow.AddDays(17);
    private readonly string _newMaterialExternalNo = "B7";

    [TestInitialize]
    public void Setup()
    {
        _command = new UpdatePunchItemCommand(
            _existingPunchItem[TestPlantA].Guid,
            new JsonPatchDocument<PatchablePunchItem>(),
            RowVersion)
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _command.PatchDocument.Replace(p => p.Description, _newDescription);
        _command.PatchDocument.Replace(p => p.RaisedByOrgGuid, _existingRaisedByOrg1[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.ClearingByOrgGuid, _existingClearingByOrg1[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.PriorityGuid, _existingPriority1[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.SortingGuid, _existingSorting1[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.TypeGuid, _existingType1[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.ActionByPersonOid, _existingPerson1.Guid);
        _command.PatchDocument.Replace(p => p.DueTimeUtc, _newDueTimeUtc);
        _command.PatchDocument.Replace(p => p.Estimate, _newEstimate);
        _command.PatchDocument.Replace(p => p.OriginalWorkOrderGuid, _existingWorkOrder1[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.WorkOrderGuid, _existingWorkOrder1[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.SWCRGuid, _existingSWCR1[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.DocumentGuid, _existingDocument1[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.MaterialRequired, _newMaterialRequired);
        _command.PatchDocument.Replace(p => p.MaterialETAUtc, _newMaterialETAUtc);
        _command.PatchDocument.Replace(p => p.MaterialExternalNo, _newMaterialExternalNo);

        _command.EnsureValidInputValidation();

        _dut = new UpdatePunchItemCommandHandler(
            _libraryItemRepositoryMock,
            _personRepositoryMock,
            _workOrderRepositoryMock,
            _swcrRepositoryMock,
            _documentRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _messageProducerMock,
            Substitute.For<ILogger<UpdatePunchItemCommandHandler>>());
    }

    #region test update when patchDocument contains operations
    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateMaterialRequired_OnPunchItem_WhenOperationsGiven()
    {
        // Arrange. Test MaterialRequired outside other tests since its neither required or can't be set null
        Assert.AreNotEqual(_newMaterialRequired, _existingPunchItem[TestPlantA].MaterialRequired);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_newMaterialRequired, _existingPunchItem[TestPlantA].MaterialRequired);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateRequiredProps_OnPunchItem_WhenOperationsGiven()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_newDescription, _existingPunchItem[TestPlantA].Description);
        Assert.AreEqual(_existingRaisedByOrg1[TestPlantA].Id, _existingPunchItem[TestPlantA].RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg1[TestPlantA].Id, _existingPunchItem[TestPlantA].ClearingByOrgId);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateOptionalProps_FromNullToValue_OnPunchItem_WhenOperationsGiven()
    {
        // Don't test MaterialRequired here. Can't be null
        // Arrange
        Assert.IsNull(_existingPunchItem[TestPlantA].PriorityId);
        Assert.IsNull(_existingPunchItem[TestPlantA].SortingId);
        Assert.IsNull(_existingPunchItem[TestPlantA].TypeId);
        Assert.IsNull(_existingPunchItem[TestPlantA].ActionById);
        Assert.IsNull(_existingPunchItem[TestPlantA].DueTimeUtc);
        Assert.IsNull(_existingPunchItem[TestPlantA].Estimate);
        Assert.IsNull(_existingPunchItem[TestPlantA].OriginalWorkOrder);
        Assert.IsNull(_existingPunchItem[TestPlantA].WorkOrder);
        Assert.IsNull(_existingPunchItem[TestPlantA].SWCR);
        Assert.IsNull(_existingPunchItem[TestPlantA].Document);
        Assert.IsNull(_existingPunchItem[TestPlantA].MaterialETAUtc);
        Assert.IsNull(_existingPunchItem[TestPlantA].MaterialExternalNo);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_existingPriority1[TestPlantA].Id, _existingPunchItem[TestPlantA].PriorityId);
        Assert.AreEqual(_existingSorting1[TestPlantA].Id, _existingPunchItem[TestPlantA].SortingId);
        Assert.AreEqual(_existingType1[TestPlantA].Id, _existingPunchItem[TestPlantA].TypeId);
        Assert.AreEqual(_existingPerson1.Id, _existingPunchItem[TestPlantA].ActionById);
        Assert.AreEqual(_newDueTimeUtc, _existingPunchItem[TestPlantA].DueTimeUtc);
        Assert.AreEqual(_newEstimate, _existingPunchItem[TestPlantA].Estimate);
        Assert.AreEqual(_existingWorkOrder1[TestPlantA].Id, _existingPunchItem[TestPlantA].OriginalWorkOrderId);
        Assert.AreEqual(_existingWorkOrder1[TestPlantA].Id, _existingPunchItem[TestPlantA].WorkOrderId);
        Assert.AreEqual(_existingSWCR1[TestPlantA].Id, _existingPunchItem[TestPlantA].SWCRId);
        Assert.AreEqual(_existingDocument1[TestPlantA].Id, _existingPunchItem[TestPlantA].DocumentId);
        Assert.AreEqual(_newMaterialETAUtc, _existingPunchItem[TestPlantA].MaterialETAUtc);
        Assert.AreEqual(_newMaterialExternalNo, _existingPunchItem[TestPlantA].MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateOptionalProps_FromValueToNull_OnPunchItem_WhenOperationsGiven()
    {
        // Don't test MaterialRequired here. Can't be set null
        // Arrange
        _existingPunchItem[TestPlantA].SetPriority(_existingPriority1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetSorting(_existingSorting1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetType(_existingType1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetActionBy(_existingPerson1);
        _existingPunchItem[TestPlantA].DueTimeUtc = _newDueTimeUtc;
        _existingPunchItem[TestPlantA].Estimate = _newEstimate;
        _existingPunchItem[TestPlantA].SetOriginalWorkOrder(_existingWorkOrder1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetWorkOrder(_existingWorkOrder1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetSWCR(_existingSWCR1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetDocument(_existingDocument1[TestPlantA]);
        _existingPunchItem[TestPlantA].MaterialETAUtc = _newMaterialETAUtc;
        _existingPunchItem[TestPlantA].MaterialExternalNo = _newMaterialExternalNo;

        Assert.IsNotNull(_existingPunchItem[TestPlantA].PriorityId);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].SortingId);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].TypeId);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].ActionById);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].DueTimeUtc);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].Estimate);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].OriginalWorkOrder);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].WorkOrder);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].SWCR);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].Document);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].MaterialETAUtc);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].MaterialExternalNo);

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
        _command.PatchDocument.Replace(p => p.MaterialETAUtc, null);
        _command.PatchDocument.Replace(p => p.MaterialExternalNo, null);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsFalse(_existingPunchItem[TestPlantA].PriorityId.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].SortingId.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].TypeId.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].ActionById.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].DueTimeUtc.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].Estimate.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].OriginalWorkOrderId.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].WorkOrderId.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].SWCRId.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].DocumentId.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].MaterialETAUtc.HasValue);
        Assert.IsTrue(_existingPunchItem[TestPlantA].MaterialExternalNo is null);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUpdateOptionalProps_FromValueToOtherValue_OnPunchItem_WhenOperationsGiven()
    {
        // Don't test MaterialRequired here. Covered in HandlingCommand_ShouldUpdateMaterialRequired_OnPunchItem_WhenOperationsGiven
        // Arrange
        _existingPunchItem[TestPlantA].SetPriority(_existingPriority1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetSorting(_existingSorting1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetType(_existingType1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetActionBy(_existingPerson1);
        _existingPunchItem[TestPlantA].DueTimeUtc = _newDueTimeUtc;
        _existingPunchItem[TestPlantA].Estimate = _newEstimate;
        _existingPunchItem[TestPlantA].SetOriginalWorkOrder(_existingWorkOrder1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetWorkOrder(_existingWorkOrder1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetSWCR(_existingSWCR1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetDocument(_existingDocument1[TestPlantA]);
        _existingPunchItem[TestPlantA].MaterialETAUtc = _newMaterialETAUtc;
        _existingPunchItem[TestPlantA].MaterialExternalNo = _newMaterialExternalNo;

        Assert.IsNotNull(_existingPunchItem[TestPlantA].PriorityId);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].SortingId);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].TypeId);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].ActionById);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].DueTimeUtc);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].Estimate);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].OriginalWorkOrderId);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].WorkOrderId);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].SWCRId);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].DocumentId);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].MaterialETAUtc);
        Assert.IsNotNull(_existingPunchItem[TestPlantA].MaterialExternalNo);

        var dueTimeUtc = _newDueTimeUtc.AddDays(1);
        var estimate = _newEstimate * 2;
        var materialETAUtc = _newMaterialETAUtc.AddDays(2);
        var materialExternalNo = $"{_newMaterialExternalNo}-X1";

        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.PriorityGuid, _existingPriority2[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.SortingGuid, _existingSorting2[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.TypeGuid, _existingType2[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.ActionByPersonOid, _existingPerson2.Guid);
        _command.PatchDocument.Replace(p => p.DueTimeUtc, dueTimeUtc);
        _command.PatchDocument.Replace(p => p.Estimate, estimate);
        _command.PatchDocument.Replace(p => p.OriginalWorkOrderGuid, _existingWorkOrder2[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.WorkOrderGuid, _existingWorkOrder2[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.SWCRGuid, _existingSWCR2[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.DocumentGuid, _existingDocument2[TestPlantA].Guid);
        _command.PatchDocument.Replace(p => p.MaterialETAUtc, materialETAUtc);
        _command.PatchDocument.Replace(p => p.MaterialExternalNo, materialExternalNo);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_existingPriority2[TestPlantA].Id, _existingPunchItem[TestPlantA].PriorityId);
        Assert.AreEqual(_existingSorting2[TestPlantA].Id, _existingPunchItem[TestPlantA].SortingId);
        Assert.AreEqual(_existingType2[TestPlantA].Id, _existingPunchItem[TestPlantA].TypeId);
        Assert.AreEqual(_existingPerson2.Id, _existingPunchItem[TestPlantA].ActionById);
        Assert.AreEqual(dueTimeUtc, _existingPunchItem[TestPlantA].DueTimeUtc);
        Assert.AreEqual(estimate, _existingPunchItem[TestPlantA].Estimate);
        Assert.AreEqual(_existingWorkOrder2[TestPlantA].Id, _existingPunchItem[TestPlantA].OriginalWorkOrderId);
        Assert.AreEqual(_existingWorkOrder2[TestPlantA].Id, _existingPunchItem[TestPlantA].WorkOrderId);
        Assert.AreEqual(_existingSWCR2[TestPlantA].Id, _existingPunchItem[TestPlantA].SWCRId);
        Assert.AreEqual(_existingDocument2[TestPlantA].Id, _existingPunchItem[TestPlantA].DocumentId);
        Assert.AreEqual(materialETAUtc, _existingPunchItem[TestPlantA].MaterialETAUtc);
        Assert.AreEqual(materialExternalNo, _existingPunchItem[TestPlantA].MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAuditData_WhenOperationsGiven()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SetAuditDataAsync();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave_WhenOperationsGiven()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAndReturnRowVersion_WhenOperationsGiven()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_command.RowVersion, result);
        Assert.AreEqual(_command.RowVersion, _existingPunchItem[TestPlantA].RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishPunchItemUpdatedIntegrationEvent_WhenOperationsGiven()
    {
        // Arrange
        PunchItemUpdatedIntegrationEvent integrationEvent = null!;
        _messageProducerMock
            .When(x => x.PublishAsync(Arg.Any<PunchItemUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<PunchItemUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _existingPunchItem[TestPlantA];
        Assert.IsNotNull(integrationEvent);
        AssertRequiredProperties(punchItem, integrationEvent);
        AssertOptionalProperties(punchItem, integrationEvent);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSendHistoryUpdatedIntegrationEvent_WhenOperationsGiven()
    {
        // Arrange
        var oldDescription = _existingPunchItem[TestPlantA].Description;
        var oldRaisedByOrg = _existingPunchItem[TestPlantA].RaisedByOrg.ToString();
        var oldClearingByOrg = _existingPunchItem[TestPlantA].ClearingByOrg.ToString();
        var oldMaterialRequired = _existingPunchItem[TestPlantA].MaterialRequired;
        HistoryUpdatedIntegrationEvent historyEvent = null!;
        _messageProducerMock
            .When(x => x.SendHistoryAsync(Arg.Any<HistoryUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _existingPunchItem[TestPlantA];
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            punchItem.Plant,
            "Punch item updated",
            punchItem,
            punchItem);

        var changedProperties = historyEvent.ChangedProperties;
        Assert.IsNotNull(changedProperties);
        Assert.AreEqual(16, _command.PatchDocument.Operations.Count);
        Assert.AreEqual(16, changedProperties.Count);

        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Description)),
            oldDescription,
            _newDescription);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.RaisedByOrg)),
            oldRaisedByOrg,
            _existingRaisedByOrg1[TestPlantA].ToString());
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ClearingByOrg)),
            oldClearingByOrg,
            _existingClearingByOrg1[TestPlantA].ToString());
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Priority)),
            null,
            _existingPriority1[TestPlantA].ToString());
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Sorting)),
            null,
            _existingSorting1[TestPlantA].ToString());
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Type)),
            null,
            _existingType1[TestPlantA].ToString());
        AssertPersonChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ActionBy)),
            null,
            new User(_existingPerson1.Guid, _existingPerson1.GetFullName()));
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.DueTimeUtc)),
            null,
            _newDueTimeUtc,
            ValueDisplayType.DateTimeAsDateOnly);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Estimate)),
            null,
            _newEstimate,
            ValueDisplayType.IntAsText);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.OriginalWorkOrder)),
            null,
            _existingWorkOrder1[TestPlantA].No);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.WorkOrder)),
            null,
            _existingWorkOrder1[TestPlantA].No);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.SWCR)),
            null,
            _existingSWCR1[TestPlantA].No,
            ValueDisplayType.IntAsText);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Document)),
            null,
            _existingDocument1[TestPlantA].No);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialRequired)),
            oldMaterialRequired,
            _newMaterialRequired,
            ValueDisplayType.BoolAsYesNo);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialETAUtc)),
            null,
            _newMaterialETAUtc, 
            ValueDisplayType.DateTimeAsDateOnly);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialExternalNo)),
            null,
            _newMaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSendHistoryUpdatedIntegrationEvent_WhenOperationsWithNullGiven()
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
        _command.PatchDocument.Replace(p => p.MaterialETAUtc, null);
        _command.PatchDocument.Replace(p => p.MaterialExternalNo, null);

        _existingPunchItem[TestPlantA].SetPriority(_existingPriority1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetSorting(_existingSorting1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetType(_existingType1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetActionBy(_existingPerson1);
        _existingPunchItem[TestPlantA].DueTimeUtc = _newDueTimeUtc;
        _existingPunchItem[TestPlantA].Estimate = _newEstimate;
        _existingPunchItem[TestPlantA].SetOriginalWorkOrder(_existingWorkOrder1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetWorkOrder(_existingWorkOrder1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetSWCR(_existingSWCR1[TestPlantA]);
        _existingPunchItem[TestPlantA].SetDocument(_existingDocument1[TestPlantA]);
        _existingPunchItem[TestPlantA].MaterialETAUtc = _newMaterialETAUtc;
        _existingPunchItem[TestPlantA].MaterialExternalNo = _newMaterialExternalNo;
        HistoryUpdatedIntegrationEvent historyEvent = null!;
        _messageProducerMock
            .When(x => x.SendHistoryAsync(Arg.Any<HistoryUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _existingPunchItem[TestPlantA];
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            punchItem.Plant,
            "Punch item updated",
            punchItem,
            punchItem);

        var changedProperties = historyEvent.ChangedProperties;
        Assert.IsNotNull(changedProperties);
        Assert.AreEqual(12, _command.PatchDocument.Operations.Count);
        Assert.AreEqual(12, changedProperties.Count);

        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Priority)),
            _existingPriority1[TestPlantA].ToString(),
            null);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Sorting)),
            _existingSorting1[TestPlantA].ToString(),
            null);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Type)),
            _existingType1[TestPlantA].ToString(),
            null);
        AssertPersonChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ActionBy)),
            new User(_existingPerson1.Guid, _existingPerson1.GetFullName()),
            null);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.DueTimeUtc)),
            _newDueTimeUtc,
            null,
            ValueDisplayType.DateTimeAsDateOnly);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Estimate)),
            _newEstimate,
            null,
            ValueDisplayType.IntAsText);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.OriginalWorkOrder)),
            _existingWorkOrder1[TestPlantA].No,
            null);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.WorkOrder)),
            _existingWorkOrder1[TestPlantA].No,
            null);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.SWCR)),
            _existingSWCR1[TestPlantA].No,
            null, 
            ValueDisplayType.IntAsText);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Document)),
            _existingDocument1[TestPlantA].No,
            null);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialETAUtc)),
            _newMaterialETAUtc,
            null, 
            ValueDisplayType.DateTimeAsDateOnly);
        AssertChange(
            changedProperties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialExternalNo)),
            _newMaterialExternalNo,
            null);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchDescriptionWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.Description, _existingPunchItem[TestPlantA].Description);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchRaisedByOrgWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.RaisedByOrgGuid, _existingPunchItem[TestPlantA].RaisedByOrg.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchClearingByOrgWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _command.PatchDocument.Replace(p => p.ClearingByOrgGuid, _existingPunchItem[TestPlantA].ClearingByOrg.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchPriorityWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].SetPriority(_existingPriority1[TestPlantA]);
        _command.PatchDocument.Replace(p => p.PriorityGuid, _existingPunchItem[TestPlantA].Priority!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchSortingWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].SetSorting(_existingSorting1[TestPlantA]);
        _command.PatchDocument.Replace(p => p.SortingGuid, _existingPunchItem[TestPlantA].Sorting!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchTypeWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].SetType(_existingType1[TestPlantA]);
        _command.PatchDocument.Replace(p => p.TypeGuid, _existingPunchItem[TestPlantA].Type!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchActionByWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].SetActionBy(_existingPerson1);
        _command.PatchDocument.Replace(p => p.ActionByPersonOid, _existingPunchItem[TestPlantA].ActionBy!.Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchDueTimeWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].DueTimeUtc = _newDueTimeUtc;
        _command.PatchDocument.Replace(p => p.DueTimeUtc, _newDueTimeUtc);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchEstimateWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].Estimate = _newEstimate;
        _command.PatchDocument.Replace(p => p.Estimate, _newEstimate);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchOriginalWorkOrderWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].SetOriginalWorkOrder(_existingWorkOrder1[TestPlantA]);
        _command.PatchDocument.Replace(p => p.OriginalWorkOrderGuid, _existingWorkOrder1[TestPlantA].Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchWorkOrderWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].SetWorkOrder(_existingWorkOrder1[TestPlantA]);
        _command.PatchDocument.Replace(p => p.WorkOrderGuid, _existingWorkOrder1[TestPlantA].Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchSWCRWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].SetSWCR(_existingSWCR1[TestPlantA]);
        _command.PatchDocument.Replace(p => p.SWCRGuid, _existingSWCR1[TestPlantA].Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchDocumentWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].SetDocument(_existingDocument1[TestPlantA]);
        _command.PatchDocument.Replace(p => p.DocumentGuid, _existingDocument1[TestPlantA].Guid);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenPatchMaterialRequiredWithSameValue()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        _existingPunchItem[TestPlantA].MaterialRequired = _newMaterialRequired;
        _command.PatchDocument.Replace(p => p.MaterialRequired, _newMaterialRequired);

        // Act
        await _dut.Handle(_command, default);

        // Assert 
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region test update when patchDocument don't contains any operations
    [TestMethod]
    public async Task HandlingCommand_ShouldNotUpdatePunchItem_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();
        var oldDescription = _existingPunchItem[TestPlantA].Description;
        var oldRaisedByOrgId = _existingPunchItem[TestPlantA].RaisedByOrgId;
        var oldClearingByOrgId = _existingPunchItem[TestPlantA].ClearingByOrgId;
        var oldPriorityId = _existingPunchItem[TestPlantA].PriorityId;
        var oldTypeId = _existingPunchItem[TestPlantA].TypeId;
        var oldSortingId = _existingPunchItem[TestPlantA].SortingId;
        var oldActionById = _existingPunchItem[TestPlantA].ActionById;
        var oldDueTimeUtc = _existingPunchItem[TestPlantA].DueTimeUtc;
        var oldEstimate = _existingPunchItem[TestPlantA].Estimate;
        var oldOriginalWorkOrderId = _existingPunchItem[TestPlantA].OriginalWorkOrderId;
        var oldWorkOrderId = _existingPunchItem[TestPlantA].WorkOrder;
        var oldSWCRId = _existingPunchItem[TestPlantA].SWCRId;
        var oldDocumentId = _existingPunchItem[TestPlantA].DocumentId;
        var oldMaterialRequired = _existingPunchItem[TestPlantA].MaterialRequired;
        var oldMaterialETAUtc = _existingPunchItem[TestPlantA].MaterialETAUtc;
        var oldMaterialExternalNo = _existingPunchItem[TestPlantA].MaterialExternalNo;

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(oldDescription, _existingPunchItem[TestPlantA].Description);
        Assert.AreEqual(oldRaisedByOrgId, _existingPunchItem[TestPlantA].RaisedByOrgId);
        Assert.AreEqual(oldClearingByOrgId, _existingPunchItem[TestPlantA].ClearingByOrgId);
        Assert.AreEqual(oldPriorityId, _existingPunchItem[TestPlantA].PriorityId);
        Assert.AreEqual(oldSortingId, _existingPunchItem[TestPlantA].SortingId);
        Assert.AreEqual(oldTypeId, _existingPunchItem[TestPlantA].TypeId);
        Assert.AreEqual(oldActionById, _existingPunchItem[TestPlantA].ActionById);
        Assert.AreEqual(oldDueTimeUtc, _existingPunchItem[TestPlantA].DueTimeUtc);
        Assert.AreEqual(oldEstimate, _existingPunchItem[TestPlantA].Estimate);
        Assert.AreEqual(oldOriginalWorkOrderId, _existingPunchItem[TestPlantA].OriginalWorkOrderId);
        Assert.AreEqual(oldWorkOrderId, _existingPunchItem[TestPlantA].WorkOrder);
        Assert.AreEqual(oldSWCRId, _existingPunchItem[TestPlantA].SWCRId);
        Assert.AreEqual(oldDocumentId, _existingPunchItem[TestPlantA].DocumentId);
        Assert.AreEqual(oldMaterialExternalNo, _existingPunchItem[TestPlantA].MaterialExternalNo);
        Assert.AreEqual(oldMaterialRequired, _existingPunchItem[TestPlantA].MaterialRequired);
        Assert.AreEqual(oldMaterialETAUtc, _existingPunchItem[TestPlantA].MaterialETAUtc);
        Assert.AreEqual(oldMaterialExternalNo, _existingPunchItem[TestPlantA].MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAuditData_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SetAuditDataAsync();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
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
        Assert.AreEqual(_command.RowVersion, result);
        Assert.AreEqual(_command.RowVersion, _existingPunchItem[TestPlantA].RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotNotPublishAnyEvent_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _messageProducerMock.Received(0)
            .PublishAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }
    #endregion

    #region Unit Tests which can be removed when no longer sync to pcs4
    [TestMethod]
    public async Task HandlingCommand_ShouldSyncWithPcs4_WhenOperationsGiven()
    {
        // Arrange
        PunchItemUpdatedIntegrationEvent integrationEvent = null!;
        _messageProducerMock
            .When(x => x.PublishAsync(Arg.Any<PunchItemUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<PunchItemUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _syncToPCS4ServiceMock.Received(1).SyncPunchListItemUpdateAsync(integrationEvent, default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSyncWithPcs4_WhenNoOperationsGiven()
    {
        // Arrange 
        _command.PatchDocument.Operations.Clear();

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _syncToPCS4ServiceMock.Received(0).SyncPunchListItemUpdateAsync(Arg.Any<object>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSyncWithPcs4_WhenSavingChangesFails()
    {
        // Arrange
        _unitOfWorkMock.When(x => x.SaveChangesAsync())
            .Do(_ => throw new Exception("SaveChangesAsync error"));

        // Act
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.Handle(_command, default);
        });

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive().SyncPunchListItemUpdateAsync(Arg.Any<object>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenSyncingWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncPunchListItemUpdateAsync(Arg.Any<object>(), default))
            .Do(_ => throw new Exception("SyncPunchListItemUpdateAsync error"));

        // Act and Assert
        try
        {
            await _dut.Handle(_command, default);
        }
        catch (Exception ex)
        {
            Assert.Fail("Excepted no exception, but got: " + ex.Message);
        }
    }
    #endregion
}
