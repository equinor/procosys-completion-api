using System;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Test.Common;

[TestClass]
public abstract class TestsBase
{
    protected readonly string TestPlantA = "PCS$PlantA";
    protected Mock<IUnitOfWork> _unitOfWorkMock;
    protected Mock<IPlantProvider> _plantProviderMock;
    protected ManualTimeProvider _timeProvider;
    protected DateTime _utcNow;

    [TestInitialize]
    public void BaseSetup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _plantProviderMock = new Mock<IPlantProvider>();
        _plantProviderMock
            .Setup(x => x.Plant)
            .Returns(TestPlantA);
        _utcNow = new DateTime(2020, 1, 1, 1, 1, 1, DateTimeKind.Utc);
        _timeProvider = new ManualTimeProvider(_utcNow);
        TimeService.SetProvider(_timeProvider);
    }
}
