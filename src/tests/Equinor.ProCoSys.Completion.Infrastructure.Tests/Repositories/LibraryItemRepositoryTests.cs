﻿using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.Moq;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class LibraryItemRepositoryTests : EntityWithGuidRepositoryTestBase<LibraryItem>
{
    protected override void SetupRepositoryWithOneKnownItem()
    {
        _knownGuid = Guid.NewGuid();
        var libraryItem = new LibraryItem(TestPlant, _knownGuid, "A", "A Desc", LibraryType.COMPLETION_ORGANIZATION);
        libraryItem.SetProtectedIdForTesting(_knownId);
        var library = new List<LibraryItem> { libraryItem };

        _dbSetMock = library.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Setup(x => x.Library)
            .Returns(_dbSetMock.Object);

        _dut = new LibraryItemRepository(_contextHelper.ContextMock.Object);
    }

    protected override LibraryItem GetNewEntity() => new(TestPlant, Guid.NewGuid(), "B", "B Desc", LibraryType.COMPLETION_ORGANIZATION);
}