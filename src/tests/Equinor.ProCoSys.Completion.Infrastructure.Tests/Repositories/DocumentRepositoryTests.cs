using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class DocumentRepositoryTests : EntityWithGuidRepositoryTestBase<Document>
{
    protected override EntityWithGuidRepository<Document> GetDut() => new DocumentRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var document = new Document(TestPlant, Guid.NewGuid(), "0001");
        _knownGuid = document.Guid;
        document.SetProtectedIdForTesting(_knownId);

        var documents = new List<Document> { document };

        _dbSetMock = documents.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Documents
            .Returns(_dbSetMock);
    }

    protected override Document GetNewEntity() => new(TestPlant, Guid.NewGuid(), "0002");
}
