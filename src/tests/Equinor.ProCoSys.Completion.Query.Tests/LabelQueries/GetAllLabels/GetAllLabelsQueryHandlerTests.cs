using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.LabelQueries.GetAllLabels;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.LabelQueries.GetAllLabels;

[TestClass]
public class GetAllLabelsQueryHandlerTests : ReadOnlyTestsBase
{
    //private Label _labelA;
    //private Label _labelB;
    //private Label _labelC;
    //private Label _labelD;
    //private Label _labelVoided;
    private readonly GetAllLabelsQuery _query = new ();

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        //using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        //_labelA = new Label("A");
        //_labelB = new Label("B");
        //_labelC = new Label("C");
        //_labelD = new Label("D");
        //_labelVoided = new Label("V") { IsVoided = true };

        //_labelEntityWith3Labels = new LabelEntity(_entityWithLabelsWith3Labels);
        //_labelEntityWithoutLabels = new LabelEntity(_entityWithLabelsWithoutLabels);
        //_labelEntityWith3Labels.AddLabel(_labelC);
        //_labelEntityWith3Labels.AddLabel(_labelA);
        //_labelEntityWith3Labels.AddLabel(_labelB);

        //context.LabelEntities.Add(_labelEntityWith3Labels);
        //context.LabelEntities.Add(_labelEntityWithoutLabels);
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_WhenNoLabelsExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var dut = new GetAllLabelsQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

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
        Add4UnorderedLabelsInclusiveAVoidedLabel(context);

        var dut = new GetAllLabelsQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(4, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectOrderedLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        Add4UnorderedLabelsInclusiveAVoidedLabel(context);

        var dut = new GetAllLabelsQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.AreEqual(LabelTextA, result.Data.ElementAt(0).Text);
        Assert.AreEqual(LabelTextB, result.Data.ElementAt(1).Text);
        Assert.AreEqual(LabelTextC, result.Data.ElementAt(2).Text);
        Assert.AreEqual(LabelTextVoided, result.Data.ElementAt(3).Text);
    }

    [TestMethod]
    public async Task Handler_ShouldReturnBothVoidedAndNonVoidedLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);
        Add4UnorderedLabelsInclusiveAVoidedLabel(context);

        var dut = new GetAllLabelsQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        var voidedLabel = result.Data.SingleOrDefault(dto => dto.IsVoided);
        Assert.IsNotNull(voidedLabel);
        Assert.AreEqual(LabelTextVoided, voidedLabel.Text);

        var nonVoidedLabels = result.Data.Where(dto => !dto.IsVoided).ToList();
        Assert.AreEqual(3, nonVoidedLabels.Count);
        Assert.AreEqual(LabelTextA, nonVoidedLabels.ElementAt(0).Text);
        Assert.AreEqual(LabelTextB, nonVoidedLabels.ElementAt(1).Text);
        Assert.AreEqual(LabelTextC, nonVoidedLabels.ElementAt(2).Text);
    }
}
