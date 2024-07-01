using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemsByCheckList;

[TestClass]
public class GetPunchItemsByCheckListGuidQueryValidatorTests
{
    private GetPunchItemsByCheckListGuidQueryValidator _dut;

    [TestInitialize]
    public void Setup_OkState() => _dut = new GetPunchItemsByCheckListGuidQueryValidator();

    [TestMethod]
    public async Task Validate_ShouldBeValid_When_Valid_Guid()
    {
        // Arrange
        var query = new GetPunchItemsByCheckListGuidQuery(Guid.NewGuid());

        // Act
        var result = await _dut.ValidateAsync(query);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_Guid_Is_Empty()
    {
        // Arrange
        var query = new GetPunchItemsByCheckListGuidQuery(Guid.Empty);

        // Act
        var result = await _dut.ValidateAsync(query);

        // Assert
        Assert.IsFalse(result.IsValid);
    }
}

