﻿using System;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations;

[TestClass]
public class AccessCheckerTests
{
    private AccessChecker _dut = null!;
    private IRestrictionRolesChecker _restrictionRolesCheckerMock = null!;
    private readonly CheckListDetailsDto _checkListDetailsDto = new(Guid.NewGuid(), "R", false, Guid.NewGuid());
    private readonly CheckListDetailsDto _checkListDetailsDto2 = new(Guid.NewGuid(), "R", false, Guid.NewGuid());

    [TestInitialize]
    public void Setup()
    {
        _restrictionRolesCheckerMock = Substitute.For<IRestrictionRolesChecker>();

        _dut = new AccessChecker(_restrictionRolesCheckerMock);
    }

    [TestMethod]
    public void HasCurrentUserWriteAccessToCheckList_ShouldReturnTrue_WhenUserHasNoRestrictions()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(true);

        // Act
        var result = _dut.HasCurrentUserWriteAccessToCheckList(_checkListDetailsDto);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasCurrentUserWriteAccessToAllCheckLists_ShouldReturnTrue_WhenUserHasNoRestrictions()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(true);

        // Act
        var result = _dut.HasCurrentUserWriteAccessToAllCheckLists([_checkListDetailsDto]);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasCurrentUserWriteAccessToCheckList_ShouldReturnTrue_WhenUserHasAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_checkListDetailsDto.ResponsibleCode).Returns(true);

        // Act
        var result = _dut.HasCurrentUserWriteAccessToCheckList(_checkListDetailsDto);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasCurrentUserWriteAccessToAllCheckLists_ShouldReturnTrue_WhenUserHasAccessToAllCheckLists()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_checkListDetailsDto.ResponsibleCode).Returns(true);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_checkListDetailsDto2.ResponsibleCode).Returns(true);

        // Act
        var result = _dut.HasCurrentUserWriteAccessToAllCheckLists([_checkListDetailsDto, _checkListDetailsDto2]);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasCurrentUserWriteAccessToCheckList_ShouldReturnFalse_WhenUserDoNotHaveAccessToCheckList()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_checkListDetailsDto.ResponsibleCode).Returns(false);

        // Act
        var result = _dut.HasCurrentUserWriteAccessToCheckList(_checkListDetailsDto);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasCurrentUserWriteAccessToAllCheckLists_ShouldReturnFalse_WhenUserDoNotHaveAccessToAllCheckLists()
    {
        // Arrange
        _restrictionRolesCheckerMock.HasCurrentUserExplicitNoRestrictions().Returns(false);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_checkListDetailsDto.ResponsibleCode).Returns(true);
        _restrictionRolesCheckerMock.HasCurrentUserExplicitAccessToContent(_checkListDetailsDto2.ResponsibleCode).Returns(false);

        // Act
        var result = _dut.HasCurrentUserWriteAccessToAllCheckLists([_checkListDetailsDto, _checkListDetailsDto2]);

        // Assert
        Assert.IsFalse(result);
    }
}
