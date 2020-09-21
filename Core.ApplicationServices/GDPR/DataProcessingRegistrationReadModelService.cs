using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;
using Core.DomainServices.Repositories.GDPR;

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingRegistrationReadModelService : IDataProcessingRegistrationReadModelService
    {
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IDataProcessingRegistrationReadModelRepository _repository;

        public DataProcessingRegistrationReadModelService(IAuthorizationContext authorizationContext, IDataProcessingRegistrationReadModelRepository repository)
        {
            _authorizationContext = authorizationContext;
            _repository = repository;
        }

        public Result<IQueryable<DataProcessingRegistrationReadModel>, OperationError> GetByOrganizationId(int organizationId)
        {
            if (_authorizationContext.GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            return Result<IQueryable<DataProcessingRegistrationReadModel>, OperationError>.Success(_repository.GetByOrganizationId(organizationId));
        }
    }
}
