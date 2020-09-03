using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.GDPR;

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingAgreementReadModelService : IDataProcessingAgreementReadModelService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDataProcessingAgreementReadModelRepository _repository;

        public DataProcessingAgreementReadModelService(IAuthorizationContext authorizationContext, IDataProcessingAgreementReadModelRepository repository)
        {
            _authorizationContext = authorizationContext;
            _repository = repository;
        }

        public Result<IQueryable<DataProcessingAgreementReadModel>, OperationError> GetByOrganizationId(int organizationId)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            return Result<IQueryable<DataProcessingAgreementReadModel>, OperationError>.Success(_repository.GetByOrganizationId(organizationId));
        }
    }
}
