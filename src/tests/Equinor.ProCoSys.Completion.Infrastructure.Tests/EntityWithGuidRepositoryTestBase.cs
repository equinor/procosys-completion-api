﻿using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests;

[TestClass]
public abstract class EntityWithGuidRepositoryTestBase<TEntity> : RepositoryTestBase<TEntity> where TEntity: EntityBase, IAggregateRoot, IHaveGuid
{
    protected Guid _knownGuid;

    protected abstract override EntityWithGuidRepository<TEntity> GetDut();

    [TestMethod]
    public async Task GetAsync_KnownGuid_ShouldReturnEntity()
    {
        var result = await GetDut().GetAsync(_knownGuid, default);

        Assert.IsNotNull(result);
        Assert.AreEqual(_knownGuid, result.Guid);
    }

    [TestMethod]
    public async Task GetAsync_UnknownGuid_ShouldThrowEntityNotFoundException() // Act and Assert
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Assert
        await Assert.ThrowsExceptionAsync<EntityNotFoundException<TEntity>>(() => GetDut().GetAsync(guid, default));
    }

    [TestMethod]
    public async Task ExistsAsync_KnownGuid_ShouldReturnTrue()
    {
        var result = await GetDut().ExistsAsync(_knownGuid, default);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ExistsAsync_UnknownGuid_ShouldReturnFalse()
    {
        var result = await GetDut().ExistsAsync(Guid.Empty, default);

        Assert.IsFalse(result);
    }
}
