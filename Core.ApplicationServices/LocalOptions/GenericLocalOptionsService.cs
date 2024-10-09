using System;
using Core.DomainModel;
using Core.DomainServices;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
using Core.ApplicationServices.Model.LocalOptions;

namespace Core.ApplicationServices.LocalOptions.Base
{
    public class GenericLocalOptionsService<TLocalModelType, TDomainModelType, TOptionType> : IGenericLocalOptionsService<TLocalModelType, TDomainModelType, TOptionType>
    where TLocalModelType : LocalOptionEntity<TOptionType>, new ()
    where TOptionType : OptionEntity<TDomainModelType>
    {
        private readonly IGenericRepository<TOptionType> _optionsRepository;
        private readonly IGenericRepository<TLocalModelType> _localOptionRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IDomainEvents _domainEvents;

        public GenericLocalOptionsService(IGenericRepository<TOptionType> optionsRepository,
            IGenericRepository<TLocalModelType> localOptionRepository,
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

        public Result<TOptionType, OperationError> GetByOrganizationAndRoleUuid(Guid organizationUuid, int roleId)
        {
            var option = GetOptionsAsQueryable().FirstOrDefault(x => x.Id == roleId);

            if (option == null)
                return new OperationError($"No option with Uuid: {roleId} found", OperationFailure.NotFound);
            
            var localOption =GetLocalOptionsByOrganizationUuid(organizationUuid)
                    .FirstOrDefault(x => x.OptionId == roleId);
            
            option.UpdateLocalOptionValues(localOption);

            return option;
        }

        public Result<TLocalModelType, OperationError> CreateLocalOption(Guid organizationUuid, LocalOptionCreateParameters parameters)
        {
            return ResolveOrganizationId(organizationUuid)
                .Bind(organizationId =>
                {
                    var canCreate = _authorizationContext.AllowCreate<TLocalModelType>(organizationId);
                    return canCreate
                        ? Result<int, OperationError>.Success(organizationId)
                        : new OperationError($"User not allowed to create Local Options for organization with uuid: {organizationUuid}", OperationFailure.Forbidden);
                })
                .Bind(organizationId =>
                {
                    return GetLocalOptionByOrganizationUuidAndOptionId(organizationUuid, parameters.OptionId)
                        .Match(existingLocalOption =>
                        {
                            existingLocalOption.Activate();
                            _domainEvents.Raise(new EntityUpdatedEvent<TLocalModelType>(existingLocalOption));
                            _localOptionRepository.Save();

                            return Result<TLocalModelType, OperationError>.Success(existingLocalOption);
                        }, () =>
                        {
                            var entity = new TLocalModelType();
                            entity.SetupNewLocalOption(organizationId, parameters.OptionId);

                            _localOptionRepository.Insert(entity);
                            _domainEvents.Raise(new EntityCreatedEvent<TLocalModelType>(entity));
                            _localOptionRepository.Save();

                            return Result<TLocalModelType, OperationError>.Success(entity);
                        });
                });
        }

        public Result<TLocalModelType, OperationError> PatchLocalOption(Guid organizationUuid, TLocalModelType entity)
        {
            return ResolveOrganizationId(organizationUuid)
                .Bind(organizationId =>
                {
                    var canCreate = _authorizationContext.AllowCreate<TLocalModelType>(organizationId);
                    return canCreate
                        ? Result<int, OperationError>.Success(organizationId)
                        : new OperationError($"User not allowed to create Local Options for organization with uuid: {organizationUuid}", OperationFailure.Forbidden);
                })
                .Bind(organizationId =>
                {
                    return GetLocalOptionByOrganizationUuidAndOptionId(organizationUuid, entity.OptionId)
                        .Match(existingLocalOption =>
                        {
                            existingLocalOption.Activate();
                            _domainEvents.Raise(new EntityUpdatedEvent<TLocalModelType>(existingLocalOption));
                            _localOptionRepository.Save();

                            return Result<TLocalModelType, OperationError>.Success(existingLocalOption);
                        }, () =>
                        {
                            //entity.SetupNewLocalOption(organizationId);
                            entity.Activate();

                            _localOptionRepository.Insert(entity);
                            _domainEvents.Raise(new EntityCreatedEvent<TLocalModelType>(entity));
                            _localOptionRepository.Save();

                            return Result<TLocalModelType, OperationError>.Success(entity);
                        });
                });
        }

        private Result<int, OperationError> ResolveOrganizationId(Guid organizationUuid)
        {
            return _identityResolver.ResolveDbId<Organization>(organizationUuid)
                .Match<Result<int, OperationError>>(id => id,
                    () => new OperationError($"Organization with uuid: {organizationUuid} not found",
                        OperationFailure.NotFound));
        }

        private Maybe<TLocalModelType> GetLocalOptionByOrganizationUuidAndOptionId(Guid organizationUuid, int optionId)
        {
            var localOption = GetLocalOptionsByOrganizationUuid(organizationUuid)
                .FirstOrDefault(x => x.OptionId == optionId);
            return localOption ?? Maybe<TLocalModelType>.None;
        }

        private IEnumerable<TLocalModelType> GetLocalOptionsByOrganizationUuid(Guid organizationUuid)
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
