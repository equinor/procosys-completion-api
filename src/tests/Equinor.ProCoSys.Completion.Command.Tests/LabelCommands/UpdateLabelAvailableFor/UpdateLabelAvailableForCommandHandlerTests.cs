using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.LabelCommands.UpdateLabelAvailableFor;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.LabelCommands.UpdateLabelAvailableFor;

[TestClass]
public class UpdateLabelAvailableForCommandHandlerTests
{
    protected IUnitOfWork _unitOfWorkMock;
    private ILabelRepository _labelRepositoryMock;
    private ILabelEntityRepository _labelEntityRepositoryMock;

    private UpdateLabelAvailableForCommandHandler _dut;
    private UpdateLabelAvailableForCommand _command;

    private Label _labelWithoutUsage;
    private Label _labelForPunchPicture;
    private readonly string _text = "A";
    private readonly EntityTypeWithLabel _oldEntityTypeWithLabel = EntityTypeWithLabel.PunchPicture;
    private readonly EntityTypeWithLabel _newEntityTypeWithLabel = EntityTypeWithLabel.PunchComment;

    [TestInitialize]
    public void Setup()
    {
        var newLabelEntity = new LabelEntity(_newEntityTypeWithLabel);
        var oldLabelEntity = new LabelEntity(_oldEntityTypeWithLabel);

        _labelWithoutUsage = new Label(_text);
        _labelForPunchPicture = new Label(_text);
        _labelForPunchPicture.MakeLabelAvailableFor(oldLabelEntity);

        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _labelRepositoryMock = Substitute.For<ILabelRepository>();
        _labelRepositoryMock.GetByTextAsync(_text, default).Returns(_labelWithoutUsage);

        _labelEntityRepositoryMock = Substitute.For<ILabelEntityRepository>();

        _labelEntityRepositoryMock.GetByTypeAsync(_newEntityTypeWithLabel, default).Returns(newLabelEntity);
        
        _command = new UpdateLabelAvailableForCommand(_text, new List<EntityTypeWithLabel> { _newEntityTypeWithLabel });

        _dut = new UpdateLabelAvailableForCommandHandler(
            _labelRepositoryMock,
            _labelEntityRepositoryMock,
            _unitOfWorkMock,
            Substitute.For<ILogger<UpdateLabelAvailableForCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldMakeLabelAvailableForEntity_WhenNoAvailableBefore()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(1, _labelWithoutUsage.AvailableFor.Count);
        Assert.AreEqual(_newEntityTypeWithLabel, _labelWithoutUsage.AvailableFor.ElementAt(0).EntityType);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldReplaceLabelAvailableForEntity_WhenSomeAvailableBefore()
    {
        // Arrange
        _labelRepositoryMock.GetByTextAsync(_text, default).Returns(_labelForPunchPicture);
        Assert.AreEqual(1, _labelForPunchPicture.AvailableFor.Count);
        Assert.AreEqual(_oldEntityTypeWithLabel, _labelForPunchPicture.AvailableFor.ElementAt(0).EntityType);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(0, _labelWithoutUsage.AvailableFor.Count);
        Assert.AreEqual(_newEntityTypeWithLabel, _labelForPunchPicture.AvailableFor.ElementAt(0).EntityType);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldRemoveLabelAvailableForEntity_WhenSomeAvailableBefore()
    {
        // Arrange
        _labelRepositoryMock.GetByTextAsync(_text, default).Returns(_labelForPunchPicture);
        Assert.AreEqual(1, _labelForPunchPicture.AvailableFor.Count);
        Assert.AreEqual(_oldEntityTypeWithLabel, _labelForPunchPicture.AvailableFor.ElementAt(0).EntityType);

        // Act
        await _dut.Handle(new UpdateLabelAvailableForCommand(_text, new List<EntityTypeWithLabel>()), default);

        // Assert
        Assert.AreEqual(0, _labelWithoutUsage.AvailableFor.Count);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }
}
