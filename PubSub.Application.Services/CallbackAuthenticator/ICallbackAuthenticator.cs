namespace PubSub.Core.ApplicationServices.CallbackAuthenticator
{
    public interface ICallbackAuthenticator
    {
        string GetAuthentication(string source);
    }
}
