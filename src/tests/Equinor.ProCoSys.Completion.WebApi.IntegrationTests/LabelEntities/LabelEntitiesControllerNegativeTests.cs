using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.LabelEntities;

[TestClass]
public class LabelEntitiesControllerNegativeTests : TestBase
{
    #region GetLabelsForEntity
    [TestMethod]
    public async Task GetLabelsForEntity_AsAnonymous_ShouldReturnUnauthorized()
        => await LabelEntitiesControllerTestsHelper.GetLabelsForEntityAsync(
            UserType.Anonymous,
            EntityTypeWithLabel.PunchComment,
            HttpStatusCode.Unauthorized);
    #endregion
}
