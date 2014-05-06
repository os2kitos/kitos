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
            this.PasswordResetRequests = new List<PasswordResetRequest>();
            this.OrganizationRights = new List<OrganizationRight>();
            this.ProjectRights = new List<ItProjectRight>();
            this.SystemRights = new List<ItSystemRight>();
            this.ContractRights = new List<ItContractRight>();
            this.AdminRights = new List<AdminRight>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public bool IsGlobalAdmin { get; set; }
        public int? DefaultOrganizationUnitId { get; set; }

        public virtual OrganizationUnit DefaultOrganizationUnit { get; set; }
        public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; }
        public virtual ICollection<OrganizationRight> OrganizationRights { get; set; }
        public virtual ICollection<ItProjectRight> ProjectRights { get; set; }
        public virtual ICollection<ItSystemRight> SystemRights { get; set; }
        public virtual ICollection<ItContractRight> ContractRights { get; set; }
        public virtual ICollection<AdminRight> AdminRights { get; set; }
        public virtual ICollection<ItSystem.ItSystem> CreatedSystems { get; set; }
        public virtual ICollection<ItSystem.ItSystemUsage> CreatedSystemUsages { get; set; }
        public virtual ICollection<Risk> ResponsibleForRisks { get; set; }
        public virtual ICollection<Wish> Wishes { get; set; }
    }
}