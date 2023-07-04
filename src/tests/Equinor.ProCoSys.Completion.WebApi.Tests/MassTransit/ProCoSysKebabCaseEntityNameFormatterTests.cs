using Equinor.ProCoSys.Completion.WebApi.MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.MassTransit;

[TestClass]
public class ProCoSysKebabCaseEntityNameFormatterTests
{
    private ProCoSysKebabCaseEntityNameFormatter _formatter;

    [TestInitialize]
    public void TestInitialize() => _formatter = new ProCoSysKebabCaseEntityNameFormatter();

    [TestMethod]
    public void TestFormatsCorrectlyWithoutIntegrationEventSuffix()
    {
        // Act
        var formattedName = _formatter.FormatEntityName<PunchCreated>();

        // Assert
        Assert.AreEqual("punch-created", formattedName);
    }

    [TestMethod]
    public void TestFormatsCorrectlyWithIntegrationEventSuffix()
    {
        // Act
        var formattedName = _formatter.FormatEntityName<PunchCreatedIntegrationEvent>();

        // Assert
        Assert.AreEqual("punch-created", formattedName);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class PunchCreated { }  // Dummy class

    // ReSharper disable once ClassNeverInstantiated.Local
    private class PunchCreatedIntegrationEvent { }  // Dummy class
}
