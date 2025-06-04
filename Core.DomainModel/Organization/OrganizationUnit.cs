using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Extensions;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.Organization
{
    /// <summary>
    /// Represents a unit or department within an organization (OIO term: "OrgEnhed").
    /// </summary>
    public class OrganizationUnit : HasRightsEntity<OrganizationUnit, OrganizationUnitRight, OrganizationUnitRole>, IHierarchy<OrganizationUnit>, IOrganizationModule, IOwnedByOrganization, IHasUuid, IHasName, IHasDirtyMarking
    {
        public const int MaxNameLength = 100;
        public OrganizationUnit()
        {
            OwnedTasks = new List<TaskRef>();
            DefaultUsers = new List<OrganizationRight>();
            Using = new List<ItSystemUsageOrgUnitUsage>();
            var uuid = Guid.NewGuid();
            Uuid = uuid;
            Origin = OrganizationUnitOrigin.Kitos;
            Children = new List<OrganizationUnit>();
            ResponsibleForItContracts = new List<ItContract.ItContract>();
            EconomyStreams = new List<EconomyStream>();
        }

        /// <summary>
        /// Determines the origin of the organization unit
        /// </summary>
        public OrganizationUnitOrigin Origin { get; set; }
        /// <summary>
        /// Determines the optional external origin-specific uuid
        /// </summary>
        public Guid? ExternalOriginUuid { get; set; }

        public string LocalId { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// EAN number of the department.
        /// </summary>
        public long? Ean { get; set; }

        public int? ParentId { get; set; }
        /// <summary>
        /// Parent department.
        /// </summary>
        public virtual OrganizationUnit Parent { get; set; }
        public virtual ICollection<OrganizationUnit> Children { get; set; }

        public int OrganizationId { get; set; }
        /// <summary>
        /// The organization which the unit belongs to.
        /// </summary>
        public virtual Organization Organization { get; set; }
        /// <summary>
        /// Local tasks that was created in this unit
        /// </summary>
        public virtual ICollection<TaskRef> OwnedTasks { get; set; }

        /// <summary>
        /// Users which have set this as their default OrganizationUnit.
        /// </summary>
        /// <remarks>
        /// Goes through <seealso cref="OrganizationRight"/>.
        /// So to access the user you must call .User on the rights object.
        /// </remarks>
        public virtual ICollection<OrganizationRight> DefaultUsers { get; set; }

        /// <summary>
        /// This Organization Unit is using these IT Systems (Via ItSystemUsage)
        /// </summary>
        public virtual ICollection<ItSystemUsageOrgUnitUsage> Using { get; set; }

        /// <summary>
        /// This Organization Unit is responsible for these IT ItContracts
        /// </summary>
        public virtual ICollection<ItContract.ItContract> ResponsibleForItContracts { get; set; }

        public virtual ICollection<DataProcessingRegistration> ResponsibleForDataProcessingRegistrations { get; set; }

        /// <summary>
        /// The Organization Unit is listed in these economy streams
        /// </summary>
        public virtual ICollection<EconomyStream> EconomyStreams { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            // check rights on parent org unit
            if (Parent != null && Parent.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }

        public override OrganizationUnitRight CreateNewRight(OrganizationUnitRole role, User user)
        {
            return new OrganizationUnitRight()
            {
                Role = role,
                User = user,
                Object = this
            };
        }

        public Guid Uuid { get; set; }

        public Maybe<OperationError> ImportNewExternalOrganizationOrgTree(OrganizationUnitOrigin origin, ExternalOrganizationUnit importRoot)
        {
            if (importRoot == null)
            {
                throw new ArgumentNullException(nameof(importRoot));
            }
            if (Origin == origin)
            {
                return new OperationError("Org unit already connected. Please do an update in stead", OperationFailure.BadState);
            }

            //Switch the origin of the root
            Origin = origin;
            ExternalOriginUuid = importRoot.Uuid;
            Name = importRoot.Name;

            foreach (var organizationUnit in importRoot.Children.Select(child => child.ToOrganizationUnit(origin, Organization)).ToList())
            {
                Children.Add(organizationUnit);
            }

            return Maybe<OperationError>.None;
        }

        public void ConvertToNativeKitosUnit()
        {
            if (Origin == OrganizationUnitOrigin.Kitos)
            {
                throw new InvalidOperationException("Already a KITOS unit");
            }

            Origin = OrganizationUnitOrigin.Kitos;
            ExternalOriginUuid = null;
        }

        public bool IsNativeKitosUnit()
        {
            return Origin == OrganizationUnitOrigin.Kitos;
        }

        public bool IsUsed()
        {
            return Using.Any() || EconomyStreams.Any() || ResponsibleForItContracts.Any() || Rights.Any();
        }

        public Maybe<OperationError> AddChild(OrganizationUnit child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));

            if (child.Organization != Organization)
            {
                return new OperationError($"child with uuid {child.Uuid} is from a different organization", OperationFailure.BadInput);
            }

            if (Children.Any(c => c.Uuid == child.Uuid))
            {
                return new OperationError($"child with uuid {child.Uuid} already added", OperationFailure.BadInput);
            }

            Children.Add(child);
            child.Parent = this;

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                return new OperationError($"newName must be defined in this unit with id {Id}", OperationFailure.BadInput);
            }

            if (newName.Length > MaxNameLength)
            {
                return new OperationError($"newName exceeds the max length of {MaxNameLength}", OperationFailure.BadInput);
            }

            Name = newName;
            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> RemoveChild(OrganizationUnit child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (!Children.Remove(child))
            {
                return new OperationError($"Child with id:{child.Id} could not be removed from this unit with id {Id} as it is not a child of this unit", OperationFailure.BadInput);
            }

            child.ResetParent();
            return Maybe<OperationError>.None;
        }

        public OrganizationUnitRegistrationDetails GetUnitRegistrations()
        {
            return new OrganizationUnitRegistrationDetails
            (
                Rights.ToList(),
                ResponsibleForItContracts.ToList(),
                GetUnitPayments().ToList(),
                Using.Where(x => x.ResponsibleItSystemUsage != null).Select(x => x.ResponsibleItSystemUsage).ToList(),
                Using.Select(x => x.ItSystemUsage).ToList()
            );
        }

        public Maybe<OrganizationUnitRight> GetRight(int rightId)
        {
            return Rights.FirstOrDefault(x => x.Id == rightId);
        }

        private IEnumerable<PaymentRegistrationDetails> GetUnitPayments()
        {
            var internContracts = EconomyStreams.Where(x => x.InternPaymentFor != null).Select(x => x.InternPaymentFor).ToList();
            var externContracts = EconomyStreams.Where(x => x.ExternPaymentFor != null).Select(x => x.ExternPaymentFor).ToList();
            var contracts = internContracts.Concat(externContracts).GroupBy(x => x.Id).Select(x => x.First()).ToList();

            return contracts
                .Select(itContract =>
                    new PaymentRegistrationDetails(
                        itContract,
                        itContract.GetInternalPaymentsForUnit(Id),
                        itContract.GetExternalPaymentsForUnit(Id)))
                .ToList();
        }

        public void ResetParent()
        {
            Parent = null;
        }

        public void MarkAsDirty()
        {
            LastChanged = DateTime.UtcNow;
        }
    }
}
