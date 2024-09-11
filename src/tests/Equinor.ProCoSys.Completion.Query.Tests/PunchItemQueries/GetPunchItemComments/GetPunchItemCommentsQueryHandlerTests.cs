using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemComments;
using Equinor.ProCoSys.Completion.Query.Comments;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemComments;

[TestClass]
public class GetPunchItemCommentsQueryHandlerTests : TestsBase
{
    private GetPunchItemCommentsQueryHandler _dut;
    private ICommentService _commentServiceMock;
    private GetPunchItemCommentsQuery _query;
    private CommentDto _commentDto;

    [TestInitialize]
    public void Setup()
    {
        _query = new GetPunchItemCommentsQuery(Guid.NewGuid());

        _commentDto = new CommentDto(
            _query.PunchItemGuid,
            Guid.NewGuid(),
            "T",
            new List<string> { "A" },
            new List<PersonDto>
            {
                new(Guid.NewGuid(), "A", "Aa", "aa", "AaEmail"),
                new(Guid.NewGuid(), "B", "Bb", "bb", "BbEmail")
            },
            new PersonDto(Guid.NewGuid(), "First", "Last", "UN", "Email"),
            new DateTime(2023, 6, 11, 1, 2, 3));
        var commentDtos = new List<CommentDto>
        {
            _commentDto
        };
        _commentServiceMock = Substitute.For<ICommentService>();
        _commentServiceMock.GetAllForParentAsync(_query.PunchItemGuid, default)
            .Returns(commentDtos);

        _dut = new GetPunchItemCommentsQueryHandler(_commentServiceMock);
    }

    [TestMethod]
    public async Task HandlingQuery_ShouldReturn_Comments()
    {
        // Act
        var result = await _dut.Handle(_query, default);

        // Assert
        Assert.IsInstanceOfType(result, typeof(IEnumerable<CommentDto>));
        var comment = result.Single();
        Assert.AreEqual(_commentDto.ParentGuid, comment.ParentGuid);
        Assert.AreEqual(_commentDto.Guid, comment.Guid);
        Assert.AreEqual(_commentDto.Text, comment.Text);
        Assert.AreEqual(_commentDto.Labels.Count, comment.Labels.Count);
        foreach (var labelText in _commentDto.Labels)
        {
            Assert.IsTrue(comment.Labels.Any(l => l == labelText));
        }
        Assert.AreEqual(_commentDto.CreatedAtUtc, comment.CreatedAtUtc);
        AssertPerson(_commentDto.CreatedBy, comment.CreatedBy);

        Assert.AreEqual(_commentDto.Mentions.Count, comment.Mentions.Count);
        foreach (var mention in _commentDto.Mentions)
        {
            AssertPerson(mention, comment.Mentions.Single(p => p.Guid == mention.Guid));
        }
    }

    [TestMethod]
    public async Task HandlingQuery_Should_CallGetAllForParent_OnCommentService()
    {
        // Act
        await _dut.Handle(_query, default);

        // Assert
        await _commentServiceMock.Received(1).GetAllForParentAsync(
            _query.PunchItemGuid,
            default);
    }

    private static void AssertPerson(PersonDto expected, PersonDto actual)
    {
        Assert.AreEqual(expected.Guid, actual.Guid);
        Assert.AreEqual(expected.FirstName, actual.FirstName);
        Assert.AreEqual(expected.LastName, actual.LastName);
        Assert.AreEqual(expected.UserName, actual.UserName);
        Assert.AreEqual(expected.Email, actual.Email);
    }
}
