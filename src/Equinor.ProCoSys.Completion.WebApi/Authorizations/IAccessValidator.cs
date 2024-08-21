using MediatR;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public interface IAccessValidator
{
    bool HasAccess<TRequest>(TRequest request) where TRequest: IBaseRequest;
}
