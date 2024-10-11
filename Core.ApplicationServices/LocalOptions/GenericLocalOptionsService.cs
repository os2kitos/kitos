﻿using System;
using Core.DomainModel;
using Core.DomainServices;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
using Core.ApplicationServices.Model.LocalOptions;

namespace Core.ApplicationServices.LocalOptions.Base
{
    public class GenericLocalOptionsService<TLocalOptionType, TDomainModelType, TOptionType> : IGenericLocalOptionsService<TLocalOptionType, TDomainModelType, TOptionType>
    where TLocalOptionType : LocalOptionEntity<TOptionType>, new ()
    where TOptionType : OptionEntity<TDomainModelType>
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
            var localOptionsResult = GetLocalOptionsByOrganizationUuid(organizationUuid)
                .ToDictionary(x => x.OptionId);

            var globalOptionsResult = GetOptionsAsQueryable()
                .Where(x => x.IsEnabled)
                .ToList();

            var returnList = new List<TOptionType>();

            foreach (var item in globalOptionsResult)
            {
                var itemToAdd = item;

                itemToAdd.UpdateLocalOptionValues(localOptionsResult[itemToAdd.Id]);
                
                returnList.Add(itemToAdd);
            }
            return returnList;
        }

        public Result<TOptionType, OperationError> GetByOrganizationUuidAndRoleId(Guid organizationUuid, int roleId)
        {
            var option = GetOptionsAsQueryable().FirstOrDefault(x => x.Id == roleId);

            if (option == null)
                return new OperationError($"No option with Uuid: {roleId} found", OperationFailure.NotFound);
            
            var localOption =GetLocalOptionsByOrganizationUuid(organizationUuid)
                    .FirstOrDefault(x => x.OptionId == roleId);
            
            option.UpdateLocalOptionValues(localOption);

            return option;
        }

        public Result<TLocalOptionType, OperationError> CreateLocalOption(Guid organizationUuid, LocalOptionCreateParameters parameters)
        {
            return ResolveOrganizationIdAndValidateCreate(organizationUuid)
                .Bind(organizationId =>
                {
                    return GetLocalOptionByOrganizationUuidAndOptionId(organizationUuid, parameters.OptionId)
                        .Match(existingLocalOption =>
                        {
                            existingLocalOption.Activate();
                            _domainEvents.Raise(new EntityUpdatedEvent<TLocalOptionType>(existingLocalOption));
                            _localOptionRepository.Save();

                            return Result<TLocalOptionType, OperationError>.Success(existingLocalOption);
                        }, () =>
                        {
                            var entity = new TLocalOptionType();
                            entity.SetupNewLocalOption(organizationId, parameters.OptionId);

                            _localOptionRepository.Insert(entity);
                            _domainEvents.Raise(new EntityCreatedEvent<TLocalOptionType>(entity));
                            _localOptionRepository.Save();

                            return Result<TLocalOptionType, OperationError>.Success(entity);
                        });
                });
        }

        public Result<TLocalOptionType, OperationError> PatchLocalOption(Guid organizationUuid, int optionId, LocalOptionUpdateParameters parameters)
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
                    _localOptionRepository.Update(localOption);
                    _localOptionRepository.Save();
                    return Result<TLocalOptionType, OperationError>.Success(localOption);
                }, () =>
                {
                    var orgIdResult = ResolveOrganizationIdAndValidateCreate(organizationUuid);
                    if (orgIdResult.Failed)
                        return orgIdResult.Error;
                    var orgId = orgIdResult.Value;

                    var entity = new TLocalOptionType();

                    entity.SetupNewLocalOption(orgId, optionId);
                    entity.UpdateLocalOption(parameters.Description);

                    _domainEvents.Raise(new EntityUpdatedEvent<TLocalOptionType>(entity));
                    _localOptionRepository.Insert(entity);
                    _localOptionRepository.Save();
                    return Result<TLocalOptionType, OperationError>.Success(entity);
                });
        }

        public Result<TLocalOptionType, OperationError> DeleteLocalOption(Guid organizationUuid, int optionId)
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
                    _domainEvents.Raise(new EntityUpdatedEvent<TLocalOptionType>(localOption));

                    _localOptionRepository.Save();
                    return Result<TLocalOptionType, OperationError>.Success(localOption);
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

        private IEnumerable<TOptionType> GetOptionsAsQueryable()
        {
            return _optionsRepository
                .AsQueryable();
        }
    }
}
