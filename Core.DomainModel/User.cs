using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{
    public class User : Entity
    {
        public User()
        {
            this.PasswordResetRequests = new List<PasswordResetRequest>();
            this.AdminRights = new List<AdminRight>();

            this.Wishes = new List<Wish>();
            this.Activities = new List<Activity>();
            this.States = new List<State>();
            this.ResponsibleForRisks = new List<Risk>();
            this.ResponsibleForCommunications = new List<Communication>();
            this.HandoverParticipants = new List<Handover>();

            this.SignerForContracts = new Collection<ItContract.ItContract>();
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public bool IsGlobalAdmin { get; set; }
        public int? DefaultOrganizationUnitId { get; set; }
        public virtual OrganizationUnit DefaultOrganizationUnit { get; set; }

        public virtual ICollection<AdminRight> AdminRights { get; set; }


        public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; }
        public virtual ICollection<Wish> Wishes { get; set; }
        public virtual ICollection<Activity> Activities { get; set; }
        public virtual ICollection<State> States { get; set; }  
        public virtual ICollection<Risk> ResponsibleForRisks { get; set; }
        public virtual ICollection<Communication> ResponsibleForCommunications { get; set; } 
        public virtual ICollection<Handover> HandoverParticipants { get; set; }
        public virtual ICollection<ItContract.ItContract> SignerForContracts { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (user.Id == this.Id) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}