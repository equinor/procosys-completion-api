using System;
using System.Linq;
using Equinor.ProCoSys.Completion.WebApi.Swagger;
using Equinor.ProCoSys.Completion.WebApi.Tests.Controllers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Swagger;

[TestClass]
public class SwaggerDocHelperTests
{
    [TestMethod]
    public void FillPatchDocumentWithSampleData_Should_FillPatchDocumentWithSampleData()
    {
        // Arrange
        var patchDocument = new JsonPatchDocument<PatchableObject>();

        // Act
        SwaggerDocHelper.FillPatchDocumentWithSampleData(patchDocument);

        // Assert
        AssertOperation(patchDocument, nameof(PatchableObject.MyString), typeof(string));
        AssertOperation(patchDocument, nameof(PatchableObject.MyNullableString1), typeof(string));
        AssertOperation(patchDocument, nameof(PatchableObject.MyInt), typeof(int));
        AssertOperation(patchDocument, nameof(PatchableObject.MyNullableInt1), typeof(int));
        AssertOperation(patchDocument, nameof(PatchableObject.MyDouble), typeof(double));
        AssertOperation(patchDocument, nameof(PatchableObject.MyNullableDouble1), typeof(double));
        AssertOperation(patchDocument, nameof(PatchableObject.MyGuid), typeof(Guid));
        AssertOperation(patchDocument, nameof(PatchableObject.MyNullableGuid1), typeof(Guid));
        AssertOperation(patchDocument, nameof(PatchableObject.MyDateTime), typeof(DateTime));
        AssertOperation(patchDocument, nameof(PatchableObject.MyNullableDateTime1), typeof(DateTime));
    }

    private void AssertOperation(
        JsonPatchDocument<PatchableObject> patchDocument, 
        string propertyName, 
        Type propertyType)
    {
        var op = patchDocument.Operations.SingleOrDefault(
            op => op.OperationType == OperationType.Replace &&
                  op.path == $"/{propertyName}" &&
                  op.value.GetType() == propertyType);
        Assert.IsNotNull(op);
    }
}
