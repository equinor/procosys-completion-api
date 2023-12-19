using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;

public class CreateLabelCommandHandler : IRequestHandler<CreateLabelCommand, Result<string>>
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

    public async Task<Result<string>> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
    {
        var label = new Label(request.Text);
        _labelRepository.Add(label);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Label {Label} created", request.Text);

        return new SuccessResult<string>(label.RowVersion.ConvertToString());
    }
}
