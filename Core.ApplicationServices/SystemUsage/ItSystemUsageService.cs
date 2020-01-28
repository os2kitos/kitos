using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Options;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.System;

namespace Core.ApplicationServices.SystemUsage
{
    public class ItSystemUsageService : IItSystemUsageService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItSystemRepository _systemRepository;
        private readonly IItContractRepository _contractRepository;
        private readonly IOptionsService<SystemRelation, RelationFrequencyType> _frequencyService;
        private readonly IOrganizationalUserContext _userContext;

        public ItSystemUsageService(
            IGenericRepository<ItSystemUsage> usageRepository,
            IAuthorizationContext authorizationContext,
            IItSystemRepository systemRepository,
            IItContractRepository contractRepository,
            IOptionsService<SystemRelation, RelationFrequencyType> frequencyService,
            IOrganizationalUserContext userContext)
        {
            _usageRepository = usageRepository;
            _authorizationContext = authorizationContext;
            _systemRepository = systemRepository;
            _contractRepository = contractRepository;
            _frequencyService = frequencyService;
            _userContext = userContext;
        }

        public Result<ItSystemUsage, OperationFailure> Add(ItSystemUsage newSystemUsage, User objectOwner)
        {
            // create the system usage
            var existing = GetByOrganizationAndSystemId(newSystemUsage.OrganizationId, newSystemUsage.ItSystemId);
            if (existing != null)
            {
                return OperationFailure.Conflict;
            }

            if (!_authorizationContext.AllowCreate<ItSystemUsage>(newSystemUsage))
            {
                return OperationFailure.Forbidden;
            }

            var itSystem = _systemRepository.GetSystem(newSystemUsage.ItSystemId);
            if (itSystem == null)
            {
                return OperationFailure.BadInput;
            }

            if (!_authorizationContext.AllowReads(itSystem))
            {
                return OperationFailure.Forbidden;
            }

            //Cannot create system usage in an org where the logical it system is unavailable to the users.
            if (!AllowUsageInTargetOrganization(newSystemUsage, itSystem))
            {
                return OperationFailure.Forbidden;
            }

            var usage = _usageRepository.Create();

            usage.ItSystemId = newSystemUsage.ItSystemId;
            usage.OrganizationId = newSystemUsage.OrganizationId;
            usage.ObjectOwner = objectOwner;
            usage.LastChangedByUser = objectOwner;
            usage.DataLevel = newSystemUsage.DataLevel;
            usage.ContainsLegalInfo = newSystemUsage.ContainsLegalInfo;
            usage.AssociatedDataWorkers = newSystemUsage.AssociatedDataWorkers;
            _usageRepository.Insert(usage);
            _usageRepository.Save(); // abuse this as UoW

            return usage;
        }

        private static bool AllowUsageInTargetOrganization(ItSystemUsage newSystemUsage, ItSystem itSystem)
        {
            return
                    newSystemUsage.OrganizationId == itSystem.OrganizationId || //It system is defined in same org as usage
                    itSystem.AccessModifier == AccessModifier.Public;           //It system is public and it is OK to place usages outside the owning organization
        }

        public Result<ItSystemUsage, OperationFailure> Delete(int id)
        {
            var itSystemUsage = GetById(id);
            if (itSystemUsage == null)
            {
                return OperationFailure.NotFound;
            }
            if (!_authorizationContext.AllowDelete(itSystemUsage))
            {
                return OperationFailure.Forbidden;
            }

            // delete it system usage
            _usageRepository.DeleteByKeyWithReferencePreload(id);
            _usageRepository.Save();
            return itSystemUsage;
        }

        public ItSystemUsage GetByOrganizationAndSystemId(int organizationId, int systemId)
        {
            return _usageRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .FirstOrDefault(u => u.ItSystemId == systemId);
        }

        public ItSystemUsage GetById(int usageId)
        {
            return _usageRepository.GetByKey(usageId);
        }

        public Result<SystemRelation, OperationError> AddRelation(
            int sourceId,
            int destinationId,
            int? interfaceId,
            string description,
            string linkName,
            string linkUrl,
            int? frequencyId,
            int? contractId)
        {
            var source = _usageRepository.GetByKey(sourceId);
            var destination = _usageRepository.GetByKey(destinationId);
            var targetContract = Maybe<ItContract>.None;
            var targetFrequency = Maybe<RelationFrequencyType>.None;

            if (source == null)
                return new OperationError("Source not found", OperationFailure.NotFound);

            if (destination == null)
                return new OperationError("Destination id does not point to a valid system usage", OperationFailure.BadInput);

            if (!_authorizationContext.AllowModify(source))
                return Result<SystemRelation, OperationError>.Failure(OperationFailure.Forbidden);

            if (frequencyId.HasValue)
            {
                targetFrequency = _frequencyService
                    .GetAvailableOptions(source.OrganizationId)
                    .FirstOrDefault(x => x.Id == frequencyId.Value)
                    .FromNullable();

                if (!targetFrequency.HasValue)
                    return new OperationError("Frequency type is not available in the organization", OperationFailure.BadInput);
            }

            if (contractId.HasValue)
            {
                targetContract = _contractRepository.GetById(contractId.Value).FromNullable();
                if (!targetContract.HasValue)
                    return new OperationError("Contract id doew not point to a valid contract", OperationFailure.BadInput);
            }

            var result = source.AddUsageRelationTo(_userContext.UserEntity, destination, interfaceId, description, linkName, linkUrl, targetFrequency, targetContract);
            if (result.Ok)
            {
                _usageRepository.Save();
            }
            return result;
        }

    }
}
