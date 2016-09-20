using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.ItContract;
// ReSharper disable VirtualMemberCallInConstructor

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
            this.OrganizationRights = new List<OrganizationRight>();
            this.OrganizationUnitRights = new List<OrganizationUnitRight>();
            this.ItProjectRights = new List<ItProjectRight>();
            this.ItSystemRights = new List<ItSystemRight>();
            this.ItContractRights = new List<ItContractRight>();
            this.Wishes = new List<Wish>();
            this.ItProjectStatuses = new List<ItProjectStatus>();
            this.ResponsibleForRisks = new List<Risk>();
            this.ResponsibleForCommunications = new List<Communication>();
            this.HandoverParticipants = new List<Handover>();
            this.SignerForContracts = new Collection<ItContract.ItContract>();
            LockedOutDate = null;
            FailedAttempts = 0;
        }

        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public Guid? Uuid { get; set; }
        public DateTime? LastAdvisDate { get; set; }

        public int? DefaultOrganizationId { get; set; }
        /// <summary>
        /// The organization the user will be automatically logged into.
        /// </summary>
        /// <remarks>
        /// WARN: this is currently abused to track what organization the user is logged into,
        /// and will change in the future.
        /// </remarks>
        public virtual Organization.Organization DefaultOrganization { get; set; }

        /// <summary>
        /// The admin rights for the user
        /// </summary>
        public virtual ICollection<OrganizationRight> OrganizationRights { get; set; }

        /// <summary>
        /// The organization unit rights for the user
        /// </summary>
        public virtual ICollection<OrganizationUnitRight> OrganizationUnitRights { get; set; }

        /// <summary>
        /// The project rights for the user
        /// </summary>
        public virtual ICollection<ItProjectRight> ItProjectRights { get; set; }

        /// <summary>
        /// The system rights for the user
        /// </summary>
        public virtual ICollection<ItSystemRight> ItSystemRights { get; set; }

        /// <summary>
        /// The system rights for the user
        /// </summary>
        public virtual ICollection<ItContractRight> ItContractRights { get; set; }

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

        public DateTime? LockedOutDate { get; set; }

        public int FailedAttempts { get; set; }

        #region Authentication

        public bool IsGlobalAdmin { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            return Id == user.Id || base.HasUserWriteAccess(user);
        }

        public bool IsLocalAdmin
        {
            get
            {
                return OrganizationRights.Any(
                    right => right.Role == OrganizationRole.LocalAdmin &&
                             right.OrganizationId == DefaultOrganizationId.GetValueOrDefault());
            }
        }

        #endregion


        public override string ToString()
        {
            return $"{Id}:{Name} {LastName}";
        }
    }
}
