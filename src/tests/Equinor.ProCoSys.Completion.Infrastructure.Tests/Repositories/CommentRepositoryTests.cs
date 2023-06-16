using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.Moq;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class CommentRepositoryTests : EntityWithGuidRepositoryTestBase<Comment>
{
    protected override void SetupRepositoryWithOneKnownItem()
    {
        var comment = new Comment("Whatever", Guid.NewGuid(), "T");
        _knownGuid = comment.Guid;
        comment.SetProtectedIdForTesting(_knownId);
        var comments = new List<Comment> { comment };

        _dbSetMock = comments.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Setup(x => x.Comments)
            .Returns(_dbSetMock.Object);

        _dut = new CommentRepository(_contextHelper.ContextMock.Object);
    }

    protected override Comment GetNewEntity() => new("Whatever", Guid.NewGuid(), "New comment");
}
