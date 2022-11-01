namespace Core.DomainModel.Commands
{
    public class RemoveOrganizationUnitRegistrationsCommand : ICommand
    {
        public RemoveOrganizationUnitRegistrationsCommand(int organizationId, int unitId)
        {
            OrganizationId = organizationId;
            UnitId = unitId;
        }

        public int OrganizationId { get; }
        public int UnitId { get; set; }

    }
}
