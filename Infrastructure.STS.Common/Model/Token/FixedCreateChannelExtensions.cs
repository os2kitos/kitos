using System;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.SoapCore;
using Digst.OioIdws.SoapCore.Bindings;
using Digst.OioIdws.SoapCore.Tokens;

namespace Infrastructure.STS.Common.Model.Token;

/// <summary>
/// Replacement for <see cref="FederatedChannelFactoryExtensions.CreateChannelWithIssuedToken{T}"/>
/// that uses <see cref="FixedFederatedChannelFactory{TChannel}"/> to work around the .NET 10 WCF
/// incompatibility where <c>FederatedChannelClientCredentials</c> is never added to endpoint
/// behaviors when <c>EndpointBehaviors.Count != 0</c> on <c>ApplyConfiguration</c> entry.
/// </summary>
internal static class FixedCreateChannelExtensions
{
    public static T CreateChannelWithIssuedToken<T>(
        GenericXmlSecurityToken token,
        IStsTokenServiceConfiguration stsConfiguration)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(stsConfiguration);

        var serviceCert = stsConfiguration.WspConfiguration.ServiceCertificate;
        var messageVersion = MessageVersion.CreateVersion(
            stsConfiguration.WspConfiguration.SoapVersion,
            AddressingVersion.WSAddressing10);

        var tokenParams = new FederatedSecurityTokenParameters(
            token, messageVersion, stsConfiguration, stsConfiguration.WspConfiguration.EndpointAddress);
        ((System.ServiceModel.Federation.WSTrustTokenParameters)tokenParams).MessageSecurityVersion =
            MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
        tokenParams.MaxReceivedMessageSize = stsConfiguration.MaxReceivedMessageSize;
        tokenParams.IncludeLibertyHeader = stsConfiguration.IncludeLibertyHeader;

        var binding = new OioIdwsSoapBinding(tokenParams);
        var factory = CreateFactory<T>(stsConfiguration, serviceCert, binding);

        stsConfiguration.WspCertificateAuthentication.Validate(serviceCert);
        return ((ChannelFactory<T>)(object)factory).CreateChannel();
    }

    private static FixedFederatedChannelFactory<T> CreateFactory<T>(
        IStsTokenServiceConfiguration config,
        X509Certificate2 serverCert,
        OioIdwsSoapBinding binding)
    {
        var factory = new FixedFederatedChannelFactory<T>(
            (Binding)(object)binding,
            new EndpointAddress(config.WspConfiguration.EndpointAddress));

        ((ChannelFactory)(object)factory).Credentials.ServiceCertificate.Authentication
            .CopyFrom(config.WspCertificateAuthentication);
        ((ChannelFactory)(object)factory).Credentials.ServiceCertificate.SslCertificateAuthentication =
            config.SslCertificateAuthentication.DeepClone();

        var dnsIdentity = new DnsEndpointIdentity(
            serverCert.GetNameInfo(X509NameType.DnsName, forIssuer: false));
        var endpointAddress = new EndpointAddress(
            new Uri(config.WspConfiguration.EndpointAddress),
            dnsIdentity,
            Array.Empty<AddressHeader>());

        ((ChannelFactory)(object)factory).Endpoint.Address = endpointAddress;
        ((ChannelFactory)(object)factory).Credentials.ClientCertificate.Certificate = config.ClientCertificate;
        ((ChannelFactory)(object)factory).Credentials.ServiceCertificate.ScopedCertificates
            .Add(endpointAddress.Uri, serverCert);

        return factory;
    }
}
