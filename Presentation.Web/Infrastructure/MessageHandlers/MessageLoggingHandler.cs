using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Presentation.Web.Infrastructure.MessageHandlers
{
    public class MessageLoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var corrId = request.GetCorrelationId();

            var requestMessage = await request.Content.ReadAsStringAsync();

            await Task.Run(() => Log.Logger.Debug("{CorrId} - Request: {Method} {RawUrl}\r\n{Body}", corrId, request.Method, request.RequestUri, requestMessage));

            var response = await base.SendAsync(request, cancellationToken);

            string responseMessage;

            if (response.IsSuccessStatusCode)
                responseMessage = await response.Content.ReadAsStringAsync();
            else
                responseMessage = response.ReasonPhrase;

            await Task.Run(() => Log.Logger.Debug("{CorrId} - Response: {Method} {RawUrl}\r\n{Body}", corrId, request.Method, request.RequestUri, responseMessage, response.StatusCode));

            return response;
        }
    }
}