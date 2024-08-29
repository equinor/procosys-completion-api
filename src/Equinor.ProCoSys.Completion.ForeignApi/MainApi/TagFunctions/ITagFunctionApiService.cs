using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.TagFunctions;

public interface ITagFunctionApiService
{
    Task<List<ProCoSys4TagFunction>> GetAllAsync(string plant, CancellationToken cancellationToken);
}
