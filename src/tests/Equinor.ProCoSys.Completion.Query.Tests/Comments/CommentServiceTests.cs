using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Completion.Query.Comments;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.Completion.Query.Tests.Comments;

[TestClass]
public class CommentServiceTests : ReadOnlyTestsBase
{
    private Comment _createdComment;
    private Guid _parentGuid;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);

        Add4UnorderedLabelsInclusiveAVoidedLabel(context);

        var labelA = context.Labels.Single(l => l.Text == LabelTextA);
        var labelB = context.Labels.Single(l => l.Text == LabelTextB);
        var labelC = context.Labels.Single(l => l.Text == LabelTextC);
        var voidedLabel = context.Labels.Single(l => l.Text == LabelTextVoided);

        var personA = context.Persons.Single(p => p.Guid == PersonAOid);
        var personB = context.Persons.Single(p => p.Guid == PersonBOid);

        _parentGuid = Guid.NewGuid();
        _createdComment = new Comment("X", _parentGuid, "T");
        // insert mentions non-ordered to test ordering
        _createdComment.SetMentions(new List<Person> { personB, personA });
        // insert labels non-ordered to test ordering
        _createdComment.UpdateLabels(new List<Label> { labelB, voidedLabel, labelC, labelA });

        context.Comments.Add(_createdComment);
        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task GetAllForParentAsync_ShouldReturnCorrectDtos()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock);
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

        AssertOrderedNonVoidedLabels(_createdComment, commentDto);
        AssertOrderedMentions(_createdComment, commentDto);
    }

    private void AssertOrderedMentions(Comment comment, CommentDto commentDto)
    {
        Assert.IsNotNull(commentDto.Mentions);
        var expectedOrderedMentions =
            comment.Mentions
                .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                .ToList();
        Assert.AreEqual(expectedOrderedMentions.Count, commentDto.Mentions.Count);
        for (var i = 0; i < expectedOrderedMentions.Count; i++)
        {
            var expectedPerson = expectedOrderedMentions.ElementAt(i);
            var personDto = commentDto.Mentions.ElementAt(i);
            Assert.AreEqual(expectedPerson.Guid, personDto.Guid);
            Assert.AreEqual(expectedPerson.FirstName, personDto.FirstName);
            Assert.AreEqual(expectedPerson.LastName, personDto.LastName);
            Assert.AreEqual(expectedPerson.UserName, personDto.UserName);
            Assert.AreEqual(expectedPerson.Email, personDto.Email);
        }
    }

    private static void AssertOrderedNonVoidedLabels(Comment comment, CommentDto commentDto)
    {
        Assert.IsNotNull(commentDto.Labels);
        var expectedOrderedNonVoidedLabels =
            comment.Labels
                .Where(l => !l.IsVoided)
                .OrderBy(l => l.Text)
                .ToList();
        Assert.AreEqual(expectedOrderedNonVoidedLabels.Count, commentDto.Labels.Count);
        for (var i = 0; i < expectedOrderedNonVoidedLabels.Count; i++)
        {
            var expectedLabel = expectedOrderedNonVoidedLabels.ElementAt(i);
            var label = commentDto.Labels.ElementAt(i);
            Assert.AreEqual(expectedLabel.Text, label);
        }
    }
}
