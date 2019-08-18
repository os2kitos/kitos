using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;

// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel
{
    /// <summary>
    ///     Represents a user with credentials and user roles
    /// </summary>
    public class User : Entity, IContextAware
    {
        public User()
        {
            PasswordResetRequests = new List<PasswordResetRequest>();
            OrganizationRights = new List<OrganizationRight>();
            OrganizationUnitRights = new List<OrganizationUnitRight>();
            ItProjectRights = new List<ItProjectRight>();
            ItSystemRights = new List<ItSystemRight>();
            ItContractRights = new List<ItContractRight>();
            Wishes = new List<Wish>();
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
        public string UniqueId { get; set; }
        public DateTime? LastAdvisDate { get; set; }

        public int? DefaultOrganizationId { get; set; }

        public string DefaultUserStartPreference { get; set; }

        /// <summary>
        ///     The organization the user will be automatically logged into.
        /// </summary>
        /// <remarks>
        ///     WARN: this is currently abused to track what organization the user is logged into,
        ///     and will change in the future.
        /// </remarks>
        public virtual Organization.Organization DefaultOrganization { get; set; }

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
        ///     Wishes created by this user
        /// </summary>
        public virtual ICollection<Wish> Wishes { get; set; }

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

        public DateTime? LockedOutDate { get; set; }

        public int FailedAttempts { get; set; }


        public override string ToString()
        {
            return $"{Id}:{Name} {LastName}";
        }

        #region Authentication

        public bool IsGlobalAdmin { get; set; }

        public bool IsReadOnly => IsReadOnlyInOrg(DefaultOrganizationId.GetValueOrDefault());

        public bool IsReadOnlyInOrg(int organizationId)
        {
            return OrganizationRights.Any(
                right => (right.Role == OrganizationRole.ReadOnly) &&
                         (right.OrganizationId == organizationId));
        }

        public override bool HasUserWriteAccess(User user)
        {
            if (IsReadOnly)
            {
                return (Id == user.Id) || base.HasUserWriteAccess(user);
            }

            return IsReadOnly;
        }

        public bool IsInContext(int organizationId)
        {
            return DefaultOrganizationId == organizationId;
        }

        public bool IsLocalAdmin => IsLocalAdminInOrg(DefaultOrganizationId.GetValueOrDefault());

        public bool IsLocalAdminInOrg(int organizationId)
        {
            return OrganizationRights.Any(
                right => (right.Role == OrganizationRole.LocalAdmin) &&
                         right.OrganizationId == organizationId);
        }

        public IEnumerable<OrganizationRole> GetRolesInOrg(int organizationId)
        {
            var roles =
                OrganizationRights
                    .Where(or => or.OrganizationId == organizationId)
                    .Select(x => x.Role)
                    .ToList();

            //NOTE: Use of this property is somewhat messy. In some cases it applies the IsGlobalAdmin boolean (the right way) and in other cases it uses the "right" with the role "Global admin" which is the wrong way
            if (IsGlobalAdmin)
            {
                roles.Add(OrganizationRole.GlobalAdmin);
            }

            return roles
                .Distinct()
                .ToList()
                .AsReadOnly();
        }

        #endregion
    }
}