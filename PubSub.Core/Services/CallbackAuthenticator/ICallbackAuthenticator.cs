namespace PubSub.Core.Services.CallbackAuthentication
{
    public interface ICallbackAuthenticator
    {
        string GetAuthentication(string source);
    }
}
