using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Misc;

[TestClass]
public class TypeExtensionTests
{
    [TestMethod]
    public void GetPropertiesWithAttribute_Should_GetPropertiesWithAttribute()
    {
        // Act
        var props = typeof(MyClass).GetPropertiesWithAttribute(typeof(StringLengthAttribute));

        // Arrange
        var propList = props.ToList();
        Assert.AreEqual(1, propList.Count);
        Assert.AreEqual("S2", propList.ElementAt(0).Name);
    }

    [TestMethod]
    public void GetWritablePropertiesWithStringLengthAttribute_Should_GetWritablePropertiesWithStringLengthAttribute()
    {
        // Act
        var props = typeof(MyClass).GetWritablePropertiesWithStringLengthAttribute();

        // Arrange
        var propList = props.ToList();
        Assert.AreEqual(1, propList.Count);
        Assert.AreEqual("S2", propList.ElementAt(0).Name);
    }

    [TestMethod]
    public void GetWritableProperties_Should_GetWritableProperties()
    {
        // Act
        var props = typeof(MyClass).GetWritableProperties();

        // Arrange
        var propList = props.ToList();
        Assert.AreEqual(2, propList.Count);
        Assert.IsTrue(propList.Any(p => p.Name == "S1"));
        Assert.IsTrue(propList.Any(p => p.Name == "S2"));
    }

    [TestMethod]
    public void HasBaseClassOfType_ShouldReturnTrue_WhenHasBaseClass()
    {
        // Act
        var hasBaseClass = typeof(MyClass).HasBaseClassOfType(typeof(MyBaseClass));

        // Arrange
        Assert.IsTrue(hasBaseClass);
    }

    [TestMethod]
    public void HasBaseClassOfType_ShouldReturnFalse_WhenDontHaveBaseClass()
    {
        // Act
        var hasBaseClass = typeof(MyBaseClass).HasBaseClassOfType(typeof(MyClass));

        // Arrange
        Assert.IsFalse(hasBaseClass);
    }

    class MyBaseClass
    {}

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    class MyClass : MyBaseClass
    {
        private string S0 { get; } = "S0";
        public string? S1 { get; set; }
        [StringLength(1)]
        public string? S2 { get; set; }

        public string S3 => S0;
    }
}
