using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests;

[TestClass]
public abstract class EntityWithGuidRepositoryTestBase<TEntity> where TEntity: EntityBase, IAggregateRoot, IHaveGuid
{
    protected const string TestPlant = "PCS$TESTPLANT";
    protected ContextHelper _contextHelper;
    protected EntityWithGuidRepository<TEntity> _dut;
    protected DbSet<TEntity> _dbSetMock;

    protected int _knownId = 5;
    protected Guid _knownGuid;

    [TestInitialize]
    public void SetupBase()
    {
        _contextHelper = new ContextHelper();
        
        SetupRepositoryWithOneKnownItem();
    }

    protected abstract void SetupRepositoryWithOneKnownItem();
    protected abstract TEntity GetNewEntity();

    [TestMethod]
    public async Task GetAll_ShouldReturnTheKnownItem()
    {
        var result = await _dut.GetAllAsync();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(_knownId, result.ElementAt(0).Id);
    }

    [TestMethod]
    public async Task GetByIds_UnknownId_ShouldReturnEmptyList()
    {
        var result = await _dut.GetByIdsAsync(new List<int> { 0 });

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task Exists_KnownId_ShouldReturnTrue()
    {
        var result = await _dut.Exists(_knownId);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Exists_UnknownId_ShouldReturnFalse()
    {
        var result = await _dut.Exists(1234);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GetById_KnownId_ShouldReturnEntity()
    {
        var result = await _dut.GetByIdAsync(_knownId);

        Assert.IsNotNull(result);
        Assert.AreEqual(_knownId, result.Id);
    }

    [TestMethod]
    public async Task GetById_UnknownId_ShouldReturnNull()
    {
        var result = await _dut.GetByIdAsync(1234);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Add_Entity_ShouldCallAdd()
    {
        var entityToAdd = GetNewEntity();
        _dut.Add(entityToAdd);

        _dbSetMock.Received().Add(entityToAdd);
    }

    [TestMethod]
    public async Task GetByGuid_KnownGuid_ShouldReturnEntity()
    {
        var result = await _dut.GetByGuidAsync(_knownGuid);

        Assert.IsNotNull(result);
        Assert.AreEqual(_knownGuid, result.Guid);
    }

    [TestMethod]
    public async Task GetByGuid_UnknownGuid_ShouldReturnNull()
    {
        var result = await _dut.GetByGuidAsync(Guid.Empty);

        Assert.IsNull(result);
    }
}
