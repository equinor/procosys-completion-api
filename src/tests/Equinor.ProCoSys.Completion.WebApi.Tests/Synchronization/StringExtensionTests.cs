using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class StringExtensionTests
{
    [TestMethod]
    public void ToLibraryType_ShouldReturnLibraryType_WhenValidLibraryType()
    {
        // Arrange
        var commPriority = LibraryType.COMM_PRIORITY;

        // Act
        var libraryType = commPriority.ToString().ToLibraryType();

        // Assert
        Assert.AreEqual(commPriority, libraryType);
    }

    [TestMethod]
    public void ToLibraryType_ShouldThrowArgumentException_WhenUnknownLibraryType() =>
        // Act and Assert
        Assert.ThrowsException<ArgumentException>(() => "Something".ToLibraryType());
}
