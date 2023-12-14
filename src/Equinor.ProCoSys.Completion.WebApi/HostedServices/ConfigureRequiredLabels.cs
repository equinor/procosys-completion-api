using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.WebApi.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.HostedServices;

public class ConfigureRequiredLabels : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ConfigureRequiredLabels> _logger;
    private readonly Guid _completionApiObjectId;
    private readonly string _rejectLabelText;

    public ConfigureRequiredLabels(
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<ApplicationOptions> applicationOptions,
        IOptionsMonitor<CompletionAuthenticatorOptions> completionAuthenticatorOptions,
        ILogger<ConfigureRequiredLabels> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _completionApiObjectId = completionAuthenticatorOptions.CurrentValue.CompletionApiObjectId;
        _rejectLabelText = applicationOptions.CurrentValue.RejectLabel;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        await ConfigureLabelEntities(scope, cancellationToken);

        await ConfigureLabels(scope, cancellationToken);

        await SaveAsync(scope, cancellationToken);
    }

    private async Task SaveAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var currentUserSetter =
            scope.ServiceProvider
                .GetRequiredService<ICurrentUserSetter>();
        var unitOfWork =
            scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

        currentUserSetter.SetCurrentUserOid(_completionApiObjectId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ConfigureLabelEntities(IServiceScope scope, CancellationToken cancellationToken)
    {
        var labelEntityRepository =
            scope.ServiceProvider
                .GetRequiredService<ILabelEntityRepository>();

        var nonExistingEntityTypes = await GetNonExistingEntityTypesAsync(labelEntityRepository, cancellationToken);
        if (!nonExistingEntityTypes.Any())
        {
            return;
        }

        CreateNonExistingEntityTypes(nonExistingEntityTypes, labelEntityRepository);
    }

    private void CreateNonExistingEntityTypes(
        List<EntityTypeWithLabel> nonExistingEntityTypes,
        ILabelEntityRepository labelEntityRepository)
    {
        foreach (var entityType in nonExistingEntityTypes)
        {
            labelEntityRepository.Add(new LabelEntity(entityType));
            _logger.LogInformation("Label entity {LabelEntity} added", entityType.ToString());
        }
    }

    private async Task<List<EntityTypeWithLabel>> GetNonExistingEntityTypesAsync(
        ILabelEntityRepository labelEntityRepository,
        CancellationToken cancellationToken)
    {
        var nonExistingLabelEntities = new List<EntityTypeWithLabel>();
        foreach (EntityTypeWithLabel entityType in Enum.GetValues(typeof(EntityTypeWithLabel)))
        {
            if (!await labelEntityRepository.ExistsAsync(entityType, cancellationToken))
            {
                nonExistingLabelEntities.Add(entityType);
            }
        }

        return nonExistingLabelEntities;
    }

    private async Task ConfigureLabels(IServiceScope scope, CancellationToken cancellationToken)
    {
        var labelRepository =
            scope.ServiceProvider
                .GetRequiredService<ILabelRepository>();

        var nonExistingLabels = await GetNonExistingLabelsAsync(labelRepository, cancellationToken);
        if (!nonExistingLabels.Any())
        {
            return;
        }

        CreateNonExistingLabels(nonExistingLabels, labelRepository);
    }

    private void CreateNonExistingLabels(List<string> nonExistingLabels, ILabelRepository labelRepository)
    {
        foreach (var nonExistingLabel in nonExistingLabels)
        {
            labelRepository.Add(new Label(nonExistingLabel));
            _logger.LogInformation("Label {Label} added", nonExistingLabel);
        }
    }

    private async Task<List<string>> GetNonExistingLabelsAsync(
        ILabelRepository labelRepository,
        CancellationToken cancellationToken)
    {
        var labelsToCheck = new List<string> { _rejectLabelText };
        var nonExistingLabels = new List<string>();

        foreach (var labelToCheck in labelsToCheck)
        {
            if (!await labelRepository.ExistsAsync(labelToCheck, cancellationToken))
            {
                nonExistingLabels.Add(labelToCheck);
            }
        }

        return nonExistingLabels;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
