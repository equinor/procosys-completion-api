﻿using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.Link;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.Link;

[TestClass]
public class LinkUpdatedV1ContractTests : ContractTestBase<ILinkUpdatedV1>
{
    [TestMethod]
    public override void Contract_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "DisplayName", typeof(string) },
            { "Guid", typeof(Guid) },
            { "SourceGuid", typeof(Guid) },
            { "SourceType", typeof(string) },
            { "Title", typeof(string) },
            { "Url", typeof(string) },
            { "ModifiedByOid", typeof(Guid) },
            { "ModifiedAtUtc", typeof(DateTime) },
            { "Changes", typeof(List<IProperty>) }
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.Link");
}