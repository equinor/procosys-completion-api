namespace Equinor.ProCoSys.Completion.Domain;

public readonly record struct Optional<T>
{
    public T Value { get; private init; } = default!;
    public bool HasValue { get; private init; }
    
    public Optional() => HasValue = false;

    public Optional(T value)
    {
        Value = value;
        HasValue = true;
    }
}
