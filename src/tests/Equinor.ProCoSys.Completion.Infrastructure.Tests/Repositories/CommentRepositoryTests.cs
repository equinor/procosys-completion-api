using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class CommentRepositoryTests : EntityWithGuidRepositoryTestBase<Comment>
{
    protected override EntityWithGuidRepository<Comment> GetDut() => new CommentRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var comment = new Comment("Whatever", Guid.NewGuid(), "T");
        _knownGuid = comment.Guid;
        comment.SetProtectedIdForTesting(_knownId);
        var comments = new List<Comment> { comment };

        _dbSetMock = comments.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Comments
            .Returns(_dbSetMock);
    }

    protected override Comment GetNewEntity() => new("Whatever", Guid.NewGuid(), "New comment");
}
