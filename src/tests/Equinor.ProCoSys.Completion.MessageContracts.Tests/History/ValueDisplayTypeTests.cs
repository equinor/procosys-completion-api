using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.History;

[TestClass]
public class ValueDisplayTypeTests
{
    // this test exists so developer reflect over consequences when changing the enum.
    // How will the receiver of a message react after the change?
    [TestMethod]
    public void ValueDisplayTypeEnums_ShouldNotChange()
    {
        // Arrange
        var expectedEnums = new List<string>
        {
            "StringAsText",
            "DateTimeAsDateAndTime",
            "DateTimeAsDateOnly",
            "BoolAsYesNo",
            "BoolAsTrueFalse",
            "IntAsText",
            "UserAsNameOnly",
            "UserAsNameAndPicture",
            "UserAsLinkToAddressBook",
            "UserAsContactCard"
        };
        var existingEnums = Enum.GetNames(typeof(ValueDisplayType)).ToList();

        // Assert
        CollectionAssert.AreEquivalent(expectedEnums, existingEnums);
    }
}
