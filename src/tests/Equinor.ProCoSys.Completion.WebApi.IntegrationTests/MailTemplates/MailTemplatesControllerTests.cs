using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.MailTemplates;

[TestClass]
public class MailTemplatesControllerTests : TestBase
{
    [TestMethod]
    public async Task GetAllMailTemplates_AsWriter_ShouldGetMailTemplates()
    {
        // Act
        var mailTemplates = await MailTemplatesControllerTestsHelper.GetAllMailTemplatesAsync(UserType.Writer);

        // Assert
        Assert.IsTrue(mailTemplates.Count >= 2);
        Assert.IsTrue(mailTemplates.Any(l => l.Code == KnownData.MailTemplateA));
        Assert.IsTrue(mailTemplates.Any(l => l.Code == KnownData.MailTemplateB));
    }
}
