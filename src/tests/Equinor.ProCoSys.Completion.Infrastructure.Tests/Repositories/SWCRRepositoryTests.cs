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
    protected override EntityWithGuidRepository<SWCR> GetDut() => new SWCRRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var swcr = new SWCR(TestPlant, Guid.NewGuid(), 1);
        _knownGuid = swcr.Guid;
        swcr.SetProtectedIdForTesting(_knownId);

        var swcrs = new List<SWCR> { swcr };

        _dbSetMock = swcrs.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .SWCRs
            .Returns(_dbSetMock);
    }

    protected override SWCR GetNewEntity() => new(TestPlant, Guid.NewGuid(), 2);
}
