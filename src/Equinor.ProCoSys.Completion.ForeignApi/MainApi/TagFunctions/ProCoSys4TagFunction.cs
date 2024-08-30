namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.TagFunctions;

public record ProCoSys4TagFunction(
    string Code,
    string Description,
    string RegisterCode,
    string RegisterDescription)
{
    public override string ToString() => $"{RegisterCode}/{Code}";
}
