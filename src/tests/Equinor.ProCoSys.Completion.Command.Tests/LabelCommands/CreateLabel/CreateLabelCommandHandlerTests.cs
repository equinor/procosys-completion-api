using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.LabelCommands.CreateLabel;

[TestClass]
public class CreateLabelCommandHandlerTests
{
    protected IUnitOfWork _unitOfWorkMock;
    private ILabelRepository _labelRepositoryMock;

    private Label _labelAddedToRepository;

    private CreateLabelCommandHandler _dut;
    private CreateLabelCommand _command;
    private readonly string _rowVersion = "AAAAAAAAABA=";

    [TestInitialize]
    public void Setup()
    {
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _labelRepositoryMock = Substitute.For<ILabelRepository>();
        _labelRepositoryMock
            .When(x => x.Add(Arg.Any<Label>()))
            .Do(callInfo =>
            {
                _labelAddedToRepository = callInfo.Arg<Label>();
                _labelAddedToRepository.SetRowVersion(_rowVersion);
            });

        _command = new CreateLabelCommand("A");

        _dut = new CreateLabelCommandHandler(
            _labelRepositoryMock,
            _unitOfWorkMock,
            Substitute.For<ILogger<CreateLabelCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldReturn_RowVersion()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(result.Data, typeof(string));
        Assert.AreEqual(_rowVersion, result.Data);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddCorrectLabel_ToLabelRepository()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNotNull(_labelAddedToRepository);
        Assert.AreEqual(_command.Text, _labelAddedToRepository.Text);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }
}
