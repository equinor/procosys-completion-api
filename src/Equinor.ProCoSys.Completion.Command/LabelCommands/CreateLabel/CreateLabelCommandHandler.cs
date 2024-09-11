using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;

public class CreateLabelCommandHandler : IRequestHandler<CreateLabelCommand, string>
{
    private readonly ILabelRepository _labelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateLabelCommandHandler> _logger;

    public CreateLabelCommandHandler(ILabelRepository labelRepository, IUnitOfWork unitOfWork, ILogger<CreateLabelCommandHandler> logger)
    {
        _labelRepository = labelRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
    {
        var label = new Label(request.Text);
        _labelRepository.Add(label);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Label {Label} created", request.Text);

        return label.RowVersion.ConvertToString();
    }
}
