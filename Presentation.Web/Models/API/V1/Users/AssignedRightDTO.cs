namespace Presentation.Web.Models.API.V1.Users
{
    public class AssignedRightDTO
    {
        /// <summary>
        /// The ID of the right, which is the assignment of the role to the business object.
        /// </summary>
        public int RightId { get; set; }
        /// <summary>
        /// The name of the role associated with he right
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// The name of the business object for which the right is created
        /// </summary>
        public string BusinessObjectName { get; set; }
        /// <summary>
        /// The role scope of the right assignment
        /// </summary>
        public BusinessRoleScope Scope { get; set; }
    }
}