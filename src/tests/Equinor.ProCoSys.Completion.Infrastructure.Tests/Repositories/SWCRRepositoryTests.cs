using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.SWCRAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class SWCRRepositoryTests : EntityWithGuidRepositoryTestBase<SWCR>
{
    private new SWCRRepository _dut;

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var swcr = new SWCR(TestPlant, Guid.NewGuid(), 1);
        _knownGuid = swcr.Guid;
        swcr.SetProtectedIdForTesting(_knownId);

        var projects = new List<SWCR> { swcr };

        _dbSetMock = projects.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .SWCRs
            .Returns(_dbSetMock);

        _dut = new SWCRRepository(_contextHelper.ContextMock);
        base._dut = _dut;
    }

    protected override SWCR GetNewEntity() => new(TestPlant, Guid.NewGuid(), 2);
}
