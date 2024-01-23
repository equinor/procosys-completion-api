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
public abstract class RepositoryTestBase<TEntity> where TEntity : EntityBase, IAggregateRoot
{
    protected const string TestPlant = "PCS$TESTPLANT";
    protected ContextHelper _contextHelper;
    protected DbSet<TEntity> _dbSetMock;

    protected int _knownId = 5;

    [TestInitialize]
    public void SetupBase()
    {
        _contextHelper = new ContextHelper();

        SetupRepositoryWithOneKnownItem();
    }

    protected abstract EntityRepository<TEntity> GetDut();
    protected abstract void SetupRepositoryWithOneKnownItem();
    protected abstract TEntity GetNewEntity();

    [TestMethod]
    public async Task GetAll_ShouldReturnTheKnownItem()
    {
        var result = await GetDut().GetAllAsync(default);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(_knownId, result.ElementAt(0).Id);
    }

    [TestMethod]
    public async Task GetByIds_UnknownId_ShouldReturnEmptyList()
    {
        var result = await GetDut().GetByIdsAsync(new List<int> { 0 }, default);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task Exists_KnownId_ShouldReturnTrue()
    {
        var result = await GetDut().Exists(_knownId, default);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Exists_UnknownId_ShouldReturnFalse()
    {
        var result = await GetDut().Exists(1234, default);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GetById_KnownId_ShouldReturnEntity()
    {
        var result = await GetDut().GetByIdAsync(_knownId, default);

        Assert.IsNotNull(result);
        Assert.AreEqual(_knownId, result.Id);
    }

    [TestMethod]
    public async Task GetById_UnknownId_ShouldReturnNull()
    {
        var result = await GetDut().GetByIdAsync(1234, default);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Add_Entity_ShouldCallAdd()
    {
        var entityToAdd = GetNewEntity();
        GetDut().Add(entityToAdd);

        _dbSetMock.Received().Add(entityToAdd);
    }
}
