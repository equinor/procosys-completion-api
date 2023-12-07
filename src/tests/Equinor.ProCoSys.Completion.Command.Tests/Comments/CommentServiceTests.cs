using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
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

    #region AddAsync
    [TestMethod]
    public async Task AddAsync_ShouldAddCommentToRepository()
    {
        // Arrange 
        var parentType = "Whatever";
        var text = "T";

        // Act
        await _dut.AddAsync(parentType, _parentGuid, text, new List<Label>(), default);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(_parentGuid, _commentAddedToRepository.ParentGuid);
        Assert.AreEqual(parentType, _commentAddedToRepository.ParentType);
        Assert.AreEqual(text, _commentAddedToRepository.Text);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddCommentToRepository_WithoutLabels()
    {
        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", new List<Label>(), default);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(0, _commentAddedToRepository.Labels.Count);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddCommentToRepository_WithLabels()
    {
        // Arrange 
        var labelA = new Label("a");
        var labelB = new Label("b");

        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", new List<Label> { labelA, labelB }, default);

        // Assert
        Assert.AreEqual(2, _commentAddedToRepository.Labels.Count);
        Assert.AreEqual(2, _commentAddedToRepository.GetOrderedNonVoidedLabels().Count());
    }

    [TestMethod]
    public async Task AddAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", new List<Label>(), default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task AddAsync_ShouldNotAddAnyDomainEvent()
    {
        // Act
        await _dut.AddAsync("Whatever", _parentGuid, "T", new List<Label>(), default);

        // Assert
        Assert.AreEqual(0, _commentAddedToRepository.DomainEvents.Count);
    }
    #endregion
}
