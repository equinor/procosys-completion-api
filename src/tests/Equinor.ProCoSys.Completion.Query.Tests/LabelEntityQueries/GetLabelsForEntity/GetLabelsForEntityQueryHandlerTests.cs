using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.LabelEntityQueries.GetLabelsForEntity;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.LabelEntityQueries.GetLabelsForEntity;

[TestClass]
public class GetLabelsForEntityQueryHandlerTests : ReadOnlyTestsBase
{
    private LabelEntity _labelEntityWith3NonVoidedLabels;
    private LabelEntity _labelEntityWithoutLabels;
    private readonly EntityWithLabelType _entityWithLabelsWith3NonVoidedLabels = EntityWithLabelType.PunchComment;
    private readonly EntityWithLabelType _entityWithLabelsWithoutLabels = EntityWithLabelType.PunchPicture;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        Add4UnorderedLabelsInclusiveAVoidedLabel(context);
        var labelA = context.Labels.Single(l => l.Text == LabelTextA);
        var labelB = context.Labels.Single(l => l.Text == LabelTextB);
        var labelC = context.Labels.Single(l => l.Text == LabelTextC);
        var labelVoided = context.Labels.Single(l => l.Text == LabelTextVoided);

        _labelEntityWith3NonVoidedLabels = new LabelEntity(_entityWithLabelsWith3NonVoidedLabels);
        _labelEntityWithoutLabels = new LabelEntity(_entityWithLabelsWithoutLabels);
        _labelEntityWith3NonVoidedLabels.AddLabel(labelC);
        _labelEntityWith3NonVoidedLabels.AddLabel(labelA);
        _labelEntityWith3NonVoidedLabels.AddLabel(labelVoided);
        _labelEntityWith3NonVoidedLabels.AddLabel(labelB);

        context.LabelEntities.Add(_labelEntityWith3NonVoidedLabels);
        context.LabelEntities.Add(_labelEntityWithoutLabels);

        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_WhenEntityHasNoLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetLabelsForEntityQuery(_entityWithLabelsWithoutLabels);
        var dut = new GetLabelsForEntityQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectNumberOfLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetLabelsForEntityQuery(_entityWithLabelsWith3NonVoidedLabels);
        var dut = new GetLabelsForEntityQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(3, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectOrderedNonVoidedLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetLabelsForEntityQuery(_entityWithLabelsWith3NonVoidedLabels);
        var dut = new GetLabelsForEntityQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.AreEqual(LabelTextA, result.Data.ElementAt(0));
        Assert.AreEqual(LabelTextB, result.Data.ElementAt(1));
        Assert.AreEqual(LabelTextC, result.Data.ElementAt(2));
    }

    [TestMethod]
    public async Task Handler_ShouldNotReturnVoidedLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetLabelsForEntityQuery(_entityWithLabelsWith3NonVoidedLabels);
        var dut = new GetLabelsForEntityQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.IsFalse(result.Data.Any(t => t == LabelTextVoided));
    }
}
