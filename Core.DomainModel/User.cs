using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.SSO;

// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel
{
    /// <summary>
    ///     Represents a user with credentials and user roles
    /// </summary>
    public class User : Entity, IIsPartOfOrganization
    {
        public User()
        {
            PasswordResetRequests = new List<PasswordResetRequest>();
            OrganizationRights = new List<OrganizationRight>();
            OrganizationUnitRights = new List<OrganizationUnitRight>();
            ItProjectRights = new List<ItProjectRight>();
            ItSystemRights = new List<ItSystemRight>();
            ItContractRights = new List<ItContractRight>();
            ItProjectStatuses = new List<ItProjectStatus>();
            ResponsibleForRisks = new List<Risk>();
            ResponsibleForCommunications = new List<Communication>();
            HandoverParticipants = new List<Handover>();
            LockedOutDate = null;
            FailedAttempts = 0;
        }

        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public DateTime? LastAdvisDate { get; set; }

        public string DefaultUserStartPreference { get; set; }

        public bool? HasApiAccess { get; set; }

        public bool CanAuthenticate()
        {
            return IsGlobalAdmin || OrganizationRights.Any();
        }

        /// <summary>
        ///     The admin rights for the user
        /// </summary>
        public virtual ICollection<OrganizationRight> OrganizationRights { get; set; }

        /// <summary>
        ///     The organization unit rights for the user
        /// </summary>
        public virtual ICollection<OrganizationUnitRight> OrganizationUnitRights { get; set; }

        /// <summary>
        ///     The project rights for the user
        /// </summary>
        public virtual ICollection<ItProjectRight> ItProjectRights { get; set; }

        /// <summary>
        ///     The system rights for the user
        /// </summary>
        public virtual ICollection<ItSystemRight> ItSystemRights { get; set; }

        /// <summary>
        ///     The system rights for the user
        /// </summary>
        public virtual ICollection<ItContractRight> ItContractRights { get; set; }

        /// <summary>
        ///     Passwords reset request issued for the user
        /// </summary>
        public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="Assignment" /> or <see cref="Milestone" /> associated with this user
        /// </summary>
        public virtual ICollection<ItProjectStatus> ItProjectStatuses { get; set; }

        /// <summary>
        ///     Risks associated with this user
        /// </summary>
        public virtual ICollection<Risk> ResponsibleForRisks { get; set; }

        /// <summary>
        ///     Communications associated with this user
        /// </summary>
        public virtual ICollection<Communication> ResponsibleForCommunications { get; set; }

        /// <summary>
        ///     Handovers associated with this user
        /// </summary>
        public virtual ICollection<Handover> HandoverParticipants { get; set; }

        public virtual ICollection<SsoUserIdentity> SsoIdentities { get; set; }

        public DateTime? LockedOutDate { get; set; }

        public int FailedAttempts { get; set; }


        public override string ToString()
        {
            return $"{Id}:{Name} {LastName}";
        }

        public IEnumerable<int> GetOrganizationIds()
        {
            return OrganizationRights
                .Select(x => x.OrganizationId)
                .Distinct();
        }

        #region Authentication

        public bool IsGlobalAdmin { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            return (Id == user.Id) || base.HasUserWriteAccess(user);
        }

        #endregion
    }
}