﻿using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingAgreement : HasRightsEntity<DataProcessingAgreement, DataProcessingAgreementRight, DataProcessingAgreementRole>, IHasName, IOwnedByOrganization, IDataProcessingModule

    {
        public static  bool IsNameValid(string name) => !string.IsNullOrWhiteSpace(name) &&
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
    }
}
