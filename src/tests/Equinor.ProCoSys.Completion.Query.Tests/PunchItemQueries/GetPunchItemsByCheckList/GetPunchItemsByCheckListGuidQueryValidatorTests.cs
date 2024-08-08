using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemsByCheckList;

[TestClass]
public class GetPunchItemsByCheckListGuidQueryValidatorTests
{
    private GetPunchItemsByCheckListGuidQueryValidator _dut;
    private GetPunchItemsByCheckListGuidQuery _query;
    private ICheckListValidator _checkListValidatorMock;


    [TestInitialize]
    public void Setup_OkState()
    {
        _query = new GetPunchItemsByCheckListGuidQuery(Guid.NewGuid(), PunchListStatusFilter.All);

        _checkListValidatorMock = Substitute.For<ICheckListValidator>();
        _checkListValidatorMock.ExistsAsync(_query.CheckListGuid, default).Returns(true);

        _dut = new GetPunchItemsByCheckListGuidQueryValidator(_checkListValidatorMock);
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
        _checkListValidatorMock.ExistsAsync(_query.CheckListGuid, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        var validationFailure = result.Errors[0];
        Assert.IsTrue(validationFailure.ErrorMessage.StartsWith("CheckList does not exist with Guid"));
        Assert.IsInstanceOfType(validationFailure.CustomState, typeof(EntityNotFoundException));
    }
}

