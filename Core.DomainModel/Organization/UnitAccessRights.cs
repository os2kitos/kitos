﻿namespace Core.DomainModel.Organization
{
    public class UnitAccessRights
    {
        public UnitAccessRights(bool canBeRead, bool canBeModified, bool canBeRenamed, bool canFieldsBeRenamed, bool canBeRearranged, bool canBeDeleted)
        {
            CanBeRead = canBeRead;
            CanBeModified = canBeModified;
            CanBeRenamed = canFieldsBeRenamed; //TODO: This one is a specific permission... only native units can be renamed by the user
            CanEanBeRenamed = canFieldsBeRenamed;
            CanDeviceIdBeRenamed= canFieldsBeRenamed;
            CanBeRearranged = canBeRearranged;
            CanBeDeleted = canBeDeleted;
        }

        public bool CanBeRead { get; }
        public bool CanBeModified { get; }
        public bool CanBeRenamed { get; }
        public bool CanEanBeRenamed { get; }
        public bool CanDeviceIdBeRenamed { get; }
        public bool CanBeRearranged{ get; }
        public bool CanBeDeleted { get; }
    }
}
