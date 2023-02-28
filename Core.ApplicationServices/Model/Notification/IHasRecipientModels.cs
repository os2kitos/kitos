namespace Core.ApplicationServices.Model.Notification
{
    public interface IHasRecipientModels
    {
        RecipientModel Ccs { get; }
        RecipientModel Receivers { get; }
    }
}
