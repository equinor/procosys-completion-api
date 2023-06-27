using MassTransit;

namespace Equinor.ProCoSys.Completion.Command.MassTransit;

public class TopicNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>() => typeof(T).Name;
}

