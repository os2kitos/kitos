﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingAgreement : HasRightsEntity<DataProcessingAgreement, DataProcessingAgreementRight, DataProcessingAgreementRole>, IHasName, IOwnedByOrganization, IDataProcessingModule

    {
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

        public IEnumerable<DataProcessingAgreementRight> GetRights(int roleId)
        {
            return Rights.Where(x => x.RoleId == roleId);
        }

        public Result<DataProcessingAgreementRight, OperationError> AssignRoleToUser(DataProcessingAgreementRole role, User user)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (GetRights(role.Id).Any(x => x.UserId == user.Id))
                return new OperationError("Existing right for same role found for the same user", OperationFailure.Conflict);

            var agreementRight = new DataProcessingAgreementRight
            {
                Role = role,
                User = user,
                Object = this
            };

            //TODO: Check if this even works
            Rights.Add(agreementRight);

            return agreementRight;
        }

        public Result<DataProcessingAgreementRight, OperationError> RemoveRole(DataProcessingAgreementRole role, User user)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (user == null) throw new ArgumentNullException(nameof(user));

            var agreementRight = GetRights(role.Id).FirstOrDefault(x => x.UserId == user.Id && x.RoleId == role.Id);
            if (agreementRight == null)
                return new OperationError($"Role with id {role.Id} is not assigned to user with id ${user.Id}", OperationFailure.BadInput);

            //TODO: Check if this even works
            agreementRight.Object = null;
            Rights.Remove(agreementRight);

            return agreementRight;
        }
    }
}
