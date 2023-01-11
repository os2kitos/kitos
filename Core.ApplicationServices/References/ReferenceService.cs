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
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.References;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.GDPR;
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

                        _domainEvents.Raise(new EntityBeingDeletedEvent<ExternalReference>(referenceAndOwner.externalReference));
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
            return DeleteExternalReferencesByRoot(system);
        }

        public Result<IEnumerable<ExternalReference>, OperationFailure> DeleteBySystemUsageId(int usageId)
        {
            var itSystemUsage = _systemUsageRepository.GetSystemUsage(usageId);
            return DeleteExternalReferencesByRoot(itSystemUsage);
        }

        public Result<IEnumerable<ExternalReference>, OperationFailure> DeleteByContractId(int contractId)
        {
            var contract = _contractRepository.GetById(contractId);
            return DeleteExternalReferencesByRoot(contract);
        }

        public Result<IEnumerable<ExternalReference>, OperationFailure> DeleteByDataProcessingRegistrationId(int id)
        {
            return _dataProcessingRegistrationRepository
                .GetById(id)
                .Select(DeleteExternalReferencesByRoot)
                .Match(r => r, () => OperationFailure.NotFound);
        }

        public Maybe<OperationError> UpdateExternalReferences(
            ReferenceRootType rootType,
            int rootId,
            IEnumerable<UpdatedExternalReferenceProperties> externalReferences)
        {
            using var transaction = _transactionManager.Begin();

            var error = _referenceRepository
                .GetRootEntity(rootId, rootType)
                .Match(root =>
                {
                    var referenceList = externalReferences.ToList();

                    var referencesToDelete = root.ExternalReferences.Where(externalReference => !referenceList.Any(referenceProperties => referenceProperties.Uuid == externalReference.Uuid));
                    var deleteResult = DeleteExternalReferences(root, referencesToDelete);
                    if(deleteResult.Failed)
                        return new OperationError("Failed to delete old references", deleteResult.Error);

                    if (referenceList.Any())
                    {
                        var masterReferencesCount = referenceList.Count(x => x.MasterReference);

                        switch (masterReferencesCount)
                        {
                            case < 1:
                                return new OperationError("A master reference must be defined", OperationFailure.BadInput);
                            case > 1:
                                return new OperationError("Only one reference can be master reference", OperationFailure.BadInput);
                        }
                        
                        var referencesToUpdate = new List<ExternalReference>();
                        foreach (var externalReferenceProperties in referenceList)
                        {
                            ExternalReference externalReference;
                            if (externalReferenceProperties.Uuid.HasValue)
                            {
                                var uuid = externalReferenceProperties.Uuid.Value;
                                var existingReferenceResult = _referenceRepository.GetByUuid(uuid);
                                if(existingReferenceResult.IsNone)
                                    return new OperationError($"External reference with uuid: {uuid} was not found", OperationFailure.NotFound);

                                externalReference = existingReferenceResult.Value;
                                MapExternalReference(externalReferenceProperties, externalReference);
                                referencesToUpdate.Add(externalReference);
                            }
                            else
                            {
                                var addReferenceResult = AddReference(rootId, rootType, externalReferenceProperties.Title, externalReferenceProperties.DocumentId, externalReferenceProperties.Url);

                                if (addReferenceResult.Failed)
                                    return new OperationError($"Failed to add reference with data:{JsonConvert.SerializeObject(externalReferenceProperties)}. Error:{addReferenceResult.Error.Message.GetValueOrEmptyString()}", addReferenceResult.Error.FailureType);

                                externalReference = addReferenceResult.Value;
                            }

                            if (!externalReferenceProperties.MasterReference) 
                                continue;

                            var masterReferenceResult = root.SetMasterReference(externalReference);
                            if (masterReferenceResult.Failed)
                                return new OperationError($"Failed while setting the master reference:{masterReferenceResult.Error.Message.GetValueOrEmptyString()}", masterReferenceResult.Error.FailureType);
                        }

                        _referenceRepository.UpdateRange(referencesToUpdate);
                    }

                    return Maybe<OperationError>.None;

                }, () => new OperationError(OperationFailure.NotFound));

            if (error.IsNone)
                transaction.Commit();

            return error;
        }

        private void MapExternalReference(UpdatedExternalReferenceProperties updatedProperties,
            ExternalReference externalReference)
        {
            externalReference.Title = updatedProperties.Title;
            externalReference.ExternalReferenceId = updatedProperties.DocumentId;
            externalReference.URL = updatedProperties.Url;
        }
        
        private Result<IEnumerable<ExternalReference>, OperationFailure> DeleteExternalReferencesByRoot(IEntityWithExternalReferences root)
        {
            if (root == null)
            {
                return OperationFailure.NotFound;
            }

            return DeleteExternalReferences(root, root.ExternalReferences);
        }

        private Result<IEnumerable<ExternalReference>, OperationFailure> DeleteExternalReferences(IEntityWithExternalReferences root, IEnumerable<ExternalReference> externalReferences)
        {
            if (!_authorizationContext.AllowModify(root))
            {
                return OperationFailure.Forbidden;
            }

            using var transaction = _transactionManager.Begin();
            var externalReferenceList = externalReferences.ToList();

            if (externalReferenceList.Count == 0)
            {
                return externalReferenceList;
            }

            foreach (var reference in externalReferenceList)
            {
                _domainEvents.Raise(new EntityBeingDeletedEvent<ExternalReference>(reference));
                _referenceRepository.Delete(reference);
            }
            RaiseRootUpdated(root);
            transaction.Commit();

            return Result<IEnumerable<ExternalReference>, OperationFailure>.Success(externalReferenceList);
        }

    }
}
