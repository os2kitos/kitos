using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel;
using Core.DomainServices.Model.Result;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.System;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.System
{
    public class ReferenceService : IReferenceService
    {
        private readonly IReferenceRepository _referenceRepository;
        private readonly IItSystemRepository _itSystemRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;

        public ReferenceService(
            IReferenceRepository referenceRepository, 
            IItSystemRepository itSystemRepository, 
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager)
        {
            _referenceRepository = referenceRepository;
            _itSystemRepository = itSystemRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
        }

        public Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemId(int systemId)
        {
            var system = _itSystemRepository.GetSystem(systemId);
            if (system == null)
            {
                return Result<IEnumerable<ExternalReference>, OperationFailure>.Failure(OperationFailure.NotFound);
            }

            if (! _authorizationContext.AllowModify(system))
            {
                return Result<IEnumerable<ExternalReference>, OperationFailure>.Failure(OperationFailure.Forbidden);
            }

            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
                var systemExternalReferences = system.ExternalReferences.ToList();

                if (systemExternalReferences.Count == 0)
                {
                    return Result<IEnumerable<ExternalReference>, OperationFailure>.Success(systemExternalReferences);
                }

                foreach (var reference in systemExternalReferences)
                {
                    _referenceRepository.Delete(reference);
                }
                transaction.Commit();
                return Result<IEnumerable<ExternalReference>, OperationFailure>.Success(systemExternalReferences);
            }
            
        }

    }
}
