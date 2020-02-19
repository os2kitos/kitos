using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.References
{
    public class ReferenceService : IReferenceService
    {
        private readonly IReferenceRepository _referenceRepository;
        private readonly IItSystemRepository _itSystemRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IOrganizationalUserContext _userContext;
        private readonly IOperationClock _operationClock;


        public ReferenceService(
            IReferenceRepository referenceRepository,
            IItSystemRepository itSystemRepository,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IOrganizationalUserContext userContext,
            IOperationClock operationClock)
        {
            _referenceRepository = referenceRepository;
            _itSystemRepository = itSystemRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _userContext = userContext;
            _operationClock = operationClock;
        }


        public Result<ExternalReference, OperationError> Create(
            int referenceOwnerId,
            ReferenceRootType referenceOwnerType,
            string title,
            string externalReferenceId,
            string url,
            Display display)
        {
            return _referenceRepository
                .GetRootEntity(referenceOwnerId, referenceOwnerType)
                .Match
                (
                    onValue: root =>
                    {
                        if (!_authorizationContext.AllowModify(root))
                        {
                            return new OperationError("Not allowed to modify root entity",OperationFailure.Forbidden);
                        }

                        return root
                            .AddExternalReference(new ExternalReference
                            {
                                Title = title,
                                ExternalReferenceId = externalReferenceId,
                                URL = url,
                                Display = display,
                                ObjectOwner = _userContext.UserEntity,
                                LastChangedByUser = _userContext.UserEntity,
                                Created = _operationClock.Now,
                                LastChanged = _operationClock.Now
                            })
                            .Match<Result<ExternalReference, OperationError>>
                            (
                                onSuccess: createdReference =>
                                {
                                    _referenceRepository.Save(root);
                                    return createdReference;
                                },
                                onFailure: error => error
                            );
                    },
                    onNone: () => new OperationError("Root entity could not be found", OperationFailure.NotFound)
                );
        }

        public Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemId(int systemId)
        {
            var system = _itSystemRepository.GetSystem(systemId);
            if (system == null)
            {
                return OperationFailure.NotFound;
            }

            if (!_authorizationContext.AllowModify(system))
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
