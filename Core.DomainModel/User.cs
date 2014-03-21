using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{
    public class User : IEntity<int>
    {
        public User()
        {
            PasswordResetRequests = new List<PasswordResetRequest>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public bool IsGlobalAdmin { get; set; }

        public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; }

        public virtual ICollection<OrganizationRight> OrganizationRights { get; set; }
        public virtual ICollection<ItProjectRight> ProjectRights { get; set; }
        public virtual ICollection<ItSystemRight> SystemRights { get; set; }
        public virtual ICollection<ItContractRight> ContractRights { get; set; }
        public virtual ICollection<AdminRight> AdminRights { get; set; }
    }
}