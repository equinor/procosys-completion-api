using System;
using System.Collections.Generic;
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
        _dut = new CommentService(_commentRepository, _unitOfWorkMock, logger);
    }

    #region AddAsync - with list of labels
    [TestMethod]
    public async Task AddAsync_ShouldAddCommentToRepository()
    {
        // Arrange 
        var parentType = "Whatever";
        var text = "T";

        // Act
        await _dut.AddAsync(parentType, _parentGuid, text, new List<Label>(), new List<Person>(), default);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(_parentGuid, _commentAddedToRepository.ParentGuid);
        Assert.AreEqual(parentType, _commentAddedToRepository.ParentType);
        Assert.AreEqual(text, _commentAddedToRepository.Text);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddCommentToRepository_WithoutLabelsAndMentions()
    {
        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", new List<Label>(), new List<Person>(), default);

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
        await _dut.AddAsync("Whatever", _parentGuid, "T", new List<Label> { labelA, labelB }, new List<Person>(), default);

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
        await _dut.AddAsync("Whatever", _parentGuid, "T", new List<Label>(), new List<Person> { personB, personA }, default);

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
        await _dut.AddAsync("Whatever", _parentGuid, "T", new List<Label>(), new List<Person>(), default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task AddAsync_ShouldNotAddAnyDomainEvent()
    {
        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", new List<Label>(), new List<Person>(), default);

        // Assert
        Assert.AreEqual(0, _commentAddedToRepository.DomainEvents.Count);
    }
    #endregion

    #region AddAsync - with single label
    [TestMethod]
    public async Task AddAsync_SingleLabel_ShouldAddCommentToRepository()
    {
        // Arrange 
        var parentType = "Whatever";
        var text = "T";

        // Act
        await _dut.AddAsync(parentType, _parentGuid, text, new Label("L"), new List<Person>(), default);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(_parentGuid, _commentAddedToRepository.ParentGuid);
        Assert.AreEqual(parentType, _commentAddedToRepository.ParentType);
        Assert.AreEqual(text, _commentAddedToRepository.Text);
    }

    [TestMethod]
    public async Task AddAsync_SingleLabel_ShouldAddCommentToRepository_WithLabel()
    {
        // Arrange 
        var labelA = new Label("a");

        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", labelA, new List<Person>(), default);

        // Assert
        Assert.AreEqual(1, _commentAddedToRepository.Labels.Count);
        Assert.AreEqual(1, _commentAddedToRepository.GetOrderedNonVoidedLabels().Count());
        Assert.IsTrue(_commentAddedToRepository.Labels.Any(l => l.Text == labelA.Text));
    }

    [TestMethod]
    public async Task AddAsync_SingleLabel_ShouldAddCommentToRepository_WithMentions()
    {
        // Arrange 
        var personA = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);
        var personB = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);

        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", new Label("L"), new List<Person> { personB, personA }, default);

        // Assert
        Assert.AreEqual(2, _commentAddedToRepository.Mentions.Count);
        Assert.AreEqual(2, _commentAddedToRepository.GetOrderedMentions().Count());
        Assert.IsTrue(_commentAddedToRepository.Mentions.Any(p => p.Guid == personA.Guid));
        Assert.IsTrue(_commentAddedToRepository.Mentions.Any(p => p.Guid == personB.Guid));
    }

    [TestMethod]
    public async Task AddAsync_SingleLabel_ShouldSaveOnce()
    {
        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", new Label("L"), new List<Person>(), default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task AddAsync_SingleLabel_ShouldNotAddAnyDomainEvent()
    {
        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", new Label("L"), new List<Person>(), default);

        // Assert
        Assert.AreEqual(0, _commentAddedToRepository.DomainEvents.Count);
    }
    #endregion
}
