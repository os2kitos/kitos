using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Shared.Write;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.References;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;

using Newtonsoft.Json;

namespace Core.ApplicationServices.References
{
    public class ReferenceService : IReferenceService
    {
        private readonly IReferenceRepository _referenceRepository;
        private readonly IItSystemRepository _itSystemRepository;
        private readonly IItSystemUsageRepository _systemUsageRepository;
        private readonly IItContractRepository _contractRepository;
        private readonly IItProjectRepository _projectRepository;
        private readonly IDataProcessingRegistrationRepository _dataProcessingRegistrationRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IOperationClock _operationClock;
        private readonly IDomainEvents _domainEvents;


        public ReferenceService(
            IReferenceRepository referenceRepository,
            IItSystemRepository itSystemRepository,
            IItSystemUsageRepository systemUsageRepository,
            IItContractRepository contractRepository,
            IItProjectRepository projectRepository,
            IDataProcessingRegistrationRepository dataProcessingRegistrationRepository,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IOperationClock operationClock,
            IDomainEvents domainEvents)
        {
            _referenceRepository = referenceRepository;
            _itSystemRepository = itSystemRepository;
            _systemUsageRepository = systemUsageRepository;
            _contractRepository = contractRepository;
            _projectRepository = projectRepository;
            _dataProcessingRegistrationRepository = dataProcessingRegistrationRepository;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _operationClock = operationClock;
            _domainEvents = domainEvents;
        }


        public Result<ExternalReference, OperationError> AddReference(
            int rootId,
            ReferenceRootType rootType,
            string title,
            string externalReferenceId,
            string url)
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
                                Created = _operationClock.Now,
                            })
                            .Match<Result<ExternalReference, OperationError>>
                            (
                                onSuccess: createdReference =>
                                {
                                    _domainEvents.Raise(new EntityCreatedEvent<ExternalReference>(createdReference));
                                    RaiseRootUpdated(root);
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

                        _domainEvents.Raise(new EntityDeletedEvent<ExternalReference>(referenceAndOwner.externalReference));
                        RaiseRootUpdated(referenceAndOwner.owner);
                        _referenceRepository.Delete(referenceAndOwner.externalReference);
                        return referenceAndOwner.externalReference;
                    })
                    .Match
                    (
                        onValue: result => result,
                        onNone: () => OperationFailure.NotFound
                    );

        }

        private void RaiseRootUpdated(IEntityWithExternalReferences owner)
        {
            switch (owner)
            {
                case DataProcessingRegistration dataProcessingRegistration:
                    _domainEvents.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(dataProcessingRegistration));
                    break;
                case ItContract itContract:
                    _domainEvents.Raise(new EntityUpdatedEvent<ItContract>(itContract));
                    break;
                case ItProject itProject:
                    _domainEvents.Raise(new EntityUpdatedEvent<ItProject>(itProject));
                    break;
                case ItSystem itSystem:
                    _domainEvents.Raise(new EntityUpdatedEvent<ItSystem>(itSystem));
                    break;
                case ItSystemUsage itSystemUsage:
                    _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(itSystemUsage));
                    break;
            }
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

        public Result<IEnumerable<ExternalReference>, OperationFailure> DeleteByDataProcessingRegistrationId(int id)
        {
            return _dataProcessingRegistrationRepository
                .GetById(id)
                .Select(DeleteExternalReferences)
                .Match(r => r, () => OperationFailure.NotFound);
        }

        public Maybe<OperationError> BatchUpdateExternalReferences(
            ReferenceRootType rootType,
            int rootId,
            IEnumerable<UpdatedExternalReferenceProperties> externalReferences)
        {
            using var transaction = _transactionManager.Begin();

            var error = _referenceRepository
                .GetRootEntity(rootId, rootType)
                .Match(root =>
                {
                    //Clear existing state
                    root.ClearMasterReference();
                    var deleteResult = DeleteExternalReferences(root);
                    if (deleteResult.Failed)
                        return new OperationError("Failed to delete old references", deleteResult.Error);

                    var newReferences = externalReferences.ToList();
                    if (newReferences.Any())
                    {
                        var masterReferencesCount = newReferences.Count(x => x.MasterReference);

                        switch (masterReferencesCount)
                        {
                            case < 1:
                                return new OperationError("A master reference must be defined", OperationFailure.BadInput);
                            case > 1:
                                return new OperationError("Only one reference can be master reference", OperationFailure.BadInput);
                        }

                        foreach (var referenceProperties in newReferences)
                        {
                            var result = AddReference(rootId, rootType, referenceProperties.Title, referenceProperties.DocumentId, referenceProperties.Url);

                            if (result.Failed)
                                return new OperationError($"Failed to add reference with data:{JsonConvert.SerializeObject(referenceProperties)}. Error:{result.Error.Message.GetValueOrEmptyString()}", result.Error.FailureType);

                            if (referenceProperties.MasterReference)
                            {
                                var masterReferenceResult = root.SetMasterReference(result.Value);
                                if (masterReferenceResult.Failed)
                                    return new OperationError($"Failed while setting the master reference:{masterReferenceResult.Error.Message.GetValueOrEmptyString()}", masterReferenceResult.Error.FailureType);
                            }
                        }
                    }
                    return Maybe<OperationError>.None;

                }, () => new OperationError(OperationFailure.NotFound));

            if (error.IsNone)
                transaction.Commit();

            return error;
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

            using var transaction = _transactionManager.Begin();
            var systemExternalReferences = root.ExternalReferences.ToList();

            if (systemExternalReferences.Count == 0)
            {
                return systemExternalReferences;
            }

            foreach (var reference in systemExternalReferences)
            {
                _domainEvents.Raise(new EntityDeletedEvent<ExternalReference>(reference));
                _referenceRepository.Delete(reference);
            }

            RaiseRootUpdated(root);
            transaction.Commit();
            return systemExternalReferences;
        }
    }
}
