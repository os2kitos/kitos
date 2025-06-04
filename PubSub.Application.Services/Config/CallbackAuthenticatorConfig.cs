namespace PubSub.Core.ApplicationServices.Config
{
    public class CallbackAuthenticatorConfig: ICallbackAuthenticatorConfig
    {
        public required string ApiKey { get; set; }
    }
}
