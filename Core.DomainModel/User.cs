using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.SSO;
using Core.DomainModel.Tracking;
using Core.DomainModel.Users;

// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel
{
    /// <summary>
    ///     Represents a user with credentials and user roles
    /// </summary>
    public class User : Entity, IIsPartOfOrganization, IHasName, IHasUuid, ISupportsUserSpecificAccessControl
    {
        public string GetFullName()
        {
            return $"{Name} {LastName}";
        }
        public User()
        {
            PasswordResetRequests = new List<PasswordResetRequest>();
            OrganizationRights = new List<OrganizationRight>();
            OrganizationUnitRights = new List<OrganizationUnitRight>();
            ItSystemRights = new List<ItSystemRight>();
            ItContractRights = new List<ItContractRight>();
            LockedOutDate = null;
            FailedAttempts = 0;
            Uuid = Guid.NewGuid();
            DataProcessingRegistrationRights = new List<DataProcessingRegistrationRight>();
            StsOrganizationChangeLogs = new List<StsOrganizationChangeLog>();
        }

        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public DateTime? LastAdvisDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool Deleted { get; set; }

        public string DefaultUserStartPreference { get; set; }
        /// <summary>
        /// User has been granted api access
        /// </summary>
        public bool? HasApiAccess { get; set; }

        public IEnumerable<AuthenticationScheme> GetAuthenticationSchemes()
        {
            if (CanAuthenticate())
            {
                if (HasApiAccess == true)
                {
                    //API users can only authenticate with tokens
                    yield return AuthenticationScheme.Token;
                }
                else
                {
                    yield return AuthenticationScheme.Cookie;
                }
            }
        }
        /// <summary>
        /// User has been marked as a user with stake holder access
        /// </summary>
        public bool HasStakeHolderAccess { get; set; }

        public bool CanAuthenticate()
        {
            return !Deleted && (IsGlobalAdmin || OrganizationRights.Any());
        }

        /// <summary>
        ///     The admin rights for the user
        /// </summary>
        public virtual ICollection<OrganizationRight> OrganizationRights { get; set; }

        /// <summary>
        ///     The organization unit rights for the user
        /// </summary>
        public virtual ICollection<OrganizationUnitRight> OrganizationUnitRights { get; set; }

        public IEnumerable<OrganizationUnitRight> GetOrganizationUnitRights(Guid organiztionId)
        {
            return OrganizationUnitRights.Where(x => x.Object.Organization.Uuid == organiztionId);
        }

        /// <summary>
        ///     The system rights for the user
        /// </summary>
        public virtual ICollection<ItSystemRight> ItSystemRights { get; set; }

        public IEnumerable<ItSystemRight> GetItSystemRights(Guid organizationId)
        {
            return ItSystemRights.Where(x => x.Object.Organization.Uuid == organizationId);
        }

        /// <summary>
        ///     The system rights for the user
        /// </summary>
        public virtual ICollection<ItContractRight> ItContractRights { get; set; }

        public IEnumerable<ItContractRight> GetItContractRights(Guid organizationId)
        {
            return ItContractRights.Where(x => x.Object.Organization.Uuid == organizationId);
        }

        /// <summary>
        ///     Passwords reset request issued for the user
        /// </summary>
        public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; }

        public virtual ICollection<SsoUserIdentity> SsoIdentities { get; set; }
        public virtual ICollection<StsOrganizationChangeLog> StsOrganizationChangeLogs { get; set; }

        /// <summary>
        /// Rights withing dpa
        /// </summary>
        public virtual ICollection<DataProcessingRegistrationRight> DataProcessingRegistrationRights { get; set; }

        public IEnumerable<DataProcessingRegistrationRight> GetDataProcessingRegistrationRights(Guid organizationId)
        {
            return DataProcessingRegistrationRights.Where(x => x.Object.Organization.Uuid == organizationId);
        }

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

        public IEnumerable<int> GetOrganizationIdsWhereRoleIsAssigned(OrganizationRole role)
        {
            return OrganizationRights.
                Where(x => x.Role == role)
                .Select(x => x.OrganizationId)
                .Distinct();
        }

        public IEnumerable<Organization.Organization> GetOrganizationsWhereRoleIsAssigned(OrganizationRole role)
        {
            return OrganizationRights
                    .Where(x => x.Role == role)
                    .Select(x => x.Organization)
                    .ToList();
        }

        public IEnumerable<Organization.Organization> GetOrganizations()
        {
            return OrganizationRights
                .Select(x => x.Organization)
                .ToList();
        }

        public IEnumerable<Organization.Organization> GetUniqueOrganizations()
        {
            return OrganizationRights
                .Select(x => x.Organization)
                .GroupBy(org => org.Uuid)
                .Select(group => group.First())
                .ToList();
        }

        public IEnumerable<OrganizationRole> GetRolesInOrganization(Guid organizationUuid)
        {
            return OrganizationRights
                .Where(organizationRight => organizationRight.Organization.Uuid == organizationUuid)
                .Select(organizationRight => organizationRight.Role)
                .Distinct();
        }



        public Maybe<OperationError> UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new OperationError($"Email '{email}' is required", OperationFailure.BadInput);
            }
            Email = email;

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateFirstName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new OperationError($"Name '{name}' is required", OperationFailure.BadInput);
            }
            Name = name;

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
            {
                return new OperationError($"Last name '{lastName}' is required", OperationFailure.BadInput);
            }
            LastName = lastName;

            return Maybe<OperationError>.None;
        }

        #region Authentication

        public bool IsGlobalAdmin { get; set; }
        public Guid Uuid { get; set; }
        public virtual ICollection<LifeCycleTrackingEvent> LifeCycleTrackingEvents { get; set; }

        public bool HasUserWriteAccess(User user)
        {
            //User can edit themselves
            return Id == user.Id;
        }

        #endregion
    }
}