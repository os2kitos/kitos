﻿using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Infrastructure.STS.Common.Factories
{
    public static class BasicHttpBindingFactory
    {
        public static BasicHttpBinding CreateHttpBinding()
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
    }
}
