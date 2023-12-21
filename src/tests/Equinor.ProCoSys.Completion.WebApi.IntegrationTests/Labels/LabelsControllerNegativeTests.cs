using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Labels;

[TestClass]
public class LabelsControllerNegativeTests : TestBase
{
    #region GetLabels
    [TestMethod]
    public async Task GetLabels_AsAnonymous_ShouldReturnUnauthorized()
        => await LabelsControllerTestsHelper.GetLabelsAsync(
            UserType.Anonymous,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetLabels_AsReader_ShouldReturnForbidden()
        => await LabelsControllerTestsHelper.GetLabelsAsync(
            UserType.Reader,
            HttpStatusCode.Forbidden);
    #endregion

    #region CreateLabel
    [TestMethod]
    public async Task CreateLabel_AsAnonymous_ShouldReturnUnauthorized()
        => await LabelsControllerTestsHelper.CreateLabelAsync(
            UserType.Anonymous,
            Guid.NewGuid().ToString(),
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task CreateLabel_AsReader_ShouldReturnForbidden()
        => await LabelsControllerTestsHelper.CreateLabelAsync(
            UserType.Reader,
            Guid.NewGuid().ToString(),
            HttpStatusCode.Forbidden);
    #endregion

    #region UpdateLabel
    [TestMethod]
    public async Task UpdateLabel_AsAnonymous_ShouldReturnUnauthorized()
        => await LabelsControllerTestsHelper.UpdateLabelAsync(
            UserType.Anonymous,
            Guid.NewGuid().ToString(),
            new List<EntityTypeWithLabel>(),
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task UpdateLabel_AsReader_ShouldReturnForbidden()
    {
        // Arrange
        var text = Guid.NewGuid().ToString();
        await LabelsControllerTestsHelper.CreateLabelAsync(UserType.Writer, text);

        // Act and Assert
        await LabelsControllerTestsHelper.UpdateLabelAsync(
            UserType.Reader,
            text,
            new List<EntityTypeWithLabel>(),
            HttpStatusCode.Forbidden);
    }

    #endregion
}
