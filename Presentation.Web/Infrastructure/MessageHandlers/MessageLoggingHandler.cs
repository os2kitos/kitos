using System;
using System.Net.Http;
using System.Text;
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

            var requestMessage = Encoding.UTF8.GetString(await request.Content.ReadAsByteArrayAsync());

            await Task.Run(() => Log.Logger.Debug("{corrId} - Request: {requestMethod} {requestUri}\r\n{body}", corrId, request.Method, request.RequestUri, requestMessage));

            var response = await base.SendAsync(request, cancellationToken);

            string responseMessage;

            if (response.IsSuccessStatusCode)
                responseMessage = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());
            else
                responseMessage = response.ReasonPhrase;

            await Task.Run(() => Log.Logger.Debug("{corrId} - Response: {requestMethod} {requestUri}\r\n{body}", corrId, request.Method, request.RequestUri, responseMessage));

            return response;
        }
    }
}