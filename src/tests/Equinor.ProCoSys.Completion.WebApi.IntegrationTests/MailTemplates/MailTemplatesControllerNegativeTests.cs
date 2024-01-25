using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.MailTemplates;

[TestClass]
public class MailTemplatesControllerNegativeTests : TestBase
{
    #region GetMailTemplates
    [TestMethod]
    public async Task GetAllMailTemplates_AsAnonymous_ShouldReturnUnauthorized()
        => await MailTemplatesControllerTestsHelper.GetAllMailTemplatesAsync(
            UserType.Anonymous,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetAllMailTemplates_AsReader_ShouldReturnForbidden()
        => await MailTemplatesControllerTestsHelper.GetAllMailTemplatesAsync(
            UserType.Reader,
            HttpStatusCode.Forbidden);
    #endregion
}
