using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.WorkOrderAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class WorkOrderRepositoryTests : EntityWithGuidRepositoryTestBase<WorkOrder>
{
    private new WorkOrderRepository _dut;

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var workOrder = new WorkOrder(TestPlant, Guid.NewGuid(), "0001");
        _knownGuid = workOrder.Guid;
        workOrder.SetProtectedIdForTesting(_knownId);

        var projects = new List<WorkOrder> { workOrder };

        _dbSetMock = projects.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .WorkOrders
            .Returns(_dbSetMock);

        _dut = new WorkOrderRepository(_contextHelper.ContextMock);
        base._dut = _dut;
    }

    protected override WorkOrder GetNewEntity() => new(TestPlant, Guid.NewGuid(), "0002");
}
