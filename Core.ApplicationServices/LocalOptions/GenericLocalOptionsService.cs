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
            return GetGlobalOptionById(optionId)
                .Bind(option =>
                {
                    var localOption = GetLocalOptionsByOrganizationUuid(organizationUuid)
                        .FirstOrDefault(x => x.OptionId == optionId);

                    option.UpdateLocalOptionValues(localOption);

                    return Result<TOptionType, OperationError>.Success(option);
                });
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
                .Match(localOption => ValidateModify(localOption)
                    .Bind(
                        localOptionWithModifyRights =>
                        {
                            if (parameters.Description.HasChange) localOptionWithModifyRights.UpdateDescription(parameters.Description.NewValue);

                            _domainEvents.Raise(new EntityCreatedEvent<TLocalOptionType>(localOptionWithModifyRights));
                            _localOptionRepository.Update(localOptionWithModifyRights);
                            _localOptionRepository.Save();
                            return GetByOrganizationUuidAndOptionId(organizationUuid, optionId);
                        }),
                    () =>
                    ResolveOrganizationIdAndValidateCreate(organizationUuid)
                        .Bind(orgId =>
                        {
                            var newLocalOption = new TLocalOptionType();
                            newLocalOption.SetupNewLocalOption(orgId, optionId);
                            if (parameters.Description.HasChange)
                                newLocalOption.UpdateDescription(parameters.Description.NewValue);

                            _domainEvents.Raise(new EntityUpdatedEvent<TLocalOptionType>(newLocalOption));
                            _localOptionRepository.Insert(newLocalOption);
                            _localOptionRepository.Save();
                            return GetByOrganizationUuidAndOptionId(organizationUuid, optionId);
                        })
                );
        }

        private Result<TLocalOptionType, OperationError> ValidateModify(TLocalOptionType localOption)
        {
            return _authorizationContext.AllowModify(localOption)
                ? localOption
                : new OperationError($"User not allowed to modify local option with id: {localOption.Id}", OperationFailure.Forbidden);
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

        private Result<TOptionType, OperationError> GetGlobalOptionById(int id)
        {
            var option = GetGlobalOptionsAsQueryable().FirstOrDefault(x => x.Id == id);
            return option != null
                ? option
                : new OperationError($"Global option with id {id} not found", OperationFailure.NotFound);
        }
    }
}
