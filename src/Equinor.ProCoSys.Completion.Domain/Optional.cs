namespace Equinor.ProCoSys.Completion.Domain;

public sealed class Optional<T>
{
    public T Value { get; private set; } = default!;
    public bool HasValue { get; private set; }
    
    public Optional()
    {
        HasValue = false;
    }

    public Optional(T value)
    {
        Value = value;
        HasValue = true;
    }
}
