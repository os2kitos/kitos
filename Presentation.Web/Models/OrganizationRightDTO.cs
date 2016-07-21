namespace Presentation.Web.Models
{
    public class OrganizationRightDTO
    {
        public int OrganizationId { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public string ObjectOwnerName { get; set; }
        public string ObjectOwnerLastName { get; set; }
        public string ObjectOwnerFullName
        {
            get { return ObjectOwnerName + " " + ObjectOwnerLastName; }
        }
        public string DefaultOrgUnitName { get; set; }
    }
}
