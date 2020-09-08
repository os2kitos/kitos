namespace Presentation.Web.Models.GDPR
{
    public class DataProcessingAgreementDTO : NamedEntityDTO
    {
        public DataProcessingAgreementDTO(int id, string name) 
            : base(id, name)
        {
        }

        public AssignedRoleDTO[] AssignedRoles { get; set; }
    }
}