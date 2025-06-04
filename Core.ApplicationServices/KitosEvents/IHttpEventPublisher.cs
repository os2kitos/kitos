using System.Net.Http;
using Core.ApplicationServices.Model.KitosEvents;
using System.Threading.Tasks;
using Core.Abstractions.Types;

namespace Core.ApplicationServices.KitosEvents;

public interface IHttpEventPublisher
{
    Task<Result<HttpResponseMessage, OperationError>> PostEventAsync(KitosEventDTO eventDTO);
}