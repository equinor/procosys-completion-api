using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests;

public class ContextHelper
{
    public ContextHelper()
    {
        DbOptions = new DbContextOptions<CompletionContext>();
        EventDispatcherMock = Substitute.For<IEventDispatcher>();
        PlantProviderMock = Substitute.For<IPlantProvider>();
        CurrentUserProviderMock = Substitute.For<ICurrentUserProvider>();
        ContextMock = Substitute.For<CompletionContext>(
            DbOptions,
            PlantProviderMock,
            EventDispatcherMock,
            CurrentUserProviderMock);
    }

    private DbContextOptions<CompletionContext> DbOptions { get; }
    private IEventDispatcher EventDispatcherMock { get; }
    private IPlantProvider PlantProviderMock { get; }
    public CompletionContext ContextMock { get; }
    private ICurrentUserProvider CurrentUserProviderMock { get; }
}
