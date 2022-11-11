namespace Core.DomainModel.Organization
{
    public class UnitAccessRights
    {
        public UnitAccessRights(bool canBeRead, bool canBeModified, bool canNameBeModified, bool canBeRearranged, bool canBeDeleted)
        {
            CanBeRead = canBeRead;
            CanBeModified = canBeModified;
            CanNameBeModified = canNameBeModified;
            CanBeRearranged = canBeRearranged;
            CanBeDeleted = canBeDeleted;
        }

        public bool CanBeRead { get; }
        public bool CanBeModified { get; }
        public bool CanNameBeModified { get; }
        public bool CanBeRearranged{ get; }
        public bool CanBeDeleted { get; }
    }
}
