using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class PersonRepositoryTests : EntityWithGuidRepositoryTestBase<Person>
{
    private readonly ICurrentUserProvider _userProviderMock = Substitute.For<ICurrentUserProvider>();

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var person = new Person(
            Guid.NewGuid(), 
            "FirstName",
            "LastName",
            "UNAME",
            "email@test.com");
        _knownGuid = person.Guid;
        person.SetProtectedIdForTesting(_knownId);

        var persons = new List<Person>
        {
            person
        };

        _dbSetMock = persons.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .Persons
            .Returns(_dbSetMock);

        _dut = new PersonRepository(_contextHelper.ContextMock, _userProviderMock);
    }

    protected override Person GetNewEntity() => new (Guid.NewGuid(), "New", "Person", "NP", "@");
}
