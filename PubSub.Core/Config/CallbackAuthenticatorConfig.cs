namespace PubSub.Core.Config
{
    public class CallbackAuthenticatorConfig: ICallbackAuthenticatorConfig
    {
        public required string ApiKey { get; set; }
    }
}
