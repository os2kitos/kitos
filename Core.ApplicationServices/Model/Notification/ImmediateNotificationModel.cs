namespace Core.ApplicationServices.Model.Notification
{
    public class ImmediateNotificationModel : IHasBaseNotificationPropertiesModel, IHasRecipientModels
    {
        public BaseNotificationPropertiesModel BaseProperties { get; set; }
        public RecipientModel Ccs { get; set; }
        public RecipientModel Receivers { get; set; }
    }
}
