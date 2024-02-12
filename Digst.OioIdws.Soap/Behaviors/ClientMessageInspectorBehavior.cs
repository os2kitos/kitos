using System;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Digst.OioIdws.Soap.Behaviors
{
    // Endpoint behavior
    public class ClientMessageInspectorBehavior : BehaviorExtensionElement, IEndpointBehavior
    {
        protected override object CreateBehavior()
        {
            return new ClientMessageInspectorBehavior();
        }

        public override Type BehaviorType
        {
            get { return typeof(ClientMessageInspectorBehavior); }
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // No implementation necessary
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new ClientMessageInspector());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // No implementation necessary
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // No implementation necessary
        }
    }
}
