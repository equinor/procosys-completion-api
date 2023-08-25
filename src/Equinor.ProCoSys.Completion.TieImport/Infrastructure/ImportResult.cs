using Equinor.ProCoSys.Completion.TieImport.Extensions;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;
public class ImportResult
{
    private ImportResult(ResultLevel result) => Result = result;

    public ResultLevel Result { get; }

    public Exception? Exception { get; set; }

    public List<ImportMessage> Messages { get; } = new();

    public bool AbortProcessing { get; set; } = false;

    public static ImportResult Ok() => new(ResultLevel.Ok);

    public static ImportResult Warning() => new(ResultLevel.Warning);

    public static ImportResult Error() => new(ResultLevel.Error);

    public static ImportResult SingleWarning(string message) => new ImportResult(ResultLevel.Warning).Add(message);

    public static ImportResult SingleError(string message, string? details = null)
        => new ImportResult(ResultLevel.Error).Add(message, details);

    public ImportResult Add(string message, string? details = null)
    {
        Messages.Add(new ImportMessage { Message = message, Details = details });
        return this;
    }

    public ImportResult Combine(ImportResult second) => Combine(() => second);

    public ImportResult Combine(Func<ImportResult> second)
    {
        var other = second();
        if (other == null)
        {
            return this;
        }

        var mostSevere = Result >= other.Result ? this : other;
        var leastSevere = Result >= other.Result ? other : this;

        // Combine messages if same severity level
        if (mostSevere.Result == leastSevere.Result)
        {
            mostSevere.Messages.AddRange(leastSevere.Messages);
        }
        return mostSevere;
    }

    public void AppendTo(TIMessageResult result, TIObject tiObject)
    {
        switch (Result)
        {
            case ResultLevel.Ok:
                return;
            case ResultLevel.Warning:
                foreach (var importMessage in Messages)
                {
                    var entry = TILog.CreateWarningEntry(tiObject, importMessage.Message, importMessage.Details);
                    result.AddLogEntry(entry);
                }
                return;
            case ResultLevel.Error:
                result.Result = MessageResults.Failed;
                if (Exception != null)
                {
                    result.ErrorMessage = Exception.Message;
                    result.ErrorDetails = Exception.StackTrace;
                }

                if (string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    result.ErrorMessage = $"Errors(1 of {Messages.Count}): {tiObject.GetLogFriendlyStringForTieLog()} {Messages.FirstOrDefault()?.Message}";
                }

                foreach (var importMessage in Messages)
                {
                    var entry = TILog.CreateEntry(tiObject, importMessage.Message, importMessage.Details);
                    result.AddLogEntry(entry);
                }
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}
