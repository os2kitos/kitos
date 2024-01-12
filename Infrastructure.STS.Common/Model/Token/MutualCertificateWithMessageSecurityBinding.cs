using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security.Tokens;
using System.ServiceModel.Security;

namespace Infrastructure.STS.Common.Model.Token
{
    public class MutualCertificateWithMessageSecurityBinding : CustomBinding
    {
        public MutualCertificateWithMessageSecurityBinding() : base(CreateBindingElements(null)) { }

        public MutualCertificateWithMessageSecurityBinding(Func<MessageEncodingBindingElement> messageEncodingElementFunc) : base(CreateBindingElements(messageEncodingElementFunc))
        {
        }

        private static BindingElementCollection CreateBindingElements(Func<MessageEncodingBindingElement> encodingElementFunc)
        {
            var transportBinding = CreateTransportBindingElement();
            var encodingBinding = CreateEncodingBindingElement(encodingElementFunc);
            var securityBinding = CreateSecurityBindingElement();

            var bindings = new BindingElementCollection {
                securityBinding,
                encodingBinding,
                transportBinding
            };

            return bindings;
        }

        private static HttpTransportBindingElement CreateTransportBindingElement()
        {
            return new HttpsTransportBindingElement
            {
                RequireClientCertificate = false
            };
        }

        private static MessageEncodingBindingElement CreateEncodingBindingElement(Func<MessageEncodingBindingElement> encodingElementFunc)
        {
            if (encodingElementFunc != null)
            {
                return encodingElementFunc();
            }

            var messageEncodingBindingElement = new TextMessageEncodingBindingElement()
            {
                MessageVersion = MessageVersion.Soap12WSAddressing10
            };

            return messageEncodingBindingElement;
        }

        private static SecurityBindingElement CreateSecurityBindingElement()
        {
            var messageSecurity = new AsymmetricSecurityBindingElement
            {
                AllowSerializedSigningTokenOnReply = true,
                MessageSecurityVersion = MessageSecurityVersion.WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10,
                DefaultAlgorithmSuite = SecurityAlgorithmSuite.Basic128Sha256,
                MessageProtectionOrder = MessageProtectionOrder.EncryptBeforeSign,
                LocalClientSettings =
                {
                    MaxClockSkew = new TimeSpan(0, 0, 1, 0),
                    TimestampValidityDuration = new TimeSpan(0, 0, 10, 0)
                },
                LocalServiceSettings =
                {
                    MaxClockSkew = new TimeSpan(0, 0, 1, 0)
                }
            };

            messageSecurity.LocalClientSettings.TimestampValidityDuration = new TimeSpan(0, 0, 10, 0);

            var x509SecurityParameter = new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Any, SecurityTokenInclusionMode.AlwaysToInitiator);
            messageSecurity.RecipientTokenParameters = x509SecurityParameter;
            messageSecurity.RecipientTokenParameters.RequireDerivedKeys = false;

            var initiator = new X509SecurityTokenParameters(X509KeyIdentifierClauseType.Any, SecurityTokenInclusionMode.AlwaysToRecipient)
            {
                RequireDerivedKeys = false
            };
            messageSecurity.InitiatorTokenParameters = initiator;

            return messageSecurity;
        }
    }
}
