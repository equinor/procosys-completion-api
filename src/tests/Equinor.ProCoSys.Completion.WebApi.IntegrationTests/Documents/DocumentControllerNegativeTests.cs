using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Documents;

[TestClass]
public class DocumentControllerNegativeTests
{
    [TestMethod]
    public async Task GetDocument_BySearch_AsAnonymous_ShouldReturnUnauthorized()
        => await DocumentControllerTestsHelper.SearchForDocumentAsync(
            KnownData.DocumentNo[KnownData.PlantA],
            UserType.Anonymous,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetDocument_BySearch_AsNoPermissionUser_ShouldReturnForbidden()
        => await DocumentControllerTestsHelper.SearchForDocumentAsync(
            KnownData.DocumentNo[KnownData.PlantA],
            UserType.NoPermissionUser,
            HttpStatusCode.Forbidden);
}

