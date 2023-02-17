namespace Core.ApplicationServices.Model.Notification
{
    public class NotificationAccessRights
    {
        public NotificationAccessRights(bool canBeDeleted, bool canBeDeactivated, bool canBeModified)
        {
            CanBeDeleted = canBeDeleted;
            CanBeDeactivated = canBeDeactivated;
            CanBeModified = canBeModified;
        }

        public static NotificationAccessRights ReadOnly() => new(false, false, false);
        public bool CanBeDeleted { get; }
        public bool CanBeDeactivated { get; }
        public bool CanBeModified { get; }
    }
}
