using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
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
    private readonly string _voidedText = "VoidedText";
    private Label _labelA;
    private Label _labelB;
    private Label _labelC;
    private Label _labelD;
    private Label _labelVoided;
    private LabelEntity _labelEntityWith3Labels;
    private LabelEntity _labelEntityWithoutLabels;
    private readonly EntityWithLabelType _entityWithLabelsWith3Labels = EntityWithLabelType.PunchComment;
    private readonly EntityWithLabelType _entityWithLabelsWithoutLabels = EntityWithLabelType.PunchPicture;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        _labelA = new Label("A");
        _labelB = new Label("B");
        _labelC = new Label("C");
        _labelD = new Label("D");
        
        _labelVoided = new Label(_voidedText) { IsVoided = true };

        _labelEntityWith3Labels = new LabelEntity(_entityWithLabelsWith3Labels);
        _labelEntityWithoutLabels = new LabelEntity(_entityWithLabelsWithoutLabels);
        _labelEntityWith3Labels.AddLabel(_labelC);
        _labelEntityWith3Labels.AddLabel(_labelA);
        _labelEntityWith3Labels.AddLabel(_labelB);
        _labelEntityWith3Labels.AddLabel(_labelVoided);

        context.Labels.Add(_labelA);
        context.Labels.Add(_labelB);
        context.Labels.Add(_labelC);
        context.Labels.Add(_labelD);
        context.Labels.Add(_labelVoided);
        context.LabelEntities.Add(_labelEntityWith3Labels);
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

        var query = new GetLabelsForEntityQuery(_entityWithLabelsWith3Labels);
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

        var query = new GetLabelsForEntityQuery(_entityWithLabelsWith3Labels);
        var dut = new GetLabelsForEntityQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.AreEqual(result.Data.ElementAt(0), _labelA.Text);
        Assert.AreEqual(result.Data.ElementAt(1), _labelB.Text);
        Assert.AreEqual(result.Data.ElementAt(2), _labelC.Text);
    }

    [TestMethod]
    public async Task Handler_ShouldNotReturnVoidedLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetLabelsForEntityQuery(_entityWithLabelsWith3Labels);
        var dut = new GetLabelsForEntityQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.IsFalse(result.Data.Any(t => t == _voidedText));
    }
}
