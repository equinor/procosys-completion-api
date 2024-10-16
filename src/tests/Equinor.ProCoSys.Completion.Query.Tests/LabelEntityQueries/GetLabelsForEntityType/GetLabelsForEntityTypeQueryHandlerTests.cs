﻿using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.LabelEntityQueries.GetLabelsForEntityType;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Query.Tests.LabelEntityQueries.GetLabelsForEntityType;

[TestClass]
public class GetLabelsForEntityTypeQueryHandlerTests : ReadOnlyTestsBase
{
    private LabelEntity _labelEntityWith3NonVoidedLabels;
    private LabelEntity _labelEntityWithoutLabels;
    private readonly EntityTypeWithLabel _entityTypeWith3NonVoidedLabels = EntityTypeWithLabel.PunchComment;
    private readonly EntityTypeWithLabel _entityTypeWithoutLabels = EntityTypeWithLabel.PunchPicture;

    protected override async void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        await using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        await Add4UnorderedLabelsInclusiveAVoidedLabelAsync(context);
        var labelA = context.Labels.Single(l => l.Text == LabelTextA);
        var labelB = context.Labels.Single(l => l.Text == LabelTextB);
        var labelC = context.Labels.Single(l => l.Text == LabelTextC);
        var labelVoided = context.Labels.Single(l => l.Text == LabelTextVoided);

        _labelEntityWith3NonVoidedLabels = new LabelEntity(_entityTypeWith3NonVoidedLabels);
        _labelEntityWithoutLabels = new LabelEntity(_entityTypeWithoutLabels);
        labelC.MakeLabelAvailableFor(_labelEntityWith3NonVoidedLabels);
        labelA.MakeLabelAvailableFor(_labelEntityWith3NonVoidedLabels);
        labelVoided.MakeLabelAvailableFor(_labelEntityWith3NonVoidedLabels);
        labelB.MakeLabelAvailableFor(_labelEntityWith3NonVoidedLabels);

        context.LabelEntities.Add(_labelEntityWith3NonVoidedLabels);
        context.LabelEntities.Add(_labelEntityWithoutLabels);

        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_WhenEntityHasNoLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var query = new GetLabelsForEntityTypeQuery(_entityTypeWithoutLabels);
        var dut = new GetLabelsForEntityTypeQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectNumberOfLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var query = new GetLabelsForEntityTypeQuery(_entityTypeWith3NonVoidedLabels);
        var dut = new GetLabelsForEntityTypeQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectOrderedNonVoidedLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var query = new GetLabelsForEntityTypeQuery(_entityTypeWith3NonVoidedLabels);
        var dut = new GetLabelsForEntityTypeQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(LabelTextA, result.ElementAt(0));
        Assert.AreEqual(LabelTextB, result.ElementAt(1));
        Assert.AreEqual(LabelTextC, result.ElementAt(2));
    }

    [TestMethod]
    public async Task Handler_ShouldNotReturnVoidedLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var query = new GetLabelsForEntityTypeQuery(_entityTypeWith3NonVoidedLabels);
        var dut = new GetLabelsForEntityTypeQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);

        Assert.IsFalse(result.Any(t => t == LabelTextVoided));
    }
}
