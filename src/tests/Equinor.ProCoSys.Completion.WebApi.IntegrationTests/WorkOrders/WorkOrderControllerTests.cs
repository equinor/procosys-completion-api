using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.WorkOrders;

[TestClass]
public class WorkOrderControllerTests
{

    [TestMethod]
    public async Task GetWorkOrder_BySearch_AsReader_ShouldReturnWorkOrder()
    {
        // Act
        var workOrders = await WorkOrderControllerTestsHelper.SearchForWorkOrderAsync("004", UserType.Reader);
        
        // Assert
        Assert.IsTrue(workOrders.Count == 1);
        Assert.IsTrue(workOrders.Any(a => a.Guid == TestFactory.OriginalWorkOrderGuid));
    }
}

