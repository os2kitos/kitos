using System.Linq;
using Digst.OioIdws.SoapCore;
using Digst.OioIdws.SoapCore.Behaviors;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Infrastructure.STS.Common.Model.Token;

/// <summary>
/// Fixed version of <see cref="FederatedChannelFactory{TChannel}"/> that correctly replaces the base
/// <see cref="ClientCredentials"/> with <see cref="FederatedChannelClientCredentials"/> after construction.
///
/// Root cause: In .NET 10 WCF, <see cref="ChannelFactory"/> .InitializeEndpoint(Binding, EndpointAddress)
/// calls <c>EnsureSecurityCredentialsManager</c> which adds a base <see cref="ClientCredentials"/> to
/// the endpoint behaviors. Unlike .NET Framework, it does NOT call <c>ApplyConfiguration</c>.
/// Kombit's <see cref="FederatedChannelFactory{TChannel}.ApplyConfiguration"/> override is therefore
/// never invoked, so <see cref="FederatedChannelClientCredentials"/> is never added.
/// <see cref="Digst.OioIdws.SoapCore.Bindings.OioIdwsSoapBindingElement.BuildChannelFactory"/> then throws
/// "No Client certificate was configured." because it requires <see cref="FederatedChannelClientCredentials"/>
/// in the binding parameters, not the base <see cref="ClientCredentials"/>.
/// </summary>
internal sealed class FixedFederatedChannelFactory<TChannel> : FederatedChannelFactory<TChannel>
{
    public FixedFederatedChannelFactory(Binding binding, EndpointAddress remoteAddress)
        : base(binding, remoteAddress)
    {
        // After base(binding, remoteAddress): Endpoint is set and EndpointBehaviors contains a base
        // ClientCredentials added by ChannelFactory.EnsureSecurityCredentialsManager.
        // ApplyConfiguration was NOT called in .NET 10 WCF for this constructor path.
        // We must replace the base ClientCredentials with FederatedChannelClientCredentials here.
        SwapCredentials(Endpoint.EndpointBehaviors);
    }

    /// <summary>
    /// Also override ApplyConfiguration for future-proofing in case other constructor paths call it.
    /// </summary>
    protected override void ApplyConfiguration(string configurationName)
    {
        SwapCredentials(Endpoint.EndpointBehaviors);
    }

    private static void SwapCredentials(System.Collections.Generic.ICollection<IEndpointBehavior> behaviors)
    {
        // Remove base ClientCredentials (but not FederatedChannelClientCredentials which is a subtype)
        var existing = behaviors.OfType<ClientCredentials>()
            .FirstOrDefault(c => c is not FederatedChannelClientCredentials);
        if (existing is not null)
            behaviors.Remove(existing);

        // Ensure FederatedChannelClientCredentials is present (OioIdwsSoapBindingElement requires it)
        if (!behaviors.OfType<FederatedChannelClientCredentials>().Any())
            behaviors.Add(new FederatedChannelClientCredentials());

        // Ensure SoapClientBehavior is present (adds message inspector)
        if (!behaviors.OfType<SoapClientBehavior>().Any())
            behaviors.Add(new SoapClientBehavior());
    }
}
