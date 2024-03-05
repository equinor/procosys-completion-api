using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemAttachments;

[TestClass]
public class GetPunchItemAttachmentsQueryValidatorTests
{
    private GetPunchItemAttachmentsQueryValidator _dut;
    private GetPunchItemAttachmentsQuery _query;
    private IPunchItemValidator _punchItemValidatorMock;

    [TestInitialize]
    public void Setup_OkState()
    {
        _query = new GetPunchItemAttachmentsQuery(Guid.NewGuid(), null, null);
        _punchItemValidatorMock = Substitute.For<IPunchItemValidator>();
        _punchItemValidatorMock.ExistsAsync(_query.PunchItemGuid, default).Returns(true);

        _dut = new GetPunchItemAttachmentsQueryValidator(_punchItemValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemNotExists()
    {
        // Arrange
        _punchItemValidatorMock.ExistsAsync(_query.PunchItemGuid, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        var validationFailure = result.Errors[0];
        Assert.IsTrue(validationFailure.ErrorMessage.StartsWith("Punch item with this guid does not exist!"));
        Assert.IsInstanceOfType(validationFailure.CustomState, typeof(EntityNotFoundException));
    }
}
