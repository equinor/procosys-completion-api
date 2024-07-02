using System;
using Equinor.ProCoSys.Completion.Domain;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetCheckListsByPunchItemGuid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetCheckListsByPunchItemGuid;

[TestClass]
public class GetCheckListsByPunchItemGuidQueryValidatorTests
{
    private GetCheckListsByPunchItemGuidQueryValidator _dut;
    private GetCheckListsByPunchItemGuidQuery _query;
    private IPunchItemValidator _punchItemValidatorMock;


    [TestInitialize]
    public void Setup_OkState()
    {
        _query = new GetCheckListsByPunchItemGuidQuery(Guid.NewGuid());

        _punchItemValidatorMock = Substitute.For<IPunchItemValidator>();
        _punchItemValidatorMock.ExistsAsync(_query.PunchItemGuid, default).Returns(true);

        _dut = new GetCheckListsByPunchItemGuidQueryValidator(_punchItemValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_When_Valid_Guid()
    {
        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_CheckListNotExists()
    {
        // Arrange
        _punchItemValidatorMock.ExistsAsync(_query.PunchItemGuid, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        var validationFailure = result.Errors[0];
        Assert.IsTrue(validationFailure.ErrorMessage.StartsWith("Punch item with this guid does not exist"));
        Assert.IsInstanceOfType(validationFailure.CustomState, typeof(EntityNotFoundException));
    }
}

