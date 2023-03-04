namespace Core.ApplicationServices.Model.Notification
{
    public class ImmediateNotificationModel : IHasBaseNotificationPropertiesModel, IHasRecipientModels
    {
        public ImmediateNotificationModel(BaseNotificationPropertiesModel baseProperties, RecipientModel ccs, RecipientModel receivers)
        {
            BaseProperties = baseProperties;
            Ccs = ccs;
            Receivers = receivers;
        }

        public BaseNotificationPropertiesModel BaseProperties { get; }
        public RecipientModel Ccs { get; }
        public RecipientModel Receivers { get; }
    }
}
