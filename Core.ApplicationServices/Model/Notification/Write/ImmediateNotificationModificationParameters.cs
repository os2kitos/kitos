namespace Core.ApplicationServices.Model.Notification.Write
{
    public class ImmediateNotificationModificationParameters : IHasBaseNotificationPropertiesParameters
    {
        public ImmediateNotificationModificationParameters(BaseNotificationPropertiesModificationParameters baseProperties)
        {
            BaseProperties = baseProperties;
        }

        public BaseNotificationPropertiesModificationParameters BaseProperties { get; }
    }
}
