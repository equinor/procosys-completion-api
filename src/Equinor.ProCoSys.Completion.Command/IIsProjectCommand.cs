using MediatR;

namespace Equinor.ProCoSys.Completion.Command;

public interface IIsProjectCommand : IBaseRequest
{
    string ProjectName { get; }
}
