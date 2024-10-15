using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.LocalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;

namespace Core.ApplicationServices.LocalOptions
{
    public class GenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType> : IGenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType>
    where TLocalOptionType : LocalOptionEntity<TOptionType>, new ()
    where TOptionType : OptionEntity<TReferenceType>
    {
        private readonly IGenericRepository<TOptionType> _optionsRepository;
        private readonly IGenericRepository<TLocalOptionType> _localOptionRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IDomainEvents _domainEvents;

        public GenericLocalOptionsService(IGenericRepository<TOptionType> optionsRepository,
            IGenericRepository<TLocalOptionType> localOptionRepository,
            IAuthorizationContext authorizationContext, 
            IEntityIdentityResolver identityResolver,
            IDomainEvents domainEvents)
        {
            _optionsRepository = optionsRepository;
            _localOptionRepository = localOptionRepository;
            _authorizationContext = authorizationContext;
            _identityResolver = identityResolver;
            _domainEvents = domainEvents;
        }


        public Result<IEnumerable<TOptionType>, OperationError> GetByOrganizationUuid(Guid organizationUuid)
        {
            var globalOptions = GetGlobalOptionsAsQueryable()
                .ToList();
            
            var localOptions = GetLocalOptionsByOrganizationUuid(organizationUuid)
                .ToDictionary(x => x.OptionId);

            return Result<IEnumerable<TOptionType>, OperationError>.Success(IncludeLocalChangesToGlobalOptions(globalOptions, localOptions));
        }

        private IEnumerable<TOptionType> IncludeLocalChangesToGlobalOptions(IEnumerable<TOptionType> globalOptions, IDictionary<int, TLocalOptionType> localOptions)
        { 
            return globalOptions.Select(optionToAdd =>
              {
                  var localOptionExists = localOptions.TryGetValue(optionToAdd.Id, out var localOption);
                  if (localOptionExists) optionToAdd.UpdateLocalOptionValues(localOption);
                  return optionToAdd;
              });
        }

        public Result<TOptionType, OperationError> GetByOrganizationUuidAndOptionId(Guid organizationUuid, int optionId)
        {
            var option = GetGlobalOptionsAsQueryable().FirstOrDefault(x => x.Id == optionId);

            if (option == null)
                return new OperationError($"No option with Id: {optionId} found", OperationFailure.NotFound);
            
            var localOption = GetLocalOptionsByOrganizationUuid(organizationUuid)
                    .FirstOrDefault(x => x.OptionId == optionId);
            
            option.UpdateLocalOptionValues(localOption);

            return option;
        }

        public Result<TOptionType, OperationError> CreateLocalOption(Guid organizationUuid, LocalOptionCreateParameters parameters)
        {
            return ResolveOrganizationIdAndValidateCreate(organizationUuid)
                .Bind(organizationId =>
                {
                    return GetLocalOptionByOrganizationUuidAndOptionId(organizationUuid, parameters.OptionId)
                        .Match(existingLocalOption =>
                        {
                            existingLocalOption.Activate();
                            _localOptionRepository.Update(existingLocalOption);
                            _domainEvents.Raise(new EntityUpdatedEvent<TLocalOptionType>(existingLocalOption));
                            _localOptionRepository.Save();

                            return GetByOrganizationUuidAndOptionId(organizationUuid, existingLocalOption.OptionId);
                        }, () =>
                        {
                            var newLocalOption = new TLocalOptionType();
                            newLocalOption.SetupNewLocalOption(organizationId, parameters.OptionId);

                            _localOptionRepository.Insert(newLocalOption);
                            _domainEvents.Raise(new EntityCreatedEvent<TLocalOptionType>(newLocalOption));
                            _localOptionRepository.Save();

                            return GetByOrganizationUuidAndOptionId(organizationUuid, newLocalOption.OptionId);
                        });
                });
        }

        public Result<TOptionType, OperationError> PatchLocalOption(Guid organizationUuid, int optionId, LocalOptionUpdateParameters parameters)
        {
            return GetLocalOptionByOrganizationUuidAndOptionId(organizationUuid, optionId)
                .Match(localOption =>
                {
                    if (!_authorizationContext.AllowModify(localOption))
                    {
                        return new OperationError($"User not allowed to modify option with id: {optionId}",
                            OperationFailure.Forbidden);
                    }

                    localOption.UpdateLocalOption(parameters.Description);
                    _domainEvents.Raise(new EntityCreatedEvent<TLocalOptionType>(localOption));
                    _localOptionRepository.Update(localOption);
                    _localOptionRepository.Save();
                    return GetByOrganizationUuidAndOptionId(organizationUuid, optionId);
                }, () =>
                {
                    var orgIdResult = ResolveOrganizationIdAndValidateCreate(organizationUuid);
                    if (orgIdResult.Failed)
                        return orgIdResult.Error;
                    var orgId = orgIdResult.Value;

                    var newLocalOption = new TLocalOptionType();

                    newLocalOption.SetupNewLocalOption(orgId, optionId);
                    newLocalOption.UpdateLocalOption(parameters.Description);

                    _domainEvents.Raise(new EntityUpdatedEvent<TLocalOptionType>(newLocalOption));
                    _localOptionRepository.Insert(newLocalOption);
                    _localOptionRepository.Save();
                    return GetByOrganizationUuidAndOptionId(organizationUuid, optionId);
                });
        }

        public Result<TOptionType, OperationError> DeleteLocalOption(Guid organizationUuid, int optionId)
        {
            return GetLocalOptionByOrganizationUuidAndOptionId(organizationUuid, optionId)
                .Match(localOption => _authorizationContext.AllowDelete(localOption)
                    ? Result<TLocalOptionType, OperationError>.Success(localOption)
                    : new OperationError($"User not allowed to delete local option with optionId: {optionId}",
                        OperationFailure.Forbidden),
                    () => new OperationError($"Local option in organization with uuid: {organizationUuid} with option id: {optionId} was not found", OperationFailure.NotFound))
                .Bind(localOption =>
                {
                    localOption.Deactivate();
                    _localOptionRepository.Update(localOption);
                    _domainEvents.Raise(new EntityUpdatedEvent<TLocalOptionType>(localOption));

                    _localOptionRepository.Save();
                    return GetByOrganizationUuidAndOptionId(organizationUuid, optionId);
                });
        }

        private Result<int, OperationError> ResolveOrganizationId(Guid organizationUuid)
        {
            return _identityResolver.ResolveDbId<Organization>(organizationUuid)
                .Match<Result<int, OperationError>>(id => id,
                    () => new OperationError($"Organization with uuid: {organizationUuid} not found",
                        OperationFailure.NotFound));
        }

        private Result<int, OperationError> ResolveOrganizationIdAndValidateCreate(Guid organizationUuid)
        {
            return ResolveOrganizationId(organizationUuid)
                .Bind(id => _authorizationContext.AllowCreate<TLocalOptionType>(id)
                    ? Result<int, OperationError>.Success(id)
                    : new OperationError(
                        $"User not allowed to create Local options for organization with uuid: {organizationUuid}",
                        OperationFailure.Forbidden));
        }

        private Maybe<TLocalOptionType> GetLocalOptionByOrganizationUuidAndOptionId(Guid organizationUuid, int optionId)
        {
            var localOption = GetLocalOptionsByOrganizationUuid(organizationUuid)
                .FirstOrDefault(x => x.OptionId == optionId);
            return localOption ?? Maybe<TLocalOptionType>.None;
        }

        private IEnumerable<TLocalOptionType> GetLocalOptionsByOrganizationUuid(Guid organizationUuid)
        {
            return _localOptionRepository
                .AsQueryable()
                .ByOrganizationUuid(organizationUuid);
        }

        private IEnumerable<TOptionType> GetGlobalOptionsAsQueryable()
        {
            return _optionsRepository
                .AsQueryable();
        }
    }
}
