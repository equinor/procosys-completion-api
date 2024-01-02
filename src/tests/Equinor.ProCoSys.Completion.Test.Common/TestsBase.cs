using System;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain;
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
    }
}
