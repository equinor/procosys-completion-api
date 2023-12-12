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
    private UpdateLabelAvailableForCommand _commandToMakeAvailableForPunchComment;

    private Label _labelNotAvailableToAnyEntityType;
    private Label _labelAvailableForPunchPicture;
    private readonly string _labelText = "A";
    private readonly EntityTypeWithLabel _punchPictureEntityType = EntityTypeWithLabel.PunchPicture;
    private readonly EntityTypeWithLabel _punchCommentEntityType = EntityTypeWithLabel.PunchComment;

    [TestInitialize]
    public void Setup()
    {
        var punchCommentLabelEntity = new LabelEntity(_punchCommentEntityType);
        var punchPictureLabelEntity = new LabelEntity(_punchPictureEntityType);

        _labelNotAvailableToAnyEntityType = new Label(_labelText);
        _labelAvailableForPunchPicture = new Label(_labelText);
        _labelAvailableForPunchPicture.MakeLabelAvailableFor(punchPictureLabelEntity);

        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _labelRepositoryMock = Substitute.For<ILabelRepository>();

        _labelEntityRepositoryMock = Substitute.For<ILabelEntityRepository>();

        _labelEntityRepositoryMock.GetByTypeAsync(_punchCommentEntityType, default).Returns(punchCommentLabelEntity);

        _commandToMakeAvailableForPunchComment =
            new UpdateLabelAvailableForCommand(_labelText, new List<EntityTypeWithLabel> { _punchCommentEntityType });

        _dut = new UpdateLabelAvailableForCommandHandler(
            _labelRepositoryMock,
            _labelEntityRepositoryMock,
            _unitOfWorkMock,
            Substitute.For<ILogger<UpdateLabelAvailableForCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldMakeLabelAvailableForEntity_WhenNotAvailableToAny()
    {
        // Arrange
        _labelRepositoryMock.GetByTextAsync(_labelText, default).Returns(_labelNotAvailableToAnyEntityType);
        Assert.AreEqual(0, _labelNotAvailableToAnyEntityType.AvailableFor.Count);

        // Act
        await _dut.Handle(_commandToMakeAvailableForPunchComment, default);

        // Assert
        Assert.AreEqual(1, _labelNotAvailableToAnyEntityType.AvailableFor.Count);
        Assert.AreEqual(_punchCommentEntityType, _labelNotAvailableToAnyEntityType.AvailableFor.ElementAt(0).EntityType);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldReplaceLabelAvailableForEntity_WhenAvailableToSome()
    {
        // Arrange
        _labelRepositoryMock.GetByTextAsync(_labelText, default).Returns(_labelAvailableForPunchPicture);
        Assert.AreEqual(1, _labelAvailableForPunchPicture.AvailableFor.Count);
        Assert.AreEqual(_punchPictureEntityType, _labelAvailableForPunchPicture.AvailableFor.ElementAt(0).EntityType);

        // Act
        await _dut.Handle(_commandToMakeAvailableForPunchComment, default);

        // Assert
        Assert.AreEqual(0, _labelNotAvailableToAnyEntityType.AvailableFor.Count);
        Assert.AreEqual(_punchCommentEntityType, _labelAvailableForPunchPicture.AvailableFor.ElementAt(0).EntityType);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldRemoveLabelAvailableForEntity_WhenAvailableToSome()
    {
        // Arrange
        _labelRepositoryMock.GetByTextAsync(_labelText, default).Returns(_labelAvailableForPunchPicture);
        Assert.AreEqual(1, _labelAvailableForPunchPicture.AvailableFor.Count);
        Assert.AreEqual(_punchPictureEntityType, _labelAvailableForPunchPicture.AvailableFor.ElementAt(0).EntityType);
        var commandToMakeAvailableForNone = new UpdateLabelAvailableForCommand(_labelText, new List<EntityTypeWithLabel>());

        // Act
        await _dut.Handle(commandToMakeAvailableForNone, default);

        // Assert
        Assert.AreEqual(0, _labelAvailableForPunchPicture.AvailableFor.Count);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Arrange
        _labelRepositoryMock.GetByTextAsync(_labelText, default).Returns(_labelNotAvailableToAnyEntityType);
        
        // Act
        await _dut.Handle(_commandToMakeAvailableForPunchComment, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
    }
}
