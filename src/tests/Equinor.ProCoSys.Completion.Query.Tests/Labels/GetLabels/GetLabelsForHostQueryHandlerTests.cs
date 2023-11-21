using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelHostAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.LabelQueries.GetLabels;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.Labels.GetLabels;

[TestClass]
public class GetLabelsForHostQueryHandlerTests : ReadOnlyTestsBase
{
    private Label _labelA;
    private Label _labelB;
    private Label _labelC;
    private Label _labelD;
    private LabelHost _labelHostWith3Labels;
    private LabelHost _labelHostWithoutLabels;
    private HostType _hostTypeWith3Labels;
    private HostType _hostTypeWithoutLabels;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        _hostTypeWith3Labels = HostType.PunchComment;
        _hostTypeWithoutLabels = HostType.GeneralComment;

        _labelA = new Label("A");
        _labelB = new Label("B");
        _labelC = new Label("C");
        _labelD = new Label("D");

        _labelHostWith3Labels = new LabelHost(_hostTypeWith3Labels);
        _labelHostWithoutLabels = new LabelHost(_hostTypeWithoutLabels);
        _labelHostWith3Labels.AddLabel(_labelC);
        _labelHostWith3Labels.AddLabel(_labelA);
        _labelHostWith3Labels.AddLabel(_labelB);

        context.Labels.Add(_labelA);
        context.Labels.Add(_labelB);
        context.Labels.Add(_labelC);
        context.Labels.Add(_labelD);
        context.LabelHosts.Add(_labelHostWith3Labels);
        context.LabelHosts.Add(_labelHostWithoutLabels);

        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_WhenHostHasNoLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetLabelsForHostQuery(_hostTypeWithoutLabels);
        var dut = new GetLabelsForHostQueryHandler(context);

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

        var query = new GetLabelsForHostQuery(_hostTypeWith3Labels);
        var dut = new GetLabelsForHostQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(3, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectOrderedLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMockObject, _eventDispatcherMockObject, _currentUserProviderMockObject);

        var query = new GetLabelsForHostQuery(_hostTypeWith3Labels);
        var dut = new GetLabelsForHostQueryHandler(context);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);

        Assert.AreEqual(result.Data.ElementAt(0), _labelA.Text);
        Assert.AreEqual(result.Data.ElementAt(1), _labelB.Text);
        Assert.AreEqual(result.Data.ElementAt(2), _labelC.Text);
    }
}
