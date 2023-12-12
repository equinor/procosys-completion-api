using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.WebApi.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Middleware;

public class VerifyLabelEntitiesExists : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<VerifyLabelEntitiesExists> _logger;
    private readonly Guid _completionApiObjectId;

    public VerifyLabelEntitiesExists(
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<CompletionAuthenticatorOptions> options,
        ILogger<VerifyLabelEntitiesExists> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _completionApiObjectId = options.CurrentValue.CompletionApiObjectId;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var labelEntityRepository =
            scope.ServiceProvider
                .GetRequiredService<ILabelEntityRepository>();

        var nonExistingEntityTypes = await GetNonExistingEntityTypesAsync(labelEntityRepository, cancellationToken);
        if (!nonExistingEntityTypes.Any())
        {
            return;
        }

        await CreateNonExistingEntityTypesAsync(scope, nonExistingEntityTypes, labelEntityRepository, cancellationToken);
    }

    private async Task CreateNonExistingEntityTypesAsync(
        IServiceScope scope,
        List<EntityTypeWithLabel> nonExistingEntityTypes,
        ILabelEntityRepository labelEntityRepository,
        CancellationToken cancellationToken)
    {
        var unitOfWork =
            scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

        var currentUserSetter =
            scope.ServiceProvider
                .GetRequiredService<ICurrentUserSetter>();
        currentUserSetter.SetCurrentUserOid(_completionApiObjectId);

        foreach (var entityType in nonExistingEntityTypes)
        {
            labelEntityRepository.Add(new LabelEntity(entityType));
            _logger.LogInformation("Label entity {LabelEntity} added", entityType.ToString());
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
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

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
