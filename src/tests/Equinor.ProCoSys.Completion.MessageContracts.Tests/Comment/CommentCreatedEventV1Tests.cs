using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.Comment;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.MessageContracts.Tests.Comment;

[TestClass]
public class CommentCreatedEventV1Tests : ContractTestBase<ICommentCreatedEventV1>
{
    [TestMethod]
    public override void Contract_Interface_DoNotChange()
    {
        // Arrange
        var expectedProperties = new Dictionary<string, Type>
        {
            { "Plant", typeof(string) },
            { "Guid", typeof(Guid) },
            { "ParentGuid", typeof(Guid) },
            { "Text", typeof(string) },
            { "CreatedBy", typeof(User) },
            { "CreatedAtUtc", typeof(DateTime) },
            { "Labels", typeof(IEnumerable<string>) },
            { "MessageId", typeof(Guid)}
        };

        // Act
        AssertPropertiesNotChanged(expectedProperties);
    }

    [TestMethod]
    public override void Contract_Namespace_DoNotChange()
        => AssertNamespaceNotChanged("Equinor.ProCoSys.Completion.MessageContracts.Comment");
}
