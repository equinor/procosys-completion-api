using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Labels;

[TestClass]
public class LabelsControllerTests : TestBase
{
    [TestMethod]
    public async Task GetLabels_AsWriter_ShouldGetLabels()
    {
        // Act
        var labels = await LabelsControllerTestsHelper.GetLabelsAsync(UserType.Writer);

        // Assert
        Assert.IsTrue(labels.Count >= 2);
        Assert.IsTrue(labels.Any(l => l.Text == KnownData.LabelA));
        Assert.IsTrue(labels.Any(l => l.Text == KnownData.LabelB));
    }

    [TestMethod]
    public async Task CreateLabel_AsWriter_ShouldCreateLabel()
    {
        // Arrange
        var text = Guid.NewGuid().ToString();

        // Act
        var rowVersion = await LabelsControllerTestsHelper.CreateLabelAsync(UserType.Writer, text);

        // Assert
        AssertValidRowVersion(rowVersion);

        var labels = await LabelsControllerTestsHelper.GetLabelsAsync(UserType.Writer);
        var label = labels.Single(l => l.Text == text);
        Assert.IsNotNull(label);
        Assert.AreEqual(0, label.AvailableFor.Count);
    }

    [TestMethod]
    public async Task UpdateLabel_AsWriter_ShouldUpdateLabel()
    {
        // Arrange
        var text = Guid.NewGuid().ToString();
        await LabelsControllerTestsHelper.CreateLabelAsync(UserType.Writer, text);

        // Act
        await LabelsControllerTestsHelper.UpdateLabelAsync(
            UserType.Writer,
            text,
            new List<EntityTypeWithLabel>{ EntityTypeWithLabel.PunchComment });

        // Assert
        var labels = await LabelsControllerTestsHelper.GetLabelsAsync(UserType.Writer);
        var label = labels.Single(l => l.Text == text);
        Assert.IsNotNull(label);
        Assert.AreEqual(1, label.AvailableFor.Count);
        Assert.AreEqual(EntityTypeWithLabel.PunchComment.ToString(), label.AvailableFor.ElementAt(0));
    }
}
