using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class HistoryItemRepositoryTests : EntityWithGuidRepositoryTestBase<HistoryItem>
{
    protected override EntityWithGuidRepository<HistoryItem> GetDut() => new HistoryItemRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var historyItem = new HistoryItem(Guid.NewGuid(), "D", Guid.NewGuid(), "FN", DateTime.UtcNow);
        _knownGuid = historyItem.Guid;
        historyItem.SetProtectedIdForTesting(_knownId);

        var historyItems = new List<HistoryItem> { historyItem };

        _dbSetMock = historyItems.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .History
            .Returns(_dbSetMock);
    }

    protected override HistoryItem GetNewEntity() => new(Guid.NewGuid(), "D", Guid.NewGuid(), "FN", DateTime.UtcNow);
}
