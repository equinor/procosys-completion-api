using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.CommentEvents;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Comments;

[TestClass]
public class CommentServiceTests : TestsBase
{
    private readonly TestableEntity _parent = new();
    private readonly string _testPlant = TestPlantA;
    private ICommentRepository _commentRepository;
    private CommentService _dut;
    private Comment _commentAddedToRepository;
    private ICompletionMailService _completionMailServiceMock;
    private IDeepLinkUtility _deepLinkUtilityMock;
    private IMessageProducer _messageProducerMock;

    [TestInitialize]
    public void Setup()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _commentRepository.When(x => x.Add(Arg.Any<Comment>()))
            .Do(info =>
            {
                _commentAddedToRepository = info.Arg<Comment>();
                _commentAddedToRepository.SetCreated(_person);
            });

        _completionMailServiceMock = Substitute.For<ICompletionMailService>();
        _deepLinkUtilityMock = Substitute.For<IDeepLinkUtility>();
        _messageProducerMock = Substitute.For<IMessageProducer>();

        _dut = new CommentService(
            _commentRepository, 
            _completionMailServiceMock,
            _deepLinkUtilityMock,
            _syncToPCS4ServiceMock,
            _messageProducerMock,
            Substitute.For<ILogger<CommentService>>());
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddCommentToRepository()
    {
        // Arrange 
        var text = "T";

        // Act
        await _dut.AddAsync(_unitOfWorkMock, _parent, _testPlant, text, [], [], "Whatever", default);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(_parent.Guid, _commentAddedToRepository.ParentGuid);
        Assert.AreEqual(_parent.GetContextName(), _commentAddedToRepository.ParentType);
        Assert.AreEqual(text, _commentAddedToRepository.Text);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddCommentToRepository_WithoutLabelsAndMentions()
    {
        // Act
        await _dut.AddAsync(_unitOfWorkMock, _parent, _testPlant, "T", [], [], "Whatever", default);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(0, _commentAddedToRepository.Labels.Count);
        Assert.AreEqual(0, _commentAddedToRepository.Mentions.Count);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddCommentToRepository_WithLabels()
    {
        // Arrange 
        var labelA = new Label("a");
        var labelB = new Label("b");

        // Act
        await _dut.AddAsync(_unitOfWorkMock, _parent, _testPlant, "T", [labelA, labelB], [], "Whatever", default);

        // Assert
        Assert.AreEqual(2, _commentAddedToRepository.Labels.Count);
        Assert.AreEqual(2, _commentAddedToRepository.GetOrderedNonVoidedLabels().Count());
        Assert.IsTrue(_commentAddedToRepository.Labels.Any(l => l.Text == labelA.Text));
        Assert.IsTrue(_commentAddedToRepository.Labels.Any(l => l.Text == labelB.Text));
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddCommentToRepository_WithMentions()
    {
        // Arrange 
        var personA = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);
        var personB = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);

        // Act
        await _dut.AddAsync(_unitOfWorkMock, _parent, _testPlant, "T", [], [personB, personA], "Whatever", default);

        // Assert
        Assert.AreEqual(2, _commentAddedToRepository.Mentions.Count);
        Assert.AreEqual(2, _commentAddedToRepository.GetOrderedMentions().Count());
        Assert.IsTrue(_commentAddedToRepository.Mentions.Any(p => p.Guid == personA.Guid));
        Assert.IsTrue(_commentAddedToRepository.Mentions.Any(p => p.Guid == personB.Guid));
    }

    [TestMethod]
    public async Task AddAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.AddAsync(_unitOfWorkMock, _parent, _testPlant, "T", [], [], "Whatever", default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task AddAsync_ShouldSendEmailEventToCorrectEmails()
    {
        // Arrange
        var person = new Person(Guid.NewGuid(), null!, null!, null!, "p1@pcs.no", false);
        var emailTemplateCode = "MyCode";
        List<string> emailSentTo = null;
        _completionMailServiceMock
            .When(x => x.SendEmailEventAsync(
                Arg.Any<string>(),
                Arg.Any<dynamic>(),
                Arg.Any<List<string>>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                emailSentTo = callInfo.ArgAt<List<string>>(2);
            });

        // Act
        await _dut.AddAsync(_unitOfWorkMock, _parent, _testPlant, "T", [], [person], emailTemplateCode, default);

        // Assert
        await _completionMailServiceMock.Received(1)
            .SendEmailEventAsync(
                emailTemplateCode,
                Arg.Any<dynamic>(),
                Arg.Any<List<string>>(),
                Arg.Any<CancellationToken>());
        Assert.AreEqual(1, emailSentTo.Count);
        Assert.AreEqual(person.Email, emailSentTo.ElementAt(0));
    }

    
    #region Unit Tests which can be removed when no longer sync to pcs4

    [TestMethod]
    public async Task AddAsync_ShouldSyncWithPcs4()
    {
        // Act
        await _dut.AddAsync(_unitOfWorkMock, _parent, _testPlant, "text", [], [], "Whatever", default);

        // Assert
        await _syncToPCS4ServiceMock.Received(1).SyncNewCommentAsync(Arg.Any<CommentCreatedIntegrationEvent>(), default);
    }

    [TestMethod]
    public async Task AddAsync_ShouldNotSyncWithPcs4_WhenSavingChangesFails()
    {
        // Arrange
        _unitOfWorkMock.When(x => x.SaveChangesAsync())
           .Do(_ => throw new Exception("SaveChangesAsync error"));

        // Act
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.AddAsync(_unitOfWorkMock, _parent, _testPlant, "text", [], [], "Whatever", default);
        });

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive().SyncNewCommentAsync(Arg.Any<CommentCreatedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenSyncingWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncNewCommentAsync(Arg.Any<object>(), default))
            .Do(_ => throw new Exception("SyncNewCommentAsync error"));

        // Act and Assert
        try
        {
            await _dut.AddAsync(_unitOfWorkMock, _parent, _testPlant, "text", [], [], "Whatever", default);
        }
        catch (Exception ex)
        {
            Assert.Fail("Excepted no exception, but got: " + ex.Message);
        }
    }
    #endregion

    private class TestableEntity : IHaveGuid
    {
        public Guid Guid { get; } = Guid.NewGuid();
    }
}
