using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PersonCommands.CreatePerson;
using Equinor.ProCoSys.Completion.WebApi.Authentication;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.HostedServices;

/// <summary>
/// Ensure that ObjectId (i.e. the application) exists as Person.
/// Needed when application modifies data, setting ModifiedById for changed records
/// </summary>
public class VerifyApplicationExistsAsPerson : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptionsMonitor<AzureAdOptions> _azureAdOptions;
    private readonly ILogger<VerifyApplicationExistsAsPerson> _logger;

    public VerifyApplicationExistsAsPerson(
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<AzureAdOptions> azureAdOptions, 
        ILogger<VerifyApplicationExistsAsPerson> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _azureAdOptions = azureAdOptions;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var mediator =
            scope.ServiceProvider
                .GetRequiredService<IMediator>();
        var currentUserSetter =
            scope.ServiceProvider
                .GetRequiredService<ICurrentUserSetter>();

        var oid = _azureAdOptions.CurrentValue.ObjectId;
        _logger.LogInformation("Ensuring {Oid} exists as Person", oid);
        try
        {
            currentUserSetter.SetCurrentUserOid(oid);
            await mediator.Send(new CreatePersonCommand(oid), cancellationToken);
            _logger.LogInformation("{Oid} ensured", oid);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception handling {nameof(CreatePersonCommand)}");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
