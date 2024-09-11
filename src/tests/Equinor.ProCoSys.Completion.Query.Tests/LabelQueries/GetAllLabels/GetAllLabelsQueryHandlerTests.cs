using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.LabelQueries.GetAllLabels;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Query.Tests.LabelQueries.GetAllLabels;

[TestClass]
public class GetAllLabelsQueryHandlerTests : ReadOnlyTestsBase
{
    private readonly GetAllLabelsQuery _query = new ();

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_WhenNoLabelsExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new GetAllLabelsQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectNumberOfLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        await Add4UnorderedLabelsInclusiveAVoidedLabelAsync(context);

        var dut = new GetAllLabelsQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnCorrectOrderedLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        await Add4UnorderedLabelsInclusiveAVoidedLabelAsync(context);

        var dut = new GetAllLabelsQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual(LabelTextA, result.ElementAt(0).Text);
        Assert.AreEqual(LabelTextB, result.ElementAt(1).Text);
        Assert.AreEqual(LabelTextC, result.ElementAt(2).Text);
        Assert.AreEqual(LabelTextVoided, result.ElementAt(3).Text);
    }

    [TestMethod]
    public async Task Handler_ShouldReturnBothVoidedAndNonVoidedLabels()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        await Add4UnorderedLabelsInclusiveAVoidedLabelAsync(context);

        var dut = new GetAllLabelsQueryHandler(context);

        // Act
        var result = await dut.Handle(_query, default);

        // Assert
        Assert.IsNotNull(result);

        var voidedLabel = result.SingleOrDefault(dto => dto.IsVoided);
        Assert.IsNotNull(voidedLabel);
        Assert.AreEqual(LabelTextVoided, voidedLabel.Text);

        var nonVoidedLabels = result.Where(dto => !dto.IsVoided).ToList();
        Assert.AreEqual(3, nonVoidedLabels.Count);
        Assert.AreEqual(LabelTextA, nonVoidedLabels.ElementAt(0).Text);
        Assert.AreEqual(LabelTextB, nonVoidedLabels.ElementAt(1).Text);
        Assert.AreEqual(LabelTextC, nonVoidedLabels.ElementAt(2).Text);
    }
}
