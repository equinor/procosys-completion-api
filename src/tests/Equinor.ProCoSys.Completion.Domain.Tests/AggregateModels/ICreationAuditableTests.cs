using System;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels;

[TestClass]
public abstract class ICreationAuditableTests
{
    protected DateTime _now = new(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    protected Person _person;

    protected abstract ICreationAuditable GetCreationAuditable();

    [TestInitialize]
    public void SetupCreationAuditableTests()
    {
        TimeService.SetProvider(new ManualTimeProvider(_now));
        _person = new Person(Guid.NewGuid(), null!, null!, null!, null!);
        _person.SetProtectedIdForTesting(3);
    }

    [TestMethod]
    public void SetCreated_ShouldSetProperties()
    {
        // Arrange
        var dut = GetCreationAuditable();
        
        // Act
        dut.SetCreated(_person);

        // Arrange
        Assert.AreEqual(_now, dut.CreatedAtUtc);
        Assert.AreEqual(_person.Id, dut.CreatedById);
        Assert.AreEqual(_person.Guid, dut.CreatedByOid);
    }
}
