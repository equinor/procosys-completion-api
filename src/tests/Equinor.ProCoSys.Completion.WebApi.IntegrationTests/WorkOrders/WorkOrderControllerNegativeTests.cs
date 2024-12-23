﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.WorkOrders;

[TestClass]
public class WorkOrderControllerNegativeTests
{
    [TestMethod]
    public async Task GetWorkOrder_BySearch_AsAnonymous_ShouldReturnUnauthorized()
        => await WorkOrderControllerTestsHelper.SearchForWorkOrderAsync(
            KnownData.WorkOrderNo[KnownData.PlantA],
            UserType.Anonymous,
            HttpStatusCode.Unauthorized);

    [TestMethod]
    public async Task GetWorkOrder_BySearch_AsNoPermissionUser_ShouldReturnForbidden()
        => await WorkOrderControllerTestsHelper.SearchForWorkOrderAsync(
            KnownData.WorkOrderNo[KnownData.PlantA],
            UserType.NoPermissionUser,
            HttpStatusCode.Forbidden);
}

