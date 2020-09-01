using System;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.GDPR
{
    public class DataProcessingAgreementService : IDataProcessingAgreementService
    {
        private readonly IAuthorizationContext _authorizationContext;

        public DataProcessingAgreementService(IAuthorizationContext authorizationContext)
        {
            _authorizationContext = authorizationContext;
        }

        public Result<DataProcessingAgreement, OperationError> Create(int organizationId, string name)
        {
            if(!_authorizationContext.AllowCreate<DataProcessingAgreement>(organizationId))
                return new OperationError
            throw new NotImplementedException();
        }

        public Maybe<OperationError> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Result<DataProcessingAgreement, OperationError> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Result<IQueryable<DataProcessingAgreement>, OperationError> GetByOrganizationId(int organizationId)
        {
            throw new NotImplementedException();
        }
    }
}
