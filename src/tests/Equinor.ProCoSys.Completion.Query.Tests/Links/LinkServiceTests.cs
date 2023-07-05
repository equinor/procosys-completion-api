using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Completion.Query.Links;

namespace Equinor.ProCoSys.Completion.Query.Tests.Links;

[TestClass]
public class LinkServiceTests : ReadOnlyTestsBase
{
    private Link _link;
    private Guid _sourceGuid;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);

        _sourceGuid = Guid.NewGuid();
        _link = new Link("X", _sourceGuid, "T", "U");

        context.Links.Add(_link);
        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task GetAllForSourceAsync_ShouldReturnCorrectDtos()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
        var dut = new LinkService(context);

        // Act
        var result = await dut.GetAllForSourceAsync(_sourceGuid, default);

        // Assert
        var linkDtos = result.ToList();
        Assert.AreEqual(1, linkDtos.Count);
        var linkDto = linkDtos.ElementAt(0);
        Assert.AreEqual(_link.SourceGuid, linkDto.SourceGuid);
        Assert.AreEqual(_link.Guid, linkDto.Guid);
        Assert.AreEqual(_link.Title, linkDto.Title);
        Assert.AreEqual(_link.Url, linkDto.Url);
    }
}
