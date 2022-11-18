namespace Core.DomainModel.Organization
{
    public class UnitAccessRightsWithUnitData
    {
        public UnitAccessRightsWithUnitData(OrganizationUnit organizationUnit, UnitAccessRights unitAccessRights)
        {
            OrganizationUnit = organizationUnit;
            UnitAccessRights = unitAccessRights;
        }

        public OrganizationUnit OrganizationUnit { get; }
        public UnitAccessRights UnitAccessRights { get; }
    }
}
