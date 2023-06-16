using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchComments;
using Equinor.ProCoSys.Completion.Query.Comments;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Query.Tests.GetPunchComments;

[TestClass]
public class GetPunchCommentsQueryHandlerTests : TestsBase
{
    private GetPunchCommentsQueryHandler _dut;
    private Mock<ICommentService> _commentServiceMock;
    private GetPunchCommentsQuery _query;
    private CommentDto _commentDto;

    [TestInitialize]
    public void Setup()
    {
        _query = new GetPunchCommentsQuery(Guid.NewGuid());

        _commentDto = new CommentDto(
            _query.PunchGuid,
            Guid.NewGuid(), 
            "T", 
            new PersonDto(Guid.NewGuid(), "First", "Last", "UN", "Email"),
            new DateTime(2023, 6, 11, 1, 2, 3));
        var commentDtos = new List<CommentDto>
        {
            _commentDto
        };
        _commentServiceMock = new Mock<ICommentService>();
        _commentServiceMock.Setup(l => l.GetAllForSourceAsync(_query.PunchGuid, default))
            .ReturnsAsync(commentDtos);

        _dut = new GetPunchCommentsQueryHandler(_commentServiceMock.Object);
    }

    [TestMethod]
    public async Task HandlingQuery_ShouldReturn_Comments()
    {
        // Act
        var result = await _dut.Handle(_query, default);

        // Assert
        Assert.IsInstanceOfType(result.Data, typeof(IEnumerable<CommentDto>));
        var comment = result.Data.Single();
        Assert.AreEqual(_commentDto.SourceGuid, comment.SourceGuid);
        Assert.AreEqual(_commentDto.Guid, comment.Guid);
        Assert.AreEqual(_commentDto.CreatedAtUtc, comment.CreatedAtUtc);
        var createdBy = comment.CreatedBy;
        Assert.IsNotNull(createdBy);
        Assert.AreEqual(_commentDto.CreatedBy.Guid, createdBy.Guid);
        Assert.AreEqual(_commentDto.CreatedBy.FirstName, createdBy.FirstName);
        Assert.AreEqual(_commentDto.CreatedBy.LastName, createdBy.LastName);
        Assert.AreEqual(_commentDto.CreatedBy.UserName, createdBy.UserName);
        Assert.AreEqual(_commentDto.CreatedBy.Email, createdBy.Email);
        Assert.AreEqual(_commentDto.CreatedAtUtc, comment.CreatedAtUtc);
    }

    [TestMethod]
    public async Task HandlingQuery_Should_CallGetAllForSource_OnCommentService()
    {
        // Act
        await _dut.Handle(_query, default);

        // Assert
        _commentServiceMock.Verify(u => u.GetAllForSourceAsync(
            _query.PunchGuid,
            default), Times.Exactly(1));
    }
}
