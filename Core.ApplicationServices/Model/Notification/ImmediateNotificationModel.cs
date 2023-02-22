namespace Core.ApplicationServices.Model.Notification
{
    public class ImmediateNotificationModel : IHasBasePropertiesModel, IHasRecipientModels
    {
        public BaseNotificationModel BaseProperties { get; set; }
        public RecipientModel Ccs { get; set; }
        public RecipientModel Receivers { get; set; }
    }
}
