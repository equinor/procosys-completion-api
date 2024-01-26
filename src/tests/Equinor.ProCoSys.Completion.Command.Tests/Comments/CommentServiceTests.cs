using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Comments;
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
    private readonly Guid _parentGuid = Guid.NewGuid();
    private ICommentRepository _commentRepository;
    private CommentService _dut;
    private Comment _commentAddedToRepository;

    [TestInitialize]
    public void Setup()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _commentRepository.When(x => x.Add(Arg.Any<Comment>()))
            .Do(info =>
            {
                _commentAddedToRepository = info.Arg<Comment>();
            });

        var logger = Substitute.For<ILogger<CommentService>>();
        _dut = new CommentService(_commentRepository, logger);
    }

    #region AddAndSaveAsync
    [TestMethod]
    public async Task AddAndSaveAsync_ShouldAddCommentToRepository()
    {
        // Arrange 
        var parentType = "Whatever";
        var text = "T";

        // Act
        await _dut.AddAndSaveAsync(_unitOfWorkMock, parentType, _parentGuid, text, [], [], default);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(_parentGuid, _commentAddedToRepository.ParentGuid);
        Assert.AreEqual(parentType, _commentAddedToRepository.ParentType);
        Assert.AreEqual(text, _commentAddedToRepository.Text);
    }

    [TestMethod]
    public async Task AddAndSaveAsync_ShouldAddCommentToRepository_WithoutLabelsAndMentions()
    {
        // Act
        await _dut.AddAndSaveAsync(_unitOfWorkMock, "Whatever", _parentGuid, "T", [], [], default);

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
        await _dut.AddAndSaveAsync(_unitOfWorkMock, "Whatever", _parentGuid, "T", [labelA, labelB], [], default);

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
        await _dut.AddAndSaveAsync(_unitOfWorkMock, "Whatever", _parentGuid, "T", [], [personB, personA], default);

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
        await _dut.AddAndSaveAsync(_unitOfWorkMock, "Whatever", _parentGuid, "T", [], [], default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
    #endregion

    #region Add
    [TestMethod]
    public void Add_ShouldAddCommentToRepository()
    {
        // Arrange 
        var parentType = "Whatever";
        var text = "T";

        // Act
        _dut.Add(parentType, _parentGuid, text, [new Label("L")], []);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(_parentGuid, _commentAddedToRepository.ParentGuid);
        Assert.AreEqual(parentType, _commentAddedToRepository.ParentType);
        Assert.AreEqual(text, _commentAddedToRepository.Text);
    }

    [TestMethod]
    public void Add_ShouldAddCommentToRepository_WithLabel()
    {
        // Arrange 
        var labelA = new Label("a");

        // Act
        _dut.Add("Whatever", _parentGuid, "T", [labelA], []);

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
        _dut.Add("Whatever", _parentGuid, "T", [], [personB, personA]);

        // Assert
        Assert.AreEqual(2, _commentAddedToRepository.Mentions.Count);
        Assert.AreEqual(2, _commentAddedToRepository.GetOrderedMentions().Count());
        Assert.IsTrue(_commentAddedToRepository.Mentions.Any(p => p.Guid == personA.Guid));
        Assert.IsTrue(_commentAddedToRepository.Mentions.Any(p => p.Guid == personB.Guid));
    }
    #endregion
}
