namespace Presentation.Web.Models.API.V1.Organizations
{
    public class SystemWithUsageOutsideOrganizationConflictDTO
    {
        public NamedEntityDTO System { get; }
        public ShallowOrganizationDTO[] OtherOrganizationsWhichUseTheSystem { get; }

        public SystemWithUsageOutsideOrganizationConflictDTO(NamedEntityDTO system, ShallowOrganizationDTO[] otherOrganizationsWhichUseTheSystem)
        {
            System = system;
            OtherOrganizationsWhichUseTheSystem = otherOrganizationsWhichUseTheSystem;
        }
    }
}