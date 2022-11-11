namespace Core.DomainModel.Organization
{
    public class UnitAccessRights
    {
        public UnitAccessRights(bool canBeRead, bool canBeModified, bool canFieldsBeModified, bool canBeRearranged, bool canBeDeleted)
        {
            CanBeRead = canBeRead;
            CanBeModified = canBeModified;
            CanNameBeModified = canFieldsBeModified;
            CanEanBeModified = canFieldsBeModified;
            CanDeviceIdBeModified= canFieldsBeModified;
            CanBeRearranged = canBeRearranged;
            CanBeDeleted = canBeDeleted;
        }

        public bool CanBeRead { get; }
        public bool CanBeModified { get; }
        public bool CanNameBeModified { get; }
        public bool CanEanBeModified { get; }
        public bool CanDeviceIdBeModified { get; }
        public bool CanBeRearranged{ get; }
        public bool CanBeDeleted { get; }
    }
}
