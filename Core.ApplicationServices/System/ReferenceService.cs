using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Result;
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
                return OperationFailure.NotFound;
            }

            if (! _authorizationContext.AllowModify(system))
            {
                return OperationFailure.Forbidden;
            }

            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                var systemExternalReferences = system.ExternalReferences.ToList();

                if (systemExternalReferences.Count == 0)
                {
                    return systemExternalReferences;
                }

                foreach (var reference in systemExternalReferences)
                {
                    _referenceRepository.Delete(reference);
                }
                transaction.Commit();
                return systemExternalReferences;
            }
            
        }

    }
}
