using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.GDPR;

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingAgreementService : IDataProcessingAgreementService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDataProcessingAgreementsRepository _repository;

        public DataProcessingAgreementService(IAuthorizationContext authorizationContext, IDataProcessingAgreementsRepository repository)
        {
            _authorizationContext = authorizationContext;
            _repository = repository;
        }

        public Result<DataProcessingAgreement, OperationError> Create(int organizationId, string name)
        {
            if (!_authorizationContext.AllowCreate<DataProcessingAgreement>(organizationId))
                return new OperationError(OperationFailure.Forbidden);

            if (string.IsNullOrWhiteSpace(name) || name.Length > DataProcessingAgreementConstraints.MaxNameLength)
                return new OperationError("Name does breaks invariant: 0 < nameLength <= 100", OperationFailure.BadInput);

            if (_repository.Search(organizationId, name).Any())
                return new OperationError("Existing DataProcessingAgreement", OperationFailure.Conflict);

            var dataProcessingAgreement = new DataProcessingAgreement()
            {
                OrganizationId = organizationId,
                Name = name
            };

            dataProcessingAgreement = _repository.Add(dataProcessingAgreement);

            return dataProcessingAgreement;
        }

        public Result<DataProcessingAgreement, OperationError> Delete(int id)
        {
            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var agreementToDelete = result.Value;

            if (!_authorizationContext.AllowDelete(agreementToDelete))
                return new OperationError(OperationFailure.Forbidden);

            _repository.DeleteById(id);

            return agreementToDelete;
        }

        public Result<DataProcessingAgreement, OperationError> Get(int id)
        {
            var result = _repository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var agreement = result.Value;

            if (!_authorizationContext.AllowReads(agreement))
                return new OperationError(OperationFailure.Forbidden);

            return agreement;
        }
    }
}
