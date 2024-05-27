using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests;

[TestClass]
public class UnitOfWorkTests
{
    private readonly string _plant = "PCS$TESTPLANT";
    private readonly Guid _currentUserOid = new("12345678-1234-1234-1234-123456789123");
    private readonly DateTime _currentTime = new(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    private DbContextOptions<CompletionContext> _dbContextOptions;
    private IPlantProvider _plantProviderMock;
    private IEventDispatcher _eventDispatcherMock;
    private ICurrentUserProvider _currentUserProviderMock;
    private TokenCredential _tokenCredentialsMock;
    private ManualTimeProvider _timeProvider;
    private Person _currentUser;

    [TestInitialize]
    public async Task Setup()
    {
        _dbContextOptions = new DbContextOptionsBuilder<CompletionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _plantProviderMock = Substitute.For<IPlantProvider>();
        _plantProviderMock.Plant
            .Returns(_plant);

        _eventDispatcherMock = Substitute.For<IEventDispatcher>();

        _currentUserProviderMock = Substitute.For<ICurrentUserProvider>();

        var fakeToken = new AccessToken("FakeToken", DateTimeOffset.MaxValue);
        _tokenCredentialsMock = Substitute.For<TokenCredential>();
        _tokenCredentialsMock.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
                   .Returns(fakeToken);

        _timeProvider = new ManualTimeProvider(_currentTime);
        TimeService.SetProvider(_timeProvider);

        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        _currentUser = new Person(_currentUserOid, "Current", "User", "cu", "cu@pcs.pcs", false);
        dut.Persons.Add(_currentUser);
        await dut.SaveChangesAsync();

        _currentUserProviderMock.GetCurrentUserOid()
            .Returns(_currentUserOid);
    }

    [TestMethod]
    public async Task SetAuditDataAsync_ShouldSetCreatedProperties_WhenCreated()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var attachment = new Attachment("Project", "Type", Guid.NewGuid(), "TestFile.docx");
        dut.Attachments.Add(attachment);

        // Act
        await dut.SetAuditDataAsync();

        // Assert
        AssertCreated(attachment);
        AssertNotModified(attachment);
    }

    [TestMethod]
    public async Task SaveChangesAsync_ShouldSetCreatedProperties_WhenCreated()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var libraryItem = new Attachment("Project", "Type", Guid.NewGuid(), "TestFile.docx");
        dut.Attachments.Add(libraryItem);

        // Act
        await dut.SaveChangesAsync();

        // Assert
        AssertCreated(libraryItem);
        AssertNotModified(libraryItem);
    }

    [TestMethod]
    public async Task SetAuditDataAsync_ShouldSetModifiedProperties_WhenModified()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var attachment = new Attachment("Project", "Type", Guid.NewGuid(), "TestFile.docx");
        dut.Attachments.Add(attachment);

        await dut.SaveChangesAsync();

        // trigger a change on record. EF change tracker notice this
        attachment.Description = "new description";

        // Act
        await dut.SetAuditDataAsync();

        // Assert
        AssertCreated(attachment);
        AssertModified(attachment);
    }

    [TestMethod]
    public async Task SaveChangesAsync_ShouldSetModifiedProperties_WhenModified()
    {
        // Arrange
        await using var dut = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var attachment = new Attachment("Project", "Type", Guid.NewGuid(), "TestFile.docx");
        dut.Attachments.Add(attachment);

        await dut.SaveChangesAsync();

        // trigger a change on record. EF change tracker notice this
        attachment.Description = "new description";

        // Act
        await dut.SaveChangesAsync();

        // Assert
        AssertCreated(attachment);
        AssertModified(attachment);
    }

    private void AssertModified(IModificationAuditable auditable)
    {
        Assert.AreEqual(_currentTime, auditable.ModifiedAtUtc);
        Assert.AreEqual(_currentUser.Id, auditable.ModifiedById);
        Assert.AreEqual(_currentUser.Guid, auditable.ModifiedBy!.Guid);
    }

    private void AssertNotModified(IModificationAuditable auditable)
    {
        Assert.IsNull(auditable.ModifiedAtUtc);
        Assert.IsNull(auditable.ModifiedById);
        Assert.IsNull(auditable.ModifiedBy);
    }

    private void AssertCreated(ICreationAuditable auditable)
    {
        Assert.AreEqual(_currentTime, auditable.CreatedAtUtc);
        Assert.AreEqual(_currentUser.Id, auditable.CreatedById);
        Assert.AreEqual(_currentUser.Guid, auditable.CreatedBy.Guid);
    }
}
