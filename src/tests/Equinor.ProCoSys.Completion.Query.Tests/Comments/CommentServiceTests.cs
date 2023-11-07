using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Completion.Query.Comments;

namespace Equinor.ProCoSys.Completion.Query.Tests.Comments;

[TestClass]
public class CommentServiceTests : ReadOnlyTestsBase
{
    private Comment _createdComment;
    private Guid _parentGuid;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        _parentGuid = Guid.NewGuid();
        _createdComment = new Comment("X", _parentGuid, "T");

        context.Comments.Add(_createdComment);
        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task GetAllForParentAsync_ShouldReturnCorrectDtos()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        var dut = new CommentService(context);

        // Act
        var result = await dut.GetAllForParentAsync(_parentGuid, default);

        // Assert
        var commentDtos = result.ToList();
        Assert.AreEqual(1, commentDtos.Count);
        var commentDto = commentDtos.ElementAt(0);
        Assert.AreEqual(_createdComment.ParentGuid, commentDto.ParentGuid);
        Assert.AreEqual(_createdComment.Guid, commentDto.Guid);
        Assert.AreEqual(_createdComment.Text, commentDto.Text);
        var createdBy = commentDto.CreatedBy;
        Assert.IsNotNull(createdBy);
        Assert.AreEqual(CurrentUserOid, createdBy.Guid);
        Assert.AreEqual(_createdComment.CreatedAtUtc, commentDto.CreatedAtUtc);
    }
}
