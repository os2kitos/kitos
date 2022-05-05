namespace Presentation.Web.Models.API.V1
{
    public class EntityWithOrganizationRelationshipDTO : NamedEntityDTO
    {
        public ShallowOrganizationDTO Organization { get; set; }

        public EntityWithOrganizationRelationshipDTO(int id, string name, ShallowOrganizationDTO organization) 
            : base(id, name)
        {
            Organization = organization;
        }
    }
}