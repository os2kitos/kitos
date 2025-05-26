using System;
using System.ServiceModel;
using Digst.OioIdws.Soap.Bindings;

namespace Infrastructure.STS.Common.Factories
{
    public static class HttpBindingFactory
    {
        public static BasicHttpBinding CreateBasicHttpBinding()
        {
            var binding = new BasicHttpBinding
            {
                Security =
                {
                    Mode = BasicHttpSecurityMode.Transport,
                    Transport = {ClientCredentialType = HttpClientCredentialType.Certificate}
                },
                MaxReceivedMessageSize = int.MaxValue,
                OpenTimeout = new TimeSpan(0, 3, 0),
                CloseTimeout = new TimeSpan(0, 3, 0),
                ReceiveTimeout = new TimeSpan(0, 3, 0),
                SendTimeout = new TimeSpan(0, 3, 0),
            };
            return binding;
        }

        public static SoapBinding CreateSoapBinding()
        {
            var binding = new SoapBinding
            {
                MaxReceivedMessageSize = int.MaxValue,
                OpenTimeout = new TimeSpan(0, 3, 0),
                CloseTimeout = new TimeSpan(0, 3, 0),
                ReceiveTimeout = new TimeSpan(0, 3, 0),
                SendTimeout = new TimeSpan(0, 3, 0),
            };
            return binding;
        }
    }
}
