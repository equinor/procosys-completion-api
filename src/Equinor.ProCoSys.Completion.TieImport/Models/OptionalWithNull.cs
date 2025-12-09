using System.Globalization;
using static Equinor.ProCoSys.Completion.TieImport.PunchObjectAttributes;

namespace Equinor.ProCoSys.Completion.TieImport.Models;

/// <summary>
/// Represents an optional value that can also indicate if the value should be explicitly nulled/deleted in the database.
/// Used when parsing import messages where "{NULL}" means "set this field to null" vs not present means "don't change".
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public readonly record struct OptionalWithNull<T>
{
    /// <summary>
    /// The actual value (if present and not marked for deletion)
    /// </summary>
    public T? Value { get; private init; }
    
    /// <summary>
    /// Indicates if a value was provided in the import message
    /// </summary>
    public bool HasValue { get; private init; }
    
    /// <summary>
    /// Indicates if the value should be explicitly set to null/deleted in the database
    /// </summary>
    public bool ShouldBeNull { get; private init; }

    /// <summary>
    /// Creates an empty optional (field was not present in import message)
    /// </summary>
    public OptionalWithNull()
    {
        Value = default;
        HasValue = false;
        ShouldBeNull = false;
    }

    /// <summary>
    /// Creates an optional with a value
    /// </summary>
    public OptionalWithNull(T? value)
    {
        Value = value;
        HasValue = true;
        ShouldBeNull = false;
    }

    /// <summary>
    /// Creates an optional that marks the field should be nulled
    /// </summary>
    public static OptionalWithNull<T> CreateNull() => new()
    {
        Value = default,
        HasValue = true,
        ShouldBeNull = true
    };

    /// <summary>
    /// Parses a string value and creates the appropriate OptionalWithNull.
    /// If the string is "{NULL}", marks the field for deletion.
    /// </summary>
    public static OptionalWithNull<int?> ParseInt(string? value)
    {
        if (value == null)
        {
            return new OptionalWithNull<int?>();
        }

        var trimmedValue = value.Trim();

        if (trimmedValue == NullMarker)
        {
            return CreateNull<int?>();
        }

        if (int.TryParse(trimmedValue, out var intValue))
        {
            return new OptionalWithNull<int?>(intValue);
        }

        return new OptionalWithNull<int?>();
    }

    /// <summary>
    /// Parses a string value and creates the appropriate OptionalWithNull.
    /// If the string is "{NULL}", marks the field for deletion.
    /// </summary>
    public static OptionalWithNull<string?> ParseString(string? value)
    {
        if (value == null)
        {
            return new OptionalWithNull<string?>(null);
        }

        if (value == NullMarker)
        {
            return CreateNull<string?>();
        }

        return new OptionalWithNull<string?>(value);
    }

    /// <summary>
    /// Parses a date string value and creates the appropriate OptionalWithNull.
    /// If the string is "{NULL}", marks the field for deletion.
    /// </summary>
    public static OptionalWithNull<DateTime?> ParseDateTime(string? value)
    {
        if (value == null)
        {
            return new OptionalWithNull<DateTime?>();
        }

        if (value.Trim() == NullMarker)
        {
            return CreateNull<DateTime?>();
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return new OptionalWithNull<DateTime?>();
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateValue))
        {
            var utcDateTime = DateTime.SpecifyKind(dateValue, DateTimeKind.Utc);
            return new OptionalWithNull<DateTime?>(utcDateTime);
        }

        return new OptionalWithNull<DateTime?>();
    }

    private static OptionalWithNull<TValue> CreateNull<TValue>() => new()
    {
        Value = default,
        HasValue = true,
        ShouldBeNull = true
    };
}
