﻿using System.Linq;
using Equinor.ProCoSys.Completion.Infrastructure;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.WorkOrderQueries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using System;

namespace Equinor.ProCoSys.Completion.Query.Tests.WorkOrderQueries.WorkOrderSearch;

[TestClass]
public class WorkOrderSearchQueryHandlerTest : ReadOnlyTestsBase
{
    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
    }

    [TestMethod]
    public async Task Handler_ShouldReturnEmptyList_WhenNoMatchesExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new WorkOrderSearchQueryHandler(context);
        WorkOrderSearchQuery query = new(Guid.NewGuid().ToString());

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handler_ShouldReturnWorkOrder_WhenMatchesExists()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        var dut = new WorkOrderSearchQueryHandler(context);
        WorkOrderSearchQuery query = new(WorkOrderNo);

        // Act
        var result = await dut.Handle(query, default);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(1, result.Data.Count());
    }
}
