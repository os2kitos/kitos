using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public class UnitWithAccessRights
    {
        public UnitWithAccessRights(OrganizationUnit organizationUnit, UnitAccessRights unitAccessRights)
        {
            OrganizationUnit = organizationUnit;
            UnitAccessRights = unitAccessRights;
        }

        public OrganizationUnit OrganizationUnit { get; }
        public UnitAccessRights UnitAccessRights { get; }
    }
}
