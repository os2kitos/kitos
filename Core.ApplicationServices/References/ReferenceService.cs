using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.References
{
    public class ReferenceService : IReferenceService
    {
        private readonly IReferenceRepository _referenceRepository;
        private readonly IItSystemRepository _itSystemRepository;
        private readonly IItSystemUsageRepository _systemUsageRepository;
        private readonly IItContractRepository _contractRepository;
        private readonly IItProjectRepository _projectRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IOperationClock _operationClock;


        public ReferenceService(
            IReferenceRepository referenceRepository,
            IItSystemRepository itSystemRepository,
            IItSystemUsageRepository systemUsageRepository,
            IItContractRepository contractRepository,
            IItProjectRepository projectRepository,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IOperationClock operationClock)
        {
            _referenceRepository = referenceRepository;
            _itSystemRepository = itSystemRepository;
            _systemUsageRepository = systemUsageRepository;
            _contractRepository = contractRepository;
            _projectRepository = projectRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _operationClock = operationClock;
        }


        public Result<ExternalReference, OperationError> AddReference(
            int rootId,
            ReferenceRootType rootType,
            string title,
            string externalReferenceId,
            string url,
            Display display)
        {
            return _referenceRepository
                .GetRootEntity(rootId, rootType)
                .Match
                (
                    onValue: root =>
                    {
                        if (!_authorizationContext.AllowModify(root))
                        {
                            return new OperationError("Not allowed to modify root entity", OperationFailure.Forbidden);
                        }

                        return root
                            .AddExternalReference(new ExternalReference
                            {
                                Title = title,
                                ExternalReferenceId = externalReferenceId,
                                URL = url,
                                Display = display,
                                Created = _operationClock.Now,
                            })
                            .Match<Result<ExternalReference, OperationError>>
                            (
                                onSuccess: createdReference =>
                                {
                                    _referenceRepository.SaveRootEntity(root);
                                    return createdReference;
                                },
                                onFailure: error => error
                            );
                    },
                    onNone: () => new OperationError("Root entity could not be found", OperationFailure.NotFound)
                );
        }

        public Result<ExternalReference, OperationFailure> DeleteByReferenceId(int referenceId)
        {
            return
                _referenceRepository.Get(referenceId)
                    .Select(externalReference => new { externalReference, owner = externalReference.GetOwner() })
                    .Select<Result<ExternalReference, OperationFailure>>(referenceAndOwner =>
                    {
                        if (!_authorizationContext.AllowModify(referenceAndOwner.owner))
                        {
                            return OperationFailure.Forbidden;
                        }

                        _referenceRepository.Delete(referenceAndOwner.externalReference);
                        return referenceAndOwner.externalReference;
                    })
                    .Match<Result<ExternalReference, OperationFailure>>
                    (
                        onValue: result => result,
                        onNone: () => OperationFailure.NotFound
                    );

        }

        public Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemId(int systemId)
        {
            var system = _itSystemRepository.GetSystem(systemId);
            return DeleteExternalReferences(system);
        }

        public Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemUsageId(int usageId)
        {
            var itSystemUsage = _systemUsageRepository.GetSystemUsage(usageId);
            return DeleteExternalReferences(itSystemUsage);
        }

        public Result<IEnumerable<ExternalReference>, OperationFailure> DeleteByContractId(int contractId)
        {
            var contract = _contractRepository.GetById(contractId);
            return DeleteExternalReferences(contract);
        }

        public Result<IEnumerable<ExternalReference>, OperationFailure> DeleteByProjectId(int projectId)
        {
            var project = _projectRepository.GetById(projectId);
            return DeleteExternalReferences(project);
        }

        private Result<IEnumerable<ExternalReference>, OperationFailure> DeleteExternalReferences(IEntityWithExternalReferences root)
        {
            if (root == null)
            {
                return OperationFailure.NotFound;
            }

            if (!_authorizationContext.AllowModify(root))
            {
                return OperationFailure.Forbidden;
            }

            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                var systemExternalReferences = root.ExternalReferences.ToList();

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
