using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Comments;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.CommentDomainEvents;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Comments;

[TestClass]
public class CommentServiceTests : TestsBase
{
    private readonly Guid _sourceGuid = Guid.NewGuid();
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
        var sourceType = "Whatever";
        var text = "T";

        // Act
        await _dut.AddAsync(sourceType, _sourceGuid, text, default);

        // Assert
        Assert.IsNotNull(_commentAddedToRepository);
        Assert.AreEqual(_sourceGuid, _commentAddedToRepository.SourceGuid);
        Assert.AreEqual(sourceType, _commentAddedToRepository.SourceType);
        Assert.AreEqual(text, _commentAddedToRepository.Text);
    }

    [TestMethod]
    public async Task AddAsync_ShouldSaveOnce()
    {
        // Act
        await _dut.AddAsync("Whatever", _sourceGuid, "T", default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }

    [TestMethod]
    public async Task AddAsync_ShouldAddCommentCreatedEvent()
    {
        // Act
        await _dut.AddAsync("Whatever", _sourceGuid, "T", default);

        // Assert
        Assert.AreEqual(0, _commentAddedToRepository.DomainEvents.Count);
    }
    #endregion
}
