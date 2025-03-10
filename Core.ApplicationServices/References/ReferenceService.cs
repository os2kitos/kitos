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
            ExternalReferenceProperties externalReferenceProperties)
        {
            return GetRootEntityAndCheckWriteAccess(rootId, rootType)
                .Bind(root => root
                    .AddExternalReference(new ExternalReference { Created = _operationClock.Now }
                        .Transform(x => MapPropertiesToExternalReference(x, externalReferenceProperties))
                    )
                    .Match<Result<ExternalReference, OperationError>>
                    (
                        onSuccess: createdReference =>
                        {
                            _domainEvents.Raise(new EntityCreatedEvent<ExternalReference>(createdReference));
                            if (externalReferenceProperties.MasterReference)
                            {
                                root.SetMasterReference(createdReference);
                            }

                            RaiseRootUpdated(root);
                            _referenceRepository.SaveRootEntity(root);
                            return createdReference;
                        },
                        onFailure: error => error
                    )
                );
        }

        public Result<ExternalReference, OperationError> UpdateReference(
            int rootId,
            ReferenceRootType rootType,
            Guid referenceUuid,
            ExternalReferenceProperties externalReferenceProperties)
        {
            return GetRootEntityAndCheckWriteAccess(rootId, rootType)
                .Bind(root =>
                    _referenceRepository
                        .GetByUuid(referenceUuid)
                        .Match(reference =>
                            {
                                reference.Transform(x => MapPropertiesToExternalReference(x, externalReferenceProperties));

                                if (externalReferenceProperties.MasterReference)
                                {
                                    root.SetMasterReference(reference);
                                }
                                else if (reference.IsMasterReference())
                                {
                                    return new OperationError("A master reference must be defined", OperationFailure.BadInput);
                                }

                                _domainEvents.Raise(new EntityCreatedEvent<ExternalReference>(reference));
                                RaiseRootUpdated(root);
                                _referenceRepository.SaveRootEntity(root);

                                return Result<ExternalReference, OperationError>.Success(reference);
                            },
                            () => new OperationError($"Reference with uuid: {referenceUuid} was not found", OperationFailure.NotFound))
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

                    //External references with uuids not included in the update are going to be deleted
                    var uuidsToUpdateHashSet = referenceList
                        .Where(referenceToUpdate => referenceToUpdate.Uuid.HasValue)
                        .Select(referenceToUpdate => referenceToUpdate.Uuid)
                        .ToHashSet();

                    var referencesToDelete = root
                        .ExternalReferences
                        .Where(externalReference => !uuidsToUpdateHashSet.Contains(externalReference.Uuid));

                    //Delete all references not explicitly referenced by the input parameters
                    var deleteResult = DeleteExternalReferences(root, referencesToDelete);
                    if (deleteResult.Failed)
                        return new OperationError("Failed to delete old references", deleteResult.Error);

                    if (referenceList.Any())
                    {
                        var masterReferencesCount = referenceList.Count(x => x.MasterReference);

                        if(masterReferencesCount > 1)
                        {
                            return new OperationError("Only one reference can be master reference", OperationFailure.BadInput);
                        }
                        
                        //Order to make sure the first update occurs on the future master reference. In that way, we cannot get into a situation where we temporarily have no master ref (update will fail)
                        var referencesWithMasterAsHead = referenceList.OrderByDescending(r => r.MasterReference).ToList();
                        foreach (var externalReferenceProperties in referencesWithMasterAsHead)
                        {
                            //Replace references, which are identified by the update using uuids
                            //If no existing reference is found by uuid, it is considered an input error
                            if (externalReferenceProperties.Uuid.HasValue)
                            {
                                var uuid = externalReferenceProperties.Uuid.Value;
                                var updateReferenceResult = UpdateReference(rootId, rootType, uuid, externalReferenceProperties);
                                if (updateReferenceResult.Failed)
                                    return updateReferenceResult.Error;
                            }
                            //If uuid is null a new reference is going to be created
                            else
                            {
                                var addReferenceResult = AddReference(rootId, rootType, externalReferenceProperties);

                                if (addReferenceResult.Failed)
                                    return new OperationError($"Failed to add reference with data:{JsonConvert.SerializeObject(externalReferenceProperties)}. Error:{addReferenceResult.Error.Message.GetValueOrEmptyString()}", addReferenceResult.Error.FailureType);
                            }
                        }

                        _referenceRepository.SaveRootEntity(root);
                    }

                    return Maybe<OperationError>.None;

                }, () => new OperationError(OperationFailure.NotFound));

            if (error.IsNone)
                transaction.Commit();
            else
                transaction.Rollback();

            return error;
        }

        private static ExternalReference MapPropertiesToExternalReference(ExternalReference reference, ExternalReferenceProperties properties)
        {
            reference.Title = properties.Title;
            reference.ExternalReferenceId = properties.DocumentId;
            reference.URL = properties.Url;

            return reference;
        }

        private Result<IEntityWithExternalReferences, OperationError> GetRootEntityAndCheckWriteAccess(int rootId, ReferenceRootType rootType)
        {
            return _referenceRepository
                .GetRootEntity(rootId, rootType)
                .Match(WithWriteAccess,
                    () => new OperationError("Root entity could not be found", OperationFailure.NotFound));
        }

        private Result<IEntityWithExternalReferences, OperationError> WithWriteAccess(IEntityWithExternalReferences root)
        {
            return _authorizationContext.AllowModify(root)
                ? Result<IEntityWithExternalReferences, OperationError>.Success(root)
                : new OperationError("Not allowed to modify root entity", OperationFailure.Forbidden);
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
