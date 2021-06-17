using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Interface;
using Core.DomainServices.Queries.ItSystem;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;
using Serilog;

namespace Core.ApplicationServices.RightsHolders
{
    public class RightsHoldersService : IRightsHoldersService
    {
        private readonly IOrganizationalUserContext _userContext;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IItSystemService _systemService;
        private readonly IItInterfaceService _itInterfaceService;
        private readonly ITransactionManager _transactionManager; 
        private readonly ILogger _logger;

        public RightsHoldersService(
            IOrganizationalUserContext userContext,
            IGenericRepository<Organization> organizationRepository,
            IItInterfaceService itInterfaceService,
            IItSystemService systemService,
            ITransactionManager transactionManager, 
            ILogger logger)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
            _itInterfaceService = itInterfaceService;
            _systemService = systemService;
            _transactionManager = transactionManager;
            _logger = logger;
        }

        public Result<ItInterface, OperationError> GetInterfaceAsRightsHolder(Guid interfaceUuid)
        {
            return WithAnyRightsHoldersAccess()
                .Match
                (
                    error => error,
                    () => _itInterfaceService.GetInterface(interfaceUuid).Bind(WithRightsHolderAccessTo)
                );
        }

        public Result<IQueryable<ItInterface>, OperationError> GetInterfacesWhereAuthenticatedUserHasRightsHolderAccess(Guid? rightsHolderUuid = null)
        {
            return WithAnyRightsHoldersAccess()
                .Match
                (
                    error => error,
                    () =>
                    {
                        var organizationIdsWhereUserHasRightsHoldersAccess = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess);

                        var refinements = new List<IDomainQuery<ItInterface>>()
                        {
                            new QueryByRightsHolderIdsOrOwnOrganizationIds(organizationIdsWhereUserHasRightsHoldersAccess)
                        };

                        if (rightsHolderUuid.HasValue)
                        {
                            var org = _organizationRepository.AsQueryable().ByUuid(rightsHolderUuid.Value);
                            if (!_userContext.HasRole(org.Id, OrganizationRole.RightsHolderAccess))
                            {
                                return new OperationError("User is not rightsholder in the provided organization", OperationFailure.Forbidden);
                            }
                            refinements.Add(new QueryByRightsHolder(rightsHolderUuid.Value));
                        }

                        return Result<IQueryable<ItInterface>, OperationError>.Success(_itInterfaceService.GetAvailableInterfaces(refinements.ToArray()));
                    }
                );
        }

        public IQueryable<Organization> ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
        {
            var organizationIds = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess).ToList();
            return _organizationRepository.AsQueryable().ByIds(organizationIds);
        }

        public Result<IQueryable<ItSystem>, OperationError> GetSystemsWhereAuthenticatedUserHasRightsHolderAccess(Guid? rightsHolderUuid = null)
        {
            return WithAnyRightsHoldersAccess()
                .Match
                (
                    error => error,
                    () =>
                    {
                        var organizationIdsWhereUserHasRightsHoldersAccess = _userContext.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess);
                        var refinements = new List<IDomainQuery<ItSystem>>
                        {
                            new QueryByRightsHolderIdOrOwnOrganizationIds(organizationIdsWhereUserHasRightsHoldersAccess)
                        };

                        if (rightsHolderUuid.HasValue)
                        {
                            var org = _organizationRepository.AsQueryable().ByUuid(rightsHolderUuid.Value);
                            if (!_userContext.HasRole(org.Id, OrganizationRole.RightsHolderAccess))
                            {
                                return new OperationError("User is not rightsholder in the provided organization", OperationFailure.Forbidden);
                            }
                            refinements.Add(new QueryByRightsHolderUuid(rightsHolderUuid.Value));
                        }

                        var systems = _systemService.GetAvailableSystems(refinements.ToArray());

                        return Result<IQueryable<ItSystem>, OperationError>.Success(systems);
                    }
                );
        }

        public Result<ItSystem, OperationError> GetSystemAsRightsHolder(Guid systemUuid)
        {
            return WithAnyRightsHoldersAccess()
                .Match
                (
                    error => error,
                    () => _systemService.GetSystem(systemUuid).Bind(WithRightsHolderAccessTo)
                );
        }

        private Maybe<OperationError> WithAnyRightsHoldersAccess()
        {
            return _userContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess)
                ? Maybe<OperationError>.None
                : new OperationError("User does not have 'rightsholders access' in any organization", OperationFailure.Forbidden);
        }

        private Result<T, OperationError> WithRightsHolderAccessTo<T>(T subject) where T : IHasRightsHolder
        {
            //User may have read access in a different context (own systems but not with rightsholder set to a rightsholding organization) but in this case we insist that rightsholder access must be issued
            var hasAssignedRightsHolderAccess = subject
                .GetRightsHolderOrganizationId()
                .Select(organizationId => _userContext.HasRole(organizationId, OrganizationRole.RightsHolderAccess))
                .GetValueOrFallback(false);

            if (hasAssignedRightsHolderAccess)
                return subject;

            return new OperationError("Not rightsholder for the requested system", OperationFailure.Forbidden);
        }

        public Result<ItInterface, OperationError> CreateNewItInterface(Guid rightsHolderUuid, Guid exposingSystemUuid, RightsHolderItInterfaceCreationParameters creationParameters)
        {
            if (creationParameters == null)
                throw new ArgumentNullException(nameof(creationParameters));

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            try
            {
                var organization = _organizationRepository.AsQueryable().ByUuid(rightsHolderUuid);

                if (organization == null)
                    return new OperationError("Invalid rights holder id provided", OperationFailure.BadInput);

                var exposingSystem = _systemService.GetSystem(exposingSystemUuid);

                if (exposingSystem.Failed)
                {
                    return exposingSystem.Error;
                }

                if (!_userContext.HasRole(organization.Id, OrganizationRole.RightsHolderAccess))
                    return new OperationError("User does not have rightsholder access in the provided organization", OperationFailure.Forbidden);

                var result = _itInterfaceService
                    .CreateNewItInterface(organization.Id, creationParameters.Name, creationParameters.InterfaceId, creationParameters.RightsHolderProvidedUuid)
                    .Bind(itInterface => _itInterfaceService.UpdateExposingSystem(itInterface.Id, exposingSystem.Value.Id))
                    .Bind(itInterface => _itInterfaceService.UpdateVersion(itInterface.Id, creationParameters.Version))
                    .Bind(itInterface => _itInterfaceService.UpdateDescription(itInterface.Id, creationParameters.Description))
                    .Bind(itInterface => _itInterfaceService.UpdateUrlReference(itInterface.Id, creationParameters.UrlReference));

                if (result.Ok) 
                {
                    transaction.Commit();
                }
                else
                {
                    _logger.Error($"RightsHolder {rightsHolderUuid} failed to create It-System {creationParameters.Name} due to error: {result.Error}");
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed creating rightsholder system for rightsholder with id {rightsHolderUuid}");
                return new OperationError(OperationFailure.UnknownError);
            }
        }
    }
}
