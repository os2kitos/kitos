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


        public IEnumerable<TOptionType> GetLocalOptions(Guid organizationUuid)
        {
            var globalOptions = GetEnabledGlobalOptionsAsQueryable()
                .ToList();
            
           var localOptions = GetLocalOptionsAsQueryable(organizationUuid)
                .ToDictionary(x => x.OptionId);

            return IncludeLocalChangesToGlobalOptions(globalOptions, localOptions);
        }

        private IEnumerable<TOptionType> IncludeLocalChangesToGlobalOptions(IEnumerable<TOptionType> globalOptions, IDictionary<int, TLocalOptionType> localOptions)
        { 
            return globalOptions
                .Select(optionToAdd =>
              {
                  optionToAdd.ResetLocalOptionAvailability();
                  var localOptionExists = localOptions.TryGetValue(optionToAdd.Id, out var localOption);
                  if (localOptionExists) optionToAdd.UpdateLocalOptionValues(localOption);
                  return optionToAdd;
              });
        }

        public Result<TOptionType, OperationError> GetLocalOption(Guid organizationUuid, Guid globalOptionUuid)
        {
            return ResolveGlobalOptionId(globalOptionUuid)
                .Bind(GetGlobalOptionById)
                .Bind(option =>
                {
                    option.ResetLocalOptionAvailability();
                    var localOption = GetLocalOptionsAsQueryable(organizationUuid)
                        .FirstOrDefault(x => x.OptionId == option.Id);

                    option.UpdateLocalOptionValues(localOption);

                    return Result<TOptionType, OperationError>.Success(option);
                });
        }

        private Result<int, OperationError> ResolveGlobalOptionId(Guid uuid)
        {
            return _identityResolver.ResolveDbId<TOptionType>(uuid)
                .Match(Result<int, OperationError>.Success,
                    () => new OperationError($"Cannot resolve global option uuid {uuid} to database id", OperationFailure.NotFound));

        }

        public Result<TOptionType, OperationError> CreateLocalOption(Guid organizationUuid, LocalOptionCreateParameters parameters)
        {
            return ResolveOrganizationIdAndValidateCreate(organizationUuid)
                .Bind(organizationId =>
                {
                    return GetLocalOptionMaybe(organizationUuid, parameters.OptionUuid)
                        .Match(existingLocalOption =>
                        {
                            existingLocalOption.Activate();
                            _localOptionRepository.Update(existingLocalOption);
                            _domainEvents.Raise(new EntityUpdatedEvent<TLocalOptionType>(existingLocalOption));
                            _localOptionRepository.Save();

                            return GetLocalOption(organizationUuid, parameters.OptionUuid);
                        }, () =>
                        {
                            var globalOptionUuid = parameters.OptionUuid;
                            var globalOptionIdResult = ResolveGlobalOptionId(globalOptionUuid);
                            if (globalOptionIdResult.Failed) return globalOptionIdResult.Error;

                            var newLocalOption = new TLocalOptionType();
                            newLocalOption.SetupNewLocalOption(organizationId, globalOptionIdResult.Value);

                            _localOptionRepository.Insert(newLocalOption);
                            _domainEvents.Raise(new EntityCreatedEvent<TLocalOptionType>(newLocalOption));
                            _localOptionRepository.Save();

                            return GetLocalOption(organizationUuid, globalOptionUuid);
                        });
                });
        }

        public Result<TOptionType, OperationError> PatchLocalOption(Guid organizationUuid, Guid globalOptionUuid, LocalOptionUpdateParameters parameters) 
        {
            return GetLocalOptionMaybe(organizationUuid, globalOptionUuid)
                .Match(localOption => ValidateModify(localOption)
                    .Bind(
                        localOptionWithModifyRights =>
                        {
                            if (parameters.Description.HasChange) localOptionWithModifyRights.UpdateDescription(parameters.Description.NewValue);

                            _domainEvents.Raise(new EntityCreatedEvent<TLocalOptionType>(localOptionWithModifyRights));
                            _localOptionRepository.Update(localOptionWithModifyRights);
                            _localOptionRepository.Save();
                            return GetLocalOption(organizationUuid, globalOptionUuid);
                        }),
                    () =>
                    ResolveOrganizationIdAndValidateCreate(organizationUuid)
                        .Bind(orgId =>
                        {
                            var newLocalOption = new TLocalOptionType();
                            var optionIdResult = ResolveGlobalOptionId(globalOptionUuid);
                            if (optionIdResult.Failed) return optionIdResult.Error;

                            newLocalOption.SetupNewLocalOption(orgId, optionIdResult.Value);
                            if (parameters.Description.HasChange)
                                newLocalOption.UpdateDescription(parameters.Description.NewValue);

                            _domainEvents.Raise(new EntityUpdatedEvent<TLocalOptionType>(newLocalOption));
                            _localOptionRepository.Insert(newLocalOption);
                            _localOptionRepository.Save();
                            return GetLocalOption(organizationUuid, globalOptionUuid);
                        })
                );
        }

        private Result<TLocalOptionType, OperationError> ValidateModify(TLocalOptionType localOption)
        {
            return _authorizationContext.AllowModify(localOption)
                ? localOption
                : new OperationError($"User not allowed to modify local option with id: {localOption.Id}", OperationFailure.Forbidden);
        }

        public Result<TOptionType, OperationError> DeleteLocalOption(Guid organizationUuid, Guid globalOptionUuid)
        {
            return GetLocalOptionMaybe(organizationUuid, globalOptionUuid)
                .Match(Result<TLocalOptionType, OperationError>.Success,
                    () => CreateAndGetLocalOption(organizationUuid, globalOptionUuid))
                .Bind(localOption =>
                {
                    if (!_authorizationContext.AllowDelete(localOption))
                        return new OperationError(
                            $"User not allowed to delete local option {localOption.Id} with global option uuid: {globalOptionUuid}",
                            OperationFailure.Forbidden);

                    localOption.Deactivate();
                    _localOptionRepository.Update(localOption);
                    _domainEvents.Raise(new EntityUpdatedEvent<TLocalOptionType>(localOption));

                    _localOptionRepository.Save();
                    return GetLocalOption(organizationUuid, globalOptionUuid);
                });
        }

        private Result<TLocalOptionType, OperationError> CreateAndGetLocalOption(Guid organizationUuid,
            Guid globalOptionUuid)
        {
            return CreateLocalOption(organizationUuid,
                new LocalOptionCreateParameters() { OptionUuid = globalOptionUuid })
                .Bind(_ =>
                {
                    var localOptionMaybe = GetLocalOptionMaybe(organizationUuid, globalOptionUuid);
                    if (localOptionMaybe.IsNone)
                        return new OperationError(
                            $"Local option in organization with uuid: {organizationUuid} with global option uuid: {globalOptionUuid} was not found",
                            OperationFailure.NotFound);
                    return Result<TLocalOptionType, OperationError>.Success(localOptionMaybe.Value);
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



        private Maybe<TLocalOptionType> GetLocalOptionMaybe(Guid organizationUuid, Guid globalOptionUuid)
        {
            var optionIdResult = ResolveGlobalOptionId(globalOptionUuid);
            if (optionIdResult.Failed) return Maybe<TLocalOptionType>.None;

            var localOption = GetLocalOptionsAsQueryable(organizationUuid)
                .FirstOrDefault(x => x.OptionId == optionIdResult.Value);
            return localOption ?? Maybe<TLocalOptionType>.None;
        }

        private IEnumerable<TLocalOptionType> GetLocalOptionsAsQueryable(Guid organizationUuid)
        { 
            return _localOptionRepository
                .AsQueryable()
                .ByOrganizationUuid(organizationUuid);
        }

        private IEnumerable<TOptionType> GetEnabledGlobalOptionsAsQueryable()
        {
            return _optionsRepository
                .AsQueryable()
                .Where(x => x.IsEnabled);
        }

        private Result<TOptionType, OperationError> GetGlobalOptionById(int id)
        {
            var option = GetEnabledGlobalOptionsAsQueryable().FirstOrDefault(x => x.Id == id);
            return option != null
                ? option
                : new OperationError($"Global option with id {id} not found", OperationFailure.NotFound);
        }
    }
}
