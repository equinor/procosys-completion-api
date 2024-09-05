namespace Equinor.ProCoSys.Completion.Domain;

/// <summary>
/// Used with import updates to know if the value was present in the xml or not.
/// if the value was present, but blank we shall update the value to blank.
/// however if the attribute is not present, we shall not update it. 
/// </summary>
/// <typeparam name="T"></typeparam>
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
