using System;
using System.IO;
using System.Text.Json.Serialization;
using Equinor.ProCoSys.Completion.Domain;

namespace Equinor.ProCoSys.Completion.Command;

public abstract class UploadAttachmentCommand(Stream content)
    : CanHaveRestrictionsViaCheckList, IDisposable
{
    private bool _isDisposed;

    // JsonIgnore needed here so GlobalExceptionHandler do not try to serialize the Stream when reporting validation errors. 
    [JsonIgnore]
    public Stream Content { get; } = content;

    public void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            Content.Dispose();
        }
       
        _isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
