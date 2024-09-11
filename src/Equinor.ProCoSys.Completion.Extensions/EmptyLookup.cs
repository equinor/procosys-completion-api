namespace Equinor.ProCoSys.Completion.Extensions;

public static class EmptyLookup<TKey, TItem>
{
    private static readonly ILookup<TKey, TItem> s_instance
        = Enumerable.Empty<TItem>().ToLookup(x => default(TKey))!;

    public static ILookup<TKey, TItem> Empty() => s_instance;
}
