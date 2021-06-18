using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.System;
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
using Core.DomainServices.Repositories.TaskRefs;
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
        private readonly ITaskRefRepository _taskRefRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;
        private readonly IItInterfaceService _itInterfaceService;

        public RightsHoldersService(
            IOrganizationalUserContext userContext,
            IGenericRepository<Organization> organizationRepository,
            IItInterfaceService itInterfaceService,
            IItSystemService systemService,
            ITaskRefRepository taskRefRepository,
            ITransactionManager transactionManager,
            ILogger logger)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
            _itInterfaceService = itInterfaceService;
            _systemService = systemService;
            _taskRefRepository = taskRefRepository;
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

        public Result<ItSystem, OperationError> CreateNewSystem(Guid rightsHolderUuid, RightsHolderSystemCreationParameters creationParameters)
        {
            if (creationParameters == null)
                throw new ArgumentNullException(nameof(creationParameters));

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            try
            {
                var organization = _organizationRepository.AsQueryable().ByUuid(rightsHolderUuid);

                if (organization == null)
                    return new OperationError("Invalid rights holder id provided", OperationFailure.BadInput);

                if (!_userContext.HasRole(organization.Id, OrganizationRole.RightsHolderAccess))
                    return new OperationError("User does not have rightsholder access in the provided organization", OperationFailure.Forbidden);

                var result = _systemService
                    .CreateNewSystem(organization.Id, creationParameters.Name, creationParameters.RightsHolderProvidedUuid)
                    .Bind(system => _systemService.UpdateRightsHolder(system.Id, rightsHolderUuid))
                    .Bind(system => _systemService.UpdatePreviousName(system.Id, creationParameters.FormerName))
                    .Bind(system => _systemService.UpdateDescription(system.Id, creationParameters.Description))
                    .Bind(system => UpdateMainUrlReference(system.Id, creationParameters.UrlReference))
                    .Bind(system => UpdateParentSystem(system.Id, creationParameters.ParentSystemUuid))
                    .Bind(system => _systemService.UpdateBusinessType(system.Id, creationParameters.BusinessTypeUuid))
                    .Bind(system => UpdateTaskRefs(system.Id, creationParameters.TaskRefKeys, creationParameters.TaskRefUuids));

                if (result.Ok)
                {
                    transaction.Commit();
                }
                else
                {
                    _logger.Error("RightsHolder {uuid} failed to create It-System {name} due to error: {errorMessage}", rightsHolderUuid, creationParameters.Name, result.Error.ToString());
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed creating rightsholder system for rightsholder with id {rightsHolderUuid}", rightsHolderUuid);
                return new OperationError(OperationFailure.UnknownError);
            }
        }

        private Result<ItSystem, OperationError> UpdateTaskRefs(int systemId, IEnumerable<string> taskRefKeys, IEnumerable<Guid> taskRefUuids)
        {
            var taskRefIds = new HashSet<int>();
            foreach (var taskRefKey in taskRefKeys)
            {
                var taskRef = _taskRefRepository.GetTaskRef(taskRefKey);

                if (taskRef.IsNone)
                    return new OperationError($"Invalid KLE Number:{taskRefKey}", OperationFailure.BadInput);

                taskRefIds.Add(taskRef.Value.Id);
            }
            foreach (var uuid in taskRefUuids)
            {
                var taskRef = _taskRefRepository.GetTaskRef(uuid);

                if (taskRef.IsNone)
                    return new OperationError($"Invalid KLE UUID:{uuid}", OperationFailure.BadInput);

                taskRefIds.Add(taskRef.Value.Id);
            }

            return _systemService.UpdateTaskRefs(systemId, taskRefIds.ToList());
        }

        private Result<ItSystem, OperationError> UpdateMainUrlReference(int systemId, string urlReference)
        {
            if (string.IsNullOrWhiteSpace(urlReference))
                return new OperationError("URL references are required for new rightsholder systems", OperationFailure.BadInput);

            return _systemService.UpdateMainUrlReference(systemId, urlReference);
        }

        private Result<ItSystem, OperationError> UpdateParentSystem(int systemId, Guid? parentSystemUuid)
        {
            var parentSystemId = default(int?);
            if (parentSystemUuid.HasValue)
            {
                //Make sure that user has rightsholders access to the parent system
                var parentSystemResult =
                    _systemService
                        .GetSystem(parentSystemUuid.Value)
                        .Bind(WithRightsHolderAccessTo);

                if (parentSystemResult.Failed)
                    return parentSystemResult.Error;


                parentSystemId = parentSystemResult.Value.Id;
            }

            return _systemService.UpdateParentSystem(systemId, parentSystemId);

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
    }
}
