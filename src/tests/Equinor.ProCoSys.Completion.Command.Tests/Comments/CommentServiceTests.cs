using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Comments;

[TestClass]
public class CommentServiceTests : TestsBase
{
    private readonly TestableEntity _parent = new();
    private ICommentRepository _commentRepository;
    private CommentService _dut;
    private Comment _commentAddedToRepository;
    private ICompletionMailService _completionMailServiceMock;
    private IDeepLinkUtility _deepLinkUtilityMock;

    [TestInitialize]
    public void Setup()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _commentRepository.When(x => x.Add(Arg.Any<Comment>()))
            .Do(info =>
            {
                _commentAddedToRepository = info.Arg<Comment>();
            });

        _completionMailServiceMock = Substitute.For<ICompletionMailService>();
        _deepLinkUtilityMock = Substitute.For<IDeepLinkUtility>();

        _dut = new CommentService(
            _commentRepository, 
            _completionMailServiceMock,
            _deepLinkUtilityMock,
            Substitute.For<ILogger<CommentService>>());
    }

    #region AddAndSaveAsync
    [TestMethod]
    public async Task AddAndSaveAsync_ShouldAddCommentToRepository()
    {
        // Arrange 
        var text = "T";

        // Act
        await _dut.AddAndSaveAsync(_unitOfWorkMock, _parent, text, [], [], "Whatever", default);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(_parent.Guid, _commentAddedToRepository.ParentGuid);
        Assert.AreEqual(_parent.GetContextName(), _commentAddedToRepository.ParentType);
        Assert.AreEqual(text, _commentAddedToRepository.Text);
    }

    [TestMethod]
    public async Task AddAndSaveAsync_ShouldAddCommentToRepository_WithoutLabelsAndMentions()
    {
        // Act
        await _dut.AddAndSaveAsync(_unitOfWorkMock, _parent, "T", [], [], "Whatever", default);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(0, _commentAddedToRepository.Labels.Count);
        Assert.AreEqual(0, _commentAddedToRepository.Mentions.Count);
    }

    [TestMethod]
    public async Task AddAndSaveAsync_ShouldAddCommentToRepository_WithLabels()
    {
        // Arrange 
        var labelA = new Label("a");
        var labelB = new Label("b");

        // Act
        await _dut.AddAndSaveAsync(_unitOfWorkMock, _parent, "T", [labelA, labelB], [], "Whatever", default);

        // Assert
        Assert.AreEqual(2, _commentAddedToRepository.Labels.Count);
        Assert.AreEqual(2, _commentAddedToRepository.GetOrderedNonVoidedLabels().Count());
        Assert.IsTrue(_commentAddedToRepository.Labels.Any(l => l.Text == labelA.Text));
        Assert.IsTrue(_commentAddedToRepository.Labels.Any(l => l.Text == labelB.Text));
    }

    [TestMethod]
    public async Task AddAndSaveAsync_ShouldAddCommentToRepository_WithMentions()
    {
        // Arrange 
        var personA = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);
        var personB = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);

        // Act
        await _dut.AddAndSaveAsync(_unitOfWorkMock, _parent, "T", [], [personB, personA], "Whatever", default);

        // Assert
        Assert.AreEqual(2, _commentAddedToRepository.Mentions.Count);
        Assert.AreEqual(2, _commentAddedToRepository.GetOrderedMentions().Count());
        Assert.IsTrue(_commentAddedToRepository.Mentions.Any(p => p.Guid == personA.Guid));
        Assert.IsTrue(_commentAddedToRepository.Mentions.Any(p => p.Guid == personB.Guid));
    }

    [TestMethod]
    public async Task AddAndSaveAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.AddAndSaveAsync(_unitOfWorkMock, _parent, "T", [], [], "Whatever", default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task AddAndSaveAsync_ShouldSendEmailToCorrectEmails()
    {
        // Arrange
        var person = new Person(Guid.NewGuid(), null!, null!, null!, "p1@pcs.no", false);
        var emailTemplateCode = "MyCode";
        List<string> emailSentTo = null;
        _completionMailServiceMock
            .When(x => x.SendEmailAsync(
                Arg.Any<string>(),
                Arg.Any<dynamic>(),
                Arg.Any<List<string>>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                emailSentTo = callInfo.ArgAt<List<string>>(2);
            });

        // Act
        await _dut.AddAndSaveAsync(_unitOfWorkMock, _parent, "T", [], [person], emailTemplateCode, default);

        // Assert
        await _completionMailServiceMock.Received(1)
            .SendEmailAsync(
                emailTemplateCode,
                Arg.Any<dynamic>(),
                Arg.Any<List<string>>(),
                Arg.Any<CancellationToken>());
        Assert.AreEqual(1, emailSentTo.Count);
        Assert.AreEqual(person.Email, emailSentTo.ElementAt(0));
    }
    #endregion

    #region Add
    [TestMethod]
    public void Add_ShouldAddCommentToRepository()
    {
        // Arrange 
        var text = "T";

        // Act
        _dut.Add(_parent, text, [new Label("L")], []);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(_parent.Guid, _commentAddedToRepository.ParentGuid);
        Assert.AreEqual(_parent.GetContextName(), _commentAddedToRepository.ParentType);
        Assert.AreEqual(text, _commentAddedToRepository.Text);
    }

    [TestMethod]
    public void Add_ShouldAddCommentToRepository_WithLabel()
    {
        // Arrange 
        var labelA = new Label("a");

        // Act
        _dut.Add(_parent, "T", [labelA], []);

        // Assert
        Assert.AreEqual(1, _commentAddedToRepository.Labels.Count);
        Assert.AreEqual(1, _commentAddedToRepository.GetOrderedNonVoidedLabels().Count());
        Assert.IsTrue(_commentAddedToRepository.Labels.Any(l => l.Text == labelA.Text));
    }

    [TestMethod]
    public void AddAsync_ShouldAddCommentToRepository_WithMentions()
    {
        // Arrange 
        var personA = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);
        var personB = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);

        // Act
        _dut.Add(_parent, "T", [], [personB, personA]);

        // Assert
        Assert.AreEqual(2, _commentAddedToRepository.Mentions.Count);
        Assert.AreEqual(2, _commentAddedToRepository.GetOrderedMentions().Count());
        Assert.IsTrue(_commentAddedToRepository.Mentions.Any(p => p.Guid == personA.Guid));
        Assert.IsTrue(_commentAddedToRepository.Mentions.Any(p => p.Guid == personB.Guid));
    }
    #endregion

    private class TestableEntity : IHaveGuid
    {
        public Guid Guid { get; } = Guid.NewGuid();
    }
}
