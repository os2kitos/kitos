namespace Core.DomainModel.Organization
{
    public class UnitAccessRights
    {
        public UnitAccessRights(bool canBeRead, bool canBeModified, bool canBeRenamed, bool canInfoAdditionalfieldsBeModified, bool canBeRearranged, bool canBeDeleted)
        {
            CanBeRead = canBeRead;
            CanBeModified = canBeModified;
            CanBeRenamed = canBeRenamed;
            CanEanBeModified = canInfoAdditionalfieldsBeModified;
            CanDeviceIdBeModified= canInfoAdditionalfieldsBeModified;
            CanBeRearranged = canBeRearranged;
            CanBeDeleted = canBeDeleted;
        }

        public static UnitAccessRights ReadOnly() => new(true, false, false, false, false, false);

        public bool CanBeRead { get; }
        public bool CanBeModified { get; }
        public bool CanBeRenamed { get; }
        public bool CanEanBeModified { get; }
        public bool CanDeviceIdBeModified { get; }
        public bool CanBeRearranged{ get; }
        public bool CanBeDeleted { get; }
    }
}
