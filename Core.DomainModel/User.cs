using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{    
    /// <summary>
    /// Represents a user with credentials and user roles
    /// </summary>
    public class User : Entity
    {
        public User()
        {
            this.PasswordResetRequests = new List<PasswordResetRequest>();
            this.AdminRights = new List<AdminRight>();
            this.Wishes = new List<Wish>();
            this.ItProjectStatuses = new List<ItProjectStatus>();
            this.ResponsibleForRisks = new List<Risk>();
            this.ResponsibleForCommunications = new List<Communication>();
            this.HandoverParticipants = new List<Handover>();
            this.SignerForContracts = new Collection<ItContract.ItContract>();
        }

        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public bool IsGlobalAdmin { get; set; }
        public Guid? Uuid { get; set; }
        public DateTime? LastAdvisDate { get; set; }   

        //TODO REMOVE THIS - replaced by admin rights default org unit
        public int? DefaultOrganizationUnitId { get; set; }

        /// <summary>
        /// The organization unit that the user has selected as his default.
        /// </summary>
        public virtual OrganizationUnit DefaultOrganizationUnit { get; set; }

        /// <summary>
        /// The admin rights of the user
        /// </summary>
        public virtual ICollection<AdminRight> AdminRights { get; set; }
        
        /// <summary>
        /// Passwords reset request issued for the user
        /// </summary>
        public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; }

        /// <summary>
        /// Wishes created by this user
        /// </summary>
        public virtual ICollection<Wish> Wishes { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Assignment"/> or <see cref="Milestone"/> associated with this user
        /// </summary>
        public virtual ICollection<ItProjectStatus> ItProjectStatuses { get; set; }
        
        /// <summary>
        /// Risks associated with this user
        /// </summary>
        public virtual ICollection<Risk> ResponsibleForRisks { get; set; }

        /// <summary>
        /// Communications associated with this user
        /// </summary>
        public virtual ICollection<Communication> ResponsibleForCommunications { get; set; }

        /// <summary>
        /// Handovers associated with this user
        /// </summary>
        public virtual ICollection<Handover> HandoverParticipants { get; set; }

        /// <summary>
        /// The contracts that the user has been marked as contract signer for
        /// </summary>
        public virtual ICollection<ItContract.ItContract> SignerForContracts { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (Id == user.Id) 
                return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
