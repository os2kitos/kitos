using System;
using Core.DomainModel.Organization;

namespace Core.DomainModel.Commands
{
    public class RemoveOrganizationUnitRegistrationsCommand : ICommand
    {
        public RemoveOrganizationUnitRegistrationsCommand(Organization.Organization organization, OrganizationUnit organizationUnit)
        {
            Organization = organization;
            OrganizationUnit = organizationUnit;
        }

        public Organization.Organization Organization { get; }
        public OrganizationUnit OrganizationUnit{ get; }

    }
}
