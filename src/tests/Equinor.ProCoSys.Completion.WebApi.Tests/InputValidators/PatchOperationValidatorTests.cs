using System;
using System.Globalization;
using Equinor.ProCoSys.Completion.WebApi.InputValidators;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.InputValidators;

[TestClass]
public class PatchOperationValidatorTests
{
    private readonly string _rowVersion = "AAAAAAAAABA=";
    private PatchOperationValidator _dut = null!;
    private JsonPatchDocument<PatchableObject> _patchDocument = null!;
    private IRowVersionValidator _rowVersionValidatorMock = null!;

    [TestInitialize]
    public void SetUp()
    {
        _patchDocument = new JsonPatchDocument<PatchableObject>();
        _patchDocument.Replace(p => p.RowVersion, _rowVersion);
        
        _patchDocument.Replace(p => p.MyString, "blah");
        _patchDocument.Replace(p => p.MyNullableString1, "blah");
        _patchDocument.Replace(p => p.MyNullableString2, null);

        _patchDocument.Replace(p => p.MyInt, 1);
        _patchDocument.Replace(p => p.MyNullableInt1, 1);
        _patchDocument.Replace(p => p.MyNullableInt2, null);

        _patchDocument.Replace(p => p.MyGuid, Guid.NewGuid());
        _patchDocument.Replace(p => p.MyNullableGuid1, Guid.NewGuid());
        _patchDocument.Replace(p => p.MyNullableGuid2, null);

        _patchDocument.Replace(p => p.MyDateTime, DateTime.Now);
        _patchDocument.Replace(p => p.MyNullableDateTime1, DateTime.Now);
        _patchDocument.Replace(p => p.MyNullableDateTime2, null);

        _rowVersionValidatorMock = Substitute.For<IRowVersionValidator>();
        _rowVersionValidatorMock.IsValid(_rowVersion).Returns(true);

        _dut = new PatchOperationValidator(_rowVersionValidatorMock);
    }

    #region HaveValidReplaceOperationsOnly and GetMessageForIllegalReplaceOperations
    [TestMethod]
    public void HaveValidReplaceOperationsOnly_ShouldReturnTrue_WhenValid()
    {
        // Act
        var result1 = _dut.HaveValidReplaceOperationsOnly(_patchDocument.Operations);
        var result2 = _dut.GetMessageForIllegalReplaceOperations(_patchDocument.Operations);

        //Assert
        Assert.IsTrue(result1);
        Assert.IsNull(result2);
    }

    [TestMethod]
    public void HaveValidReplaceOperationsOnly_ShouldReturnTrue_WhenSettingIntAsString()
    {
        // Assert
        _patchDocument.Operations.Clear();
        // cheat to be able to set null to a non-nullable 
        _patchDocument.Operations.Add(new Operation<PatchableObject>(
            "replace",
            $"/{nameof(PatchableObject.MyInt)}",
            null,
            "2"));

        // Act
        var result1 = _dut.HaveValidReplaceOperationsOnly(_patchDocument.Operations);
        var result2 = _dut.GetMessageForIllegalReplaceOperations(_patchDocument.Operations);

        //Assert
        Assert.IsTrue(result1);
        Assert.IsNull(result2);
    }

    [TestMethod]
    public void HaveValidReplaceOperationsOnly_ShouldReturnFalse_WhenSettingIllegalInt()
    {
        // Assert
        _patchDocument.Operations.Clear();
        // cheat to be able to set null to a non-nullable 
        var propName = nameof(PatchableObject.MyInt);
        _patchDocument.Operations.Add(new Operation<PatchableObject>(
            "replace",
            $"/{propName}",
            null,
            "blah"));

        // Act
        var result1 = _dut.HaveValidReplaceOperationsOnly(_patchDocument.Operations);
        var result2 = _dut.GetMessageForIllegalReplaceOperations(_patchDocument.Operations);

        //Assert
        Assert.IsFalse(result1);
        Assert.IsNotNull(result2);
        Assert.IsTrue(result2.StartsWith("Can't assign value"));
        Assert.IsTrue(result2.Contains($"to property {propName}"));
    }

    [TestMethod]
    public void HaveValidReplaceOperationsOnly_ShouldReturnTrue_WhenSettingGuidAsString()
    {
        // Assert
        _patchDocument.Operations.Clear();
        // cheat to be able to set null to a non-nullable 
        _patchDocument.Operations.Add(new Operation<PatchableObject>(
            "replace",
            $"/{nameof(PatchableObject.MyGuid)}",
            null,
            Guid.NewGuid().ToString()));

        // Act
        var result1 = _dut.HaveValidReplaceOperationsOnly(_patchDocument.Operations);
        var result2 = _dut.GetMessageForIllegalReplaceOperations(_patchDocument.Operations);

        //Assert
        Assert.IsTrue(result1);
        Assert.IsNull(result2);
    }

    [TestMethod]
    public void HaveValidReplaceOperationsOnly_ShouldReturnFalse_WhenSettingIllegalGuid()
    {
        // Assert
        _patchDocument.Operations.Clear();
        // cheat to be able to set null to a non-nullable 
        var propName = nameof(PatchableObject.MyGuid);
        _patchDocument.Operations.Add(new Operation<PatchableObject>(
            "replace",
            $"/{propName}",
            null,
            "blah"));

        // Act
        var result1 = _dut.HaveValidReplaceOperationsOnly(_patchDocument.Operations);
        var result2 = _dut.GetMessageForIllegalReplaceOperations(_patchDocument.Operations);

        //Assert
        Assert.IsFalse(result1);
        Assert.IsNotNull(result2);
        Assert.IsTrue(result2.StartsWith("Can't assign value"));
        Assert.IsTrue(result2.Contains($"to property {propName}"));
    }

    [TestMethod]
    public void HaveValidReplaceOperationsOnly_ShouldReturnTrue_WhenSettingDateTimeAsString()
    {
        // Assert
        _patchDocument.Operations.Clear();
        // cheat to be able to set null to a non-nullable 
        _patchDocument.Operations.Add(new Operation<PatchableObject>(
            "replace",
            $"/{nameof(PatchableObject.MyDateTime)}",
            null,
            DateTime.Now.ToString(CultureInfo.InvariantCulture)));

        // Act
        var result1 = _dut.HaveValidReplaceOperationsOnly(_patchDocument.Operations);
        var result2 = _dut.GetMessageForIllegalReplaceOperations(_patchDocument.Operations);

        //Assert
        Assert.IsTrue(result1);
        Assert.IsNull(result2);
    }

    [TestMethod]
    public void HaveValidReplaceOperationsOnly_ShouldReturnFalse_WhenSettingIllegalDateTime()
    {
        // Assert
        _patchDocument.Operations.Clear();
        // cheat to be able to set null to a non-nullable 
        var propName = nameof(PatchableObject.MyDateTime);
        _patchDocument.Operations.Add(new Operation<PatchableObject>(
            "replace",
            $"/{propName}",
            null,
            "blah"));

        // Act
        var result1 = _dut.HaveValidReplaceOperationsOnly(_patchDocument.Operations);
        var result2 = _dut.GetMessageForIllegalReplaceOperations(_patchDocument.Operations);

        //Assert
        Assert.IsFalse(result1);
        Assert.IsNotNull(result2);
        Assert.IsTrue(result2.StartsWith("Can't assign value"));
        Assert.IsTrue(result2.Contains($"to property {propName}"));
    }

    // todo add more test to check invalid

    #endregion

    #region HaveReplaceOperationsOnly
    [TestMethod]
    public void HaveReplaceOperationsOnly_ShouldReturnTrue_WhenValid()
    {
        // Act
        var result = _dut.HaveReplaceOperationsOnly(_patchDocument.Operations);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HaveReplaceOperationsOnly_ShouldReturnFalse_WhenAddOperationExists()
    {
        // Arrange
        _patchDocument.Add(p => p.MyString, "");

        // Act
        var result = _dut.HaveReplaceOperationsOnly(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HaveReplaceOperationsOnly_ShouldReturnFalse_WhenMoveOperationExists()
    {
        // Arrange
        _patchDocument.Move(p => p.MyString, p => p.MyNullableString1);

        // Act
        var result = _dut.HaveReplaceOperationsOnly(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HaveReplaceOperationsOnly_ShouldReturnFalse_WhenCopyOperationExists()
    {
        // Arrange
        _patchDocument.Copy(p => p.MyString, p => p.MyNullableString1);

        // Act
        var result = _dut.HaveReplaceOperationsOnly(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HaveReplaceOperationsOnly_ShouldReturnFalse_WhenTestOperationExists()
    {
        // Arrange
        _patchDocument.Test(p => p.MyString, "blah");

        // Act
        var result = _dut.HaveReplaceOperationsOnly(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HaveReplaceOperationsOnly_ShouldReturnFalse_WhenRemoveOperationExists()
    {
        // Arrange
        _patchDocument.Remove(p => p.MyString);

        // Act
        var result = _dut.HaveReplaceOperationsOnly(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region HaveUniqueReplaceOperations
    [TestMethod]
    public void HaveUniqueReplaceOperations_ShouldReturnTrue_WhenValid()
    {
        // Act
        var result = _dut.HaveUniqueReplaceOperations(_patchDocument.Operations);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HaveUniqueReplaceOperations_ShouldReturnFalse_WhenDuplicateReplaceOperationGiven()
    {
        // Arrange
        _patchDocument.Replace(p => p.MyString, "blah");

        // Act
        var result = _dut.HaveUniqueReplaceOperations(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region HaveValidRowVersionOperation
    [TestMethod]
    public void HaveValidRowVersionOperation_ShouldReturnTrue_WhenValid()
    {
        // Act
        var result = _dut.HaveValidRowVersionOperation(_patchDocument.Operations);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HaveValidRowVersionOperation_ShouldReturnFalse_WhenRowVersionNotGiven()
    {
        // Arrange
        _patchDocument.Operations.Clear();

        // Act
        var result = _dut.HaveValidRowVersionOperation(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HaveValidRowVersionOperation_ShouldReturnFalse_WhenInvalidRowVersion()
    {
        // Arrange
        _rowVersionValidatorMock.IsValid(_rowVersion).Returns(false);

        // Act
        var result = _dut.HaveValidRowVersionOperation(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result);
    }
    #endregion

    #region AllRequiredFieldsHaveValue and 

    [TestMethod]
    public void AllRequiredFieldsHaveValue_ShouldReturnTrue_WhenValid()
    {
        // Act
        var result1 = _dut.AllRequiredFieldsHaveValue(_patchDocument.Operations);
        var result2 = _dut.GetMessageForRequiredFields(_patchDocument.Operations);

        // Assert
        Assert.IsTrue(result1);
        Assert.IsNull(result2);
    }

    [TestMethod]
    public void AllRequiredFieldsHaveValue_ShouldReturnFalse_WhenTryingToSetNullInRequiredString()
    {
        // Arrange
        _patchDocument.Operations.Clear();
        _patchDocument.Replace(p => p.MyString, null);
        
        // Act
        var result1 = _dut.AllRequiredFieldsHaveValue(_patchDocument.Operations);
        var result2 = _dut.GetMessageForRequiredFields(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result1);
        Assert.IsNotNull(result2);
        Assert.IsTrue(result2.EndsWith("MyString"));
    }

    [TestMethod]
    public void AllRequiredFieldsHaveValue_ShouldReturnFalse_WhenTryingToSetNullInRequiredInt()
    {
        // Arrange
        _patchDocument.Operations.Clear();
        // cheat to be able to set null to a non-nullable 
        _patchDocument.Operations.Add(new Operation<PatchableObject>(
            "replace",
            $"/{nameof(PatchableObject.MyInt)}",
            null,
            null));

        // Act
        var result1 = _dut.AllRequiredFieldsHaveValue(_patchDocument.Operations);
        var result2 = _dut.GetMessageForRequiredFields(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result1);
        Assert.IsNotNull(result2);
        Assert.IsTrue(result2.EndsWith("MyInt"));
    }

    [TestMethod]
    public void AllRequiredFieldsHaveValue_ShouldReturnFalse_WhenTryingToSetNullInRequiredDateTime()
    {
        // Arrange
        _patchDocument.Operations.Clear();
        // cheat to be able to set null to a non-nullable 
        _patchDocument.Operations.Add(new Operation<PatchableObject>(
            "replace",
            $"/{nameof(PatchableObject.MyDateTime)}",
            null,
            null));

        // Act
        var result1 = _dut.AllRequiredFieldsHaveValue(_patchDocument.Operations);
        var result2 = _dut.GetMessageForRequiredFields(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result1);
        Assert.IsNotNull(result2);
        Assert.IsTrue(result2.EndsWith("MyDateTime"));
    }

    [TestMethod]
    public void AllRequiredFieldsHaveValue_ShouldReturnFalse_WhenTryingToSetNullInRequiredGuid()
    {
        // Arrange
        _patchDocument.Operations.Clear();
        // cheat to be able to set null to a non-nullable 
        _patchDocument.Operations.Add(new Operation<PatchableObject>(
            "replace",
            $"/{nameof(PatchableObject.MyGuid)}",
            null,
            null));

        // Act
        var result1 = _dut.AllRequiredFieldsHaveValue(_patchDocument.Operations);
        var result2 = _dut.GetMessageForRequiredFields(_patchDocument.Operations);

        // Assert
        Assert.IsFalse(result1);
        Assert.IsNotNull(result2);
        Assert.IsTrue(result2.EndsWith("MyGuid"));
    }

    #endregion
}
