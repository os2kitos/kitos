namespace Presentation.Web.Models.API.V1.Organizations
{
    public class InterfaceExposedOnSystemsOutsideTheOrganizationConflictDTO
    {
        public NamedEntityDTO ExposedInterface { get; }
        public EntityWithOrganizationRelationshipDTO ExposedBy { get; }

        public InterfaceExposedOnSystemsOutsideTheOrganizationConflictDTO(NamedEntityDTO exposedInterface, EntityWithOrganizationRelationshipDTO exposedBy)
        {
            ExposedInterface = exposedInterface;
            ExposedBy = exposedBy;
        }
    }
}