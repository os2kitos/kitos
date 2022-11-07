using System;

namespace Core.DomainModel.Commands
{
    public class RemoveOrganizationUnitRegistrationsCommand : ICommand
    {
        public RemoveOrganizationUnitRegistrationsCommand(Guid organizationUuid, Guid unitUuid)
        {
            OrganizationUuid = organizationUuid;
            UnitUuid = unitUuid;
        }

        public Guid OrganizationUuid { get; }
        public Guid UnitUuid { get; }

    }
}
