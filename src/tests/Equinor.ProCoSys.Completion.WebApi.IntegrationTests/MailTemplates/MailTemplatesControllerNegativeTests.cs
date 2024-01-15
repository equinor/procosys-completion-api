using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.MailTemplates;

[TestClass]
public class MailTemplatesControllerNegativeTests : TestBase
{
    #region GetMailTemplates
    [TestMethod]
    public async Task GetMailTemplates_AsAnonymous_ShouldReturnUnauthorized()
        => await MailTemplatesControllerTestsHelper.GetMailTemplatesAsync(
            UserType.Anonymous,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetMailTemplates_AsReader_ShouldReturnForbidden()
        => await MailTemplatesControllerTestsHelper.GetMailTemplatesAsync(
            UserType.Reader,
            HttpStatusCode.Forbidden);
    #endregion
}
