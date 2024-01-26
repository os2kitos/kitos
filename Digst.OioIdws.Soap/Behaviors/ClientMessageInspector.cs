using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Digst.OioIdws.Soap.Behaviors
{
    public class ClientMessageInspector : IClientMessageInspector
    {
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            HttpRequestMessageProperty property = new HttpRequestMessageProperty();
            property.Headers["Content-Type"] = "application/soap+xml; charset=utf-8; action=\"" + request.Headers.Action + "\";";
            request.Properties.Add(HttpRequestMessageProperty.Name, property);

            return null;
        }

        /// <summary>
        /// Enables inspection or modification of a message after a reply message is received but prior to passing it back to the client application.
        /// </summary>
        /// <param name="reply">The message to be transformed into types and handed back to the client application.</param>
        /// <param name="correlationState">Correlation state data.</param>
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            // Nothing special here
        }
    }
}
