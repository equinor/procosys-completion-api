using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;
using Equinor.ProCoSys.Completion.WebApi.Authorizations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Behaviors;

public class CheckAccessBehavior<TRequest, TResponse>(
    ILogger<CheckAccessBehavior<TRequest, TResponse>> logger,
    IAccessValidator accessValidator)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var typeName = request.GetGenericTypeName();

        logger.LogInformation("----- Checking access for {TypeName}", typeName);

        if (request is IImportCommand)
        {
            //Skip auth if import command
            return await next();
        }

        if (!accessValidator.HasAccess(request as IBaseRequest))
        {
            logger.LogWarning("User do not have access - {TypeName}", typeName);

            throw new UnauthorizedAccessException();
        }

        return await next();
    }
}
