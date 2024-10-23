using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.HostedServices;

public class CheckMovedCheckLists(
    IServiceScopeFactory serviceScopeFactory,
    IOptionsMonitor<ApplicationOptions> applicationOptions,
    ILogger<CheckMovedCheckLists> logger)
    : IHostedService
{
    private readonly Guid _completionApiObjectId = applicationOptions.CurrentValue.ObjectId;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        await CheckCheckListsAsync(scope, cancellationToken);

        //await SaveAsync(scope, cancellationToken);
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

    private async Task CheckCheckListsAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        var projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        var punchItemRepository = scope.ServiceProvider.GetRequiredService<IPunchItemRepository>();
        var checkListApiService = scope.ServiceProvider.GetRequiredService<ICheckListApiService>();

        var checkListGuids = await punchItemRepository.GetAllUniqueCheckListGuidsAsync(cancellationToken);
        logger.LogInformation("Found {UniqueCheckListGuidCount} unique checklist guids", checkListGuids.Count);

        var page = 0;
        var pageSize = 200;
        var totalPages = (int)Math.Ceiling((double)checkListGuids.Count / pageSize);
        List<Guid> pageCheckListGuids;
        do
        {
            pageCheckListGuids = checkListGuids.Skip(pageSize * page).Take(pageSize).ToList();

            if (pageCheckListGuids.Any())
            {
                var checkListsPage
                    = await checkListApiService.GetManyCheckListsAsync(pageCheckListGuids, cancellationToken);

                logger.LogInformation("Checking page {Page} of {TotalPages} of checklist guids", page+1, totalPages);
                await CheckPunchItemsAsync(projectRepository, punchItemRepository, checkListsPage, cancellationToken);
            }

            page++;


        } while (pageCheckListGuids.Count == pageSize);
    }

    private async Task CheckPunchItemsAsync(
        IProjectRepository projectRepository, 
        IPunchItemRepository punchItemRepository, 
        List<ProCoSys4CheckList> checkLists, 
        CancellationToken cancellationToken)
    {
        var checkListGuids = checkLists.Select(c => c.CheckListGuid).ToList();
        var punchItems = await punchItemRepository.GetByCheckListGuidsAsync(checkListGuids, cancellationToken);

        foreach (var checkList in checkLists)
        {
            var punchItemWithWrongProject = punchItems.Where(p => p.Project.Guid != checkList.ProjectGuid).ToList();

            if (punchItemWithWrongProject.Count > 0)
            {
                var correctProject = await projectRepository.GetAsync(checkList.ProjectGuid, cancellationToken);
                var punchItemNosWithWrongProject = punchItemWithWrongProject.Select(p => p.ItemNo.ToString());
                var wrongProjects = punchItemWithWrongProject.Select(p => p.Project.Name);
                logger.LogWarning(
                    "Found {PunchItemWithWrongProjectCount} punchItems with wrong project. " + 
                    "ItemNos: {PunchItemNosWithWrongProject}. " +
                    "Wrong projects: {WrongProjects}. " +
                    "Correct project: {CorrectProject}. " +
                    "Correct project guid: {CorrectProjectGuid}",
                    punchItemWithWrongProject.Count,
                    string.Join(',', punchItemNosWithWrongProject),
                    string.Join(',', wrongProjects),
                    correctProject.Name,
                    correctProject.Guid);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
