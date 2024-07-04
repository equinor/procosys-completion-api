using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.IntegrationEvents.PunchItemEvents;

public class PunchItemIntegrationEventTestsBase : TestsBase
{
    protected PunchItem _punchItem;
    protected Project _project;

    [TestInitialize]
    public void SetupBase()
    {
        _project = new Project(TestPlantA, Guid.NewGuid(), null!, null!);
        var raisedByOrg = new LibraryItem(TestPlantA, Guid.NewGuid(), "RC", "RD", LibraryType.COMPLETION_ORGANIZATION);
        var clearingByOrg = new LibraryItem(TestPlantA, Guid.NewGuid(), "CC", "CD", LibraryType.COMPLETION_ORGANIZATION);
        _punchItem = new PunchItem(TestPlantA, _project, Guid.NewGuid(), Category.PB, "Desc", raisedByOrg, clearingByOrg);
        _punchItem.SetCreated(_person);
    }

    protected void FillOptionalProperties(PunchItem punchItem)
    {
        punchItem.SetSorting(new LibraryItem(TestPlantA, Guid.NewGuid(), "SortC", "SortD", LibraryType.PUNCHLIST_SORTING));
        punchItem.SetPriority(new LibraryItem(TestPlantA, Guid.NewGuid(), "PriC", "PriD", LibraryType.COMM_PRIORITY));
        punchItem.SetType(new LibraryItem(TestPlantA, Guid.NewGuid(), "TypC", "TypD", LibraryType.PUNCHLIST_TYPE));
        punchItem.DueTimeUtc = DateTime.UtcNow;
        punchItem.Estimate = 100;
        punchItem.ExternalItemNo = "100";
        punchItem.MaterialRequired = true;
        punchItem.MaterialETAUtc = DateTime.UtcNow.AddDays(2);
        punchItem.MaterialExternalNo = "120";
        punchItem.SetWorkOrder(new WorkOrder(TestPlantA, Guid.NewGuid(), "WO1"));
        punchItem.SetOriginalWorkOrder(new WorkOrder(TestPlantA, Guid.NewGuid(), "WO2"));
        punchItem.SetDocument(new Document(TestPlantA, Guid.NewGuid(), "Doc"));
        punchItem.SetSWCR(new SWCR(TestPlantA, Guid.NewGuid(), 7));
        punchItem.SetActionBy(new Person(Guid.NewGuid(), null!, null!, null!, null!, false));
    }
}
