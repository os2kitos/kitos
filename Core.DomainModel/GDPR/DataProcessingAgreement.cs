using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItContract;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingAgreement :
        HasRightsEntity<DataProcessingAgreement, DataProcessingAgreementRight, DataProcessingAgreementRole>,
        IHasName,
        IOwnedByOrganization,
        IDataProcessingModule,
        IEntityWithExternalReferences

    {
        public DataProcessingAgreement()
        {
            ExternalReferences = new List<ExternalReference>();
        }

        public static bool IsNameValid(string name) => !string.IsNullOrWhiteSpace(name) &&
                                                       name.Length <= DataProcessingAgreementConstraints.MaxNameLength;

        public Maybe<OperationError> SetName(string newName)
        {
            if (IsNameValid(newName))
            {
                Name = newName;
                return Maybe<OperationError>.None;
            }
            return new OperationError("Name does not meet validation criteria", OperationFailure.BadInput);
        }

        public string Name { get; set; }

        public int OrganizationId { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public virtual ICollection<DataProcessingAgreementReadModel> ReadModels { get; set; }

        public virtual ICollection<ItSystemUsage.ItSystemUsage> SystemUsages { get; set; }

        public IEnumerable<DataProcessingAgreementRight> GetRights(int roleId)
        {
            return Rights.Where(x => x.RoleId == roleId);
        }

        public IEnumerable<ItSystem.ItSystem> GetAssignedSystems() => SystemUsages.Select(x => x.ItSystem);


        public Result<ItSystem.ItSystem, OperationError> AssignSystem(ItSystem.ItSystem system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));

            var usageResult = system.GetUsageForOrganization(OrganizationId);
            if (usageResult.IsNone)
                return new OperationError($"System is not in use in organization with id {OrganizationId}", OperationFailure.BadInput);

            var usage = usageResult.Value;

            if (GetAssignedSystemUsage(usage.Id).HasValue)
                return new OperationError("System usage is already assigned", OperationFailure.Conflict);

            SystemUsages.Add(usage);

            return system;
        }

        public Result<ItSystem.ItSystem, OperationError> RemoveSystem(ItSystem.ItSystem system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));

            var usageResult = system.GetUsageForOrganization(OrganizationId);
            if (usageResult.IsNone)
                return new OperationError($"System is not in use in organization with id {OrganizationId}", OperationFailure.BadInput);

            var usage = usageResult.Value;

            if (GetAssignedSystemUsage(usage.Id).IsNone)
                return new OperationError("Usage not assigned", OperationFailure.BadInput);

            SystemUsages.Remove(usage);

            return system;
        }

        private Maybe<ItSystemUsage.ItSystemUsage> GetAssignedSystemUsage(int usageId)
        {
            return SystemUsages.FirstOrDefault(x => x.Id == usageId).FromNullable();
        }

        public Result<DataProcessingAgreementRight, OperationError> AssignRoleToUser(DataProcessingAgreementRole role, User user)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (HasRight(role, user))
                return new OperationError("Existing right for same role found for the same user", OperationFailure.Conflict);

            var agreementRight = new DataProcessingAgreementRight
            {
                Role = role,
                User = user,
                Object = this
            };

            Rights.Add(agreementRight);

            return agreementRight;
        }

        private bool HasRight(DataProcessingAgreementRole role, User user)
        {
            return GetRight(role, user).HasValue;
        }

        private Maybe<DataProcessingAgreementRight> GetRight(DataProcessingAgreementRole role, User user)
        {
            return GetRights(role.Id).FirstOrDefault(x => x.UserId == user.Id);
        }

        public Result<DataProcessingAgreementRight, OperationError> RemoveRole(DataProcessingAgreementRole role, User user)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (user == null) throw new ArgumentNullException(nameof(user));

            return GetRight(role, user)
                .Match<Result<DataProcessingAgreementRight, OperationError>>
                (
                    right =>
                    {
                        Rights.Remove(right);
                        return right;
                    },
                    () => new OperationError($"Role with id {role.Id} is not assigned to user with id ${user.Id}",
                        OperationFailure.BadInput)
                );

        }

        public virtual ICollection<ExternalReference> ExternalReferences { get; set; }

        public ReferenceRootType GetRootType() => ReferenceRootType.DataProcessingAgreement;

        public Result<ExternalReference, OperationError> AddExternalReference(ExternalReference newReference)
        {
            return new AddReferenceCommand(this).AddExternalReference(newReference);
        }

        public Result<ExternalReference, OperationError> SetMasterReference(ExternalReference newReference)
        {
            Reference = newReference;
            return newReference;
        }

        public int? ReferenceId { get; set; }

        public virtual ExternalReference Reference { get; set; }
    }
}
