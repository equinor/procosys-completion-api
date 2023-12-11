using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class LabelRepositoryTests : RepositoryTestBase<Label>
{
    private readonly string _labelTextWithBothCasing = "Abc";

    protected override EntityRepository<Label> GetDut()
        => new LabelRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var label = new Label(_labelTextWithBothCasing);
        label.SetProtectedIdForTesting(_knownId);
        label.MakeLabelAvailableFor(new LabelEntity(EntityTypeWithLabels.PunchComment));

        var labels = new List<Label> { label };

        _dbSetMock = labels.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Labels
            .Returns(_dbSetMock);
    }

    protected override Label GetNewEntity() => new("Whatever");

    [TestMethod]
    public async Task GetManyAsync_KnownLabels_ShouldReturnLabels()
    {
        // Arrange
        var dut = new LabelRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.GetManyAsync(new List<string>{_labelTextWithBothCasing}, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result.Any(l => l.Text == _labelTextWithBothCasing));
    }

    [TestMethod]
    public async Task GetManyAsync_KnownLabels_ShouldReturnLabels_CaseInsensitive()
    {
        // Arrange
        var dut = new LabelRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.GetManyAsync(new List<string> { _labelTextWithBothCasing.ToLower() }, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result.Any(l => l.Text == _labelTextWithBothCasing));
    }

    [TestMethod]
    public async Task GetManyAsync_UnknownLabels_ShouldReturnEmptyList()
    {
        // Arrange
        var dut = new LabelRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.GetManyAsync(new List<string> { Guid.NewGuid().ToString() }, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetByTextAsync_KnownLabel_ShouldReturnLabelWithLabelEntities()
    {
        // Arrange
        var dut = new LabelRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.GetByTextAsync(_labelTextWithBothCasing, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_labelTextWithBothCasing, result.Text);
        Assert.AreEqual(1, result.AvailableFor.Count);
    }

    [TestMethod]
    public async Task GetByTextAsync_KnownLabel_ShouldReturnLabel_CaseInsensitive()
    {
        // Arrange
        var dut = new LabelRepository(_contextHelper.ContextMock);

        // Act
        var result = await dut.GetByTextAsync(_labelTextWithBothCasing.ToLower(), default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_labelTextWithBothCasing, result.Text);
        Assert.AreEqual(1, result.AvailableFor.Count);
    }

    [TestMethod]
    public async Task GetByTextAsync_UnknownLabel_ShouldThrowException()
    {
        // Arrange
        var dut = new LabelRepository(_contextHelper.ContextMock);

        // Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => dut.GetByTextAsync(Guid.NewGuid().ToString(), default));
    }
}
