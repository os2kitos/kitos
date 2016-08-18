﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;

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
        public bool IsGlobalAdmin { get; set; }
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
        /// The admin rights of the user
        /// </summary>
        public virtual ICollection<OrganizationRight> OrganizationRights { get; set; }

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

        public override bool HasUserWriteAccess(User user)
        {
            if (Id == user.Id)
                return true;

            return base.HasUserWriteAccess(user);
        }

        public override string ToString()
        {
            return $"{Id}:{Name} {LastName}";
        }
    }
}
