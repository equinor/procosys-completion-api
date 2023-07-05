using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests;

public class ContextHelper
{
    public ContextHelper()
    {
        DbOptions = new DbContextOptions<CompletionContext>();
        EventDispatcherMock = new Mock<IEventDispatcher>();
        PlantProviderMock = new Mock<IPlantProvider>();
        CurrentUserProviderMock = new Mock<ICurrentUserProvider>();
        ContextMock = new Mock<CompletionContext>(
            DbOptions,
            PlantProviderMock.Object,
            EventDispatcherMock.Object,
            CurrentUserProviderMock.Object);
    }

    public DbContextOptions<CompletionContext> DbOptions { get; }
    public Mock<IEventDispatcher> EventDispatcherMock { get; }
    public Mock<IPlantProvider> PlantProviderMock { get; }
    public Mock<CompletionContext> ContextMock { get; }
    public Mock<ICurrentUserProvider> CurrentUserProviderMock { get; }
}