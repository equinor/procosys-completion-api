using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class PunchItemCommentEventConsumerTests
{
    private readonly Guid _createdByGuid = Guid.NewGuid();
    private readonly ICommentRepository _commentRepositoryMock = Substitute.For<ICommentRepository>();
    private readonly IPersonRepository _personRepoMock = Substitute.For<IPersonRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private PunchItemCommentEventConsumer _dut = null!;
    private readonly ConsumeContext<PunchItemCommentEvent> _contextMock = Substitute.For<ConsumeContext<PunchItemCommentEvent>>();
    private Comment? _commentAddedToRepository;
    private PunchItemCommentEvent _bEvent = null!;

    [TestInitialize]
    public void Setup()
    {
        _dut = new PunchItemCommentEventConsumer(
            Substitute.For<ILogger<PunchItemCommentEventConsumer>>(),
            _personRepoMock,
            _commentRepositoryMock,
            _unitOfWorkMock);

        _commentRepositoryMock
            .When(x => x.Add(Arg.Any<Comment>()))
            .Do(callInfo =>
            {
                _commentAddedToRepository = callInfo.Arg<Comment>();
            });

        _personRepoMock.GetAsync(_createdByGuid, default)
            .Returns(new Person(_createdByGuid, "fn", "ln", "un", "@", false));

        _bEvent = new PunchItemCommentEvent(Guid.NewGuid(), Guid.NewGuid(), "comment", _createdByGuid, DateTime.UtcNow);
        _contextMock.Message.Returns(_bEvent);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewComment_WhenCommentDoesNotExist()
    {
        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(_bEvent.CommentGuid, _commentAddedToRepository.Guid);
        Assert.AreEqual(_bEvent.PunchItemGuid, _commentAddedToRepository.ParentGuid);
        Assert.AreEqual(_bEvent.Comment, _commentAddedToRepository.Text);
        Assert.AreEqual(_createdByGuid, _commentAddedToRepository.CreatedBy.Guid);
        Assert.AreEqual(_bEvent.CreatedAt, _commentAddedToRepository.CreatedAtUtc);
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldNotAddNewComment_WhenCommentExists()
    {
        //Arrange
        _commentRepositoryMock.ExistsAsync(_bEvent.CommentGuid, _contextMock.CancellationToken).Returns(true);
        
        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNull(_commentAddedToRepository);
        await _unitOfWorkMock.Received(0).SaveChangesFromSyncAsync();
    }
}
