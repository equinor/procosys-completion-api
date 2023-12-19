using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class LinkRepositoryTests : EntityWithGuidRepositoryTestBase<Link>
{
    protected override EntityWithGuidRepository<Link> GetDut() => new LinkRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var link = new Link("Whatever", Guid.NewGuid(), "T", "www");
        _knownGuid = link.Guid;
        link.SetProtectedIdForTesting(_knownId);
        var links = new List<Link> { link };

        _dbSetMock = links.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Links
            .Returns(_dbSetMock);
    }

    protected override Link GetNewEntity() => new("Whatever", Guid.NewGuid(), "New link", "U");
}
