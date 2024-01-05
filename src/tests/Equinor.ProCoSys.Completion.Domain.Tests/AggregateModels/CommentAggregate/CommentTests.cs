using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Domain.Tests.AggregateModels.CommentAggregate;

[TestClass]
public class CommentTests : ICreationAuditableTests
{
    private Comment _dut;
    private readonly string _parentType = "X";
    private readonly Guid _parentGuid = Guid.NewGuid();
    private readonly string _text = "A";

    protected override ICreationAuditable GetCreationAuditable() => _dut;

    [TestInitialize]
    public void Setup() => _dut = new Comment(_parentType, _parentGuid, _text);

    [TestMethod]
    public void Constructor_ShouldSetProperties()
    {
        Assert.AreEqual(_text, _dut.Text);
        Assert.AreEqual(_parentType, _dut.ParentType);
        Assert.AreEqual(_parentGuid, _dut.ParentGuid);
        Assert.AreNotEqual(_parentGuid, _dut.Guid);
        Assert.AreNotEqual(Guid.Empty, _dut.Guid);
        Assert.IsNotNull(_dut.Labels);
        Assert.AreEqual(0, _dut.Labels.Count);
        Assert.IsNotNull(_dut.Mentions);
        Assert.AreEqual(0, _dut.Mentions.Count);
    }

    [TestMethod]
    public void SetMentions_ShouldSetMentions()
    {
        // Arrange
        var personA = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);
        var personB = new Person(Guid.NewGuid(), null!, null!, null!, null!, false);

        // Act
        _dut.SetMentions(new List<Person> { personA, personB });

        // Assert
        Assert.AreEqual(2, _dut.Mentions.Count);
        Assert.IsTrue(_dut.Mentions.Any(p => p.Guid == personA.Guid));
        Assert.IsTrue(_dut.Mentions.Any(p => p.Guid == personB.Guid));
    }

    [TestMethod]
    public void GetOrderedMentions_ShouldReturnOrderedMentions()
    {
        // Arrange
        var personA1 = new Person(Guid.NewGuid(), "A", "Aa1", null!, null!, false);
        var personA2 = new Person(Guid.NewGuid(), "A", "Aa2", null!, null!, false);
        var personB = new Person(Guid.NewGuid(), "B", "Bb", null!, null!, false);
        var personC = new Person(Guid.NewGuid(), "C", "Cc", null!, null!, false);
        _dut.SetMentions(new List<Person> { personB, personA2, personC, personA1 });

        // Act
        var result = _dut.GetOrderedMentions().ToList();
        
        // Assert
        Assert.AreEqual(4, result.Count);
        Assert.AreEqual(personA1.Guid, result.ElementAt(0).Guid);
        Assert.AreEqual(personA2.Guid, result.ElementAt(1).Guid);
        Assert.AreEqual(personB.Guid, result.ElementAt(2).Guid);
        Assert.AreEqual(personC.Guid, result.ElementAt(3).Guid);
    }
}
