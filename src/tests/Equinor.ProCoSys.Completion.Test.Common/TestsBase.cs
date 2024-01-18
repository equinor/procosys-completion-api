using System;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Test.Common
{
    [TestClass]
    public abstract class TestsBase
    {
        protected static string TestPlantA = "PCS$PlantA";
        protected static string TestPlantB = "PCS$PlantB";
        protected static string TestPlantWithoutData = "PCS$EmptyPlant";

        protected IUnitOfWork _unitOfWorkMock;
        protected IPlantProvider _plantProviderMock;
        protected ManualTimeProvider _timeProvider;
        protected DateTime _utcNow;

        [TestInitialize]
        public void BaseSetup()
        {
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _plantProviderMock = Substitute.For<IPlantProvider>();
            _plantProviderMock.Plant.Returns(TestPlantA);
            
            _utcNow = new DateTime(2020, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            _timeProvider = new ManualTimeProvider(_utcNow);
            TimeService.SetProvider(_timeProvider);
        }

        protected void AssertPerson(IProperty property, User value)
        {
            Assert.IsNotNull(property);
            var user = property.Value as User;
            Assert.IsNotNull(user);
            Assert.AreEqual(value.Oid, user.Oid);
            Assert.AreEqual(value.FullName, user.FullName);
        }

        protected void AssertProperty(IProperty property, object value)
        {
            Assert.IsNotNull(property);
            Assert.IsNotNull(value);
            Assert.AreEqual(value, property.Value);
        }

        protected void AssertChange(IChangedProperty change, object oldValue, object newValue)
        {
            Assert.IsNotNull(change);
            Assert.AreEqual(oldValue, change.OldValue);
            Assert.AreEqual(newValue, change.NewValue);
        }

        protected void AssertPersonChange(IChangedProperty change, User oldValue, User newValue)
        {
            Assert.IsNotNull(change);
            if (change.OldValue is null)
            {
                Assert.IsNull(oldValue);
            }
            else
            {
                var user = change.OldValue as User;
                Assert.IsNotNull(user);
                Assert.AreEqual(oldValue.Oid, user.Oid);
                Assert.AreEqual(oldValue.FullName, user.FullName);
            }
            if (change.NewValue is null)
            {
                Assert.IsNull(newValue);
            }
            else
            {
                var user = change.NewValue as User;
                Assert.IsNotNull(user);
                Assert.AreEqual(newValue.Oid, user.Oid);
                Assert.AreEqual(newValue.FullName, user.FullName);
            }
        }
    }
}
