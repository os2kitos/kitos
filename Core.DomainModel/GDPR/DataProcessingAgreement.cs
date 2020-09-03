using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingAgreement : Entity, IHasName, IOwnedByOrganization, IDataProcessingModule

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
    }
}
