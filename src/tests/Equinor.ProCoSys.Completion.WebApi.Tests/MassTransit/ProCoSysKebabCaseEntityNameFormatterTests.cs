using Equinor.ProCoSys.Completion.WebApi.MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.MassTransit;

[TestClass]
public class ProCoSysKebabCaseEntityNameFormatterTests
{
    private ProCoSysKebabCaseEntityNameFormatter _formatter = null!;

    [TestInitialize]
    public void TestInitialize() => _formatter = new ProCoSysKebabCaseEntityNameFormatter();

    [TestMethod]
    public void TestFormatsCorrectlyWithoutIntegrationEventSuffix()
    {
        // Act
        var formattedName = _formatter.FormatEntityName<PunchItemCreated>();

        // Assert
        Assert.AreEqual("punch-item-created", formattedName);
    }

    [TestMethod]
    public void TestFormatsCorrectlyWithIntegrationEventSuffix()
    {
        // Act
        var formattedName = _formatter.FormatEntityName<PunchItemCreatedIntegrationEvent>();

        // Assert
        Assert.AreEqual("punch-item-created", formattedName);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class PunchItemCreated { }  // Dummy class

    // ReSharper disable once ClassNeverInstantiated.Local
    private class PunchItemCreatedIntegrationEvent { }  // Dummy class
}
