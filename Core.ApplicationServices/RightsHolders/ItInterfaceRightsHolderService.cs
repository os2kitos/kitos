using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Interface;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.DataAccess;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Core.ApplicationServices.RightsHolders
{
    public class ItInterfaceRightsHolderService : BaseRightsHolderService, IItInterfaceRightsHolderService
    {
        private readonly IOrganizationalUserContext _userContext;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IItSystemService _systemService;
        private readonly IItInterfaceService _itInterfaceService;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger _logger;

        public ItInterfaceRightsHolderService(
            IOrganizationalUserContext userContext,
            IOrganizationRepository organizationRepository, 
            IItSystemService systemService,
            IItInterfaceService itInterfaceService,
            ITransactionManager transactionManager,
            ILogger logger) : base(userContext, organizationRepository)
        {
            _userContext = userContext;
            _organizationRepository = organizationRepository;
            _itInterfaceService = itInterfaceService;
            _transactionManager = transactionManager;
            _systemService = systemService;
            _logger = logger;
        }

        public Result<ItInterface, OperationError> CreateNewItInterface(Guid rightsHolderUuid, Guid exposingSystemUuid, RightsHolderItInterfaceCreationParameters creationParameters)
        {
            if (creationParameters == null)
                throw new ArgumentNullException(nameof(creationParameters));

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            try
            {
                var organizationId = _organizationRepository.GetByUuid(rightsHolderUuid).Select(x => x.Id);

                if (organizationId.IsNone)
                    return new OperationError("Invalid rights holder id provided", OperationFailure.BadInput);

                var exposingSystem = _systemService.GetSystem(exposingSystemUuid);

                if (exposingSystem.Failed)
                {
                    if(exposingSystem.Error.FailureType == OperationFailure.NotFound) //If we can't find the exposing system the call will never work and should return BadInput.
                        return new OperationError("Invalid exposing system id provided", OperationFailure.BadInput);
                    return exposingSystem.Error;
                }

                if (!_userContext.HasRole(organizationId.Value, OrganizationRole.RightsHolderAccess))
                    return new OperationError("User does not have rightsholder access in the provided organization", OperationFailure.Forbidden);

                var result = _itInterfaceService
                    .CreateNewItInterface(organizationId.Value, creationParameters.Name, creationParameters.InterfaceId, creationParameters.RightsHolderProvidedUuid)
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
                    _logger.Error($"RightsHolder {rightsHolderUuid} failed to create It-Interface {creationParameters.Name} due to error: {result.Error}");
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed creating rightsholder It-Interface for rightsholder with id {rightsHolderUuid}");
                return new OperationError(OperationFailure.UnknownError);
            }
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
                            var organizationId = _organizationRepository.GetByUuid(rightsHolderUuid.Value).Select(x => x.Id);

                            if (organizationId.IsNone)
                                return new OperationError("Invalid organization id", OperationFailure.BadInput);

                            if (!_userContext.HasRole(organizationId.Value, OrganizationRole.RightsHolderAccess))
                            {
                                return new OperationError("User is not rightsholder in the provided organization", OperationFailure.Forbidden);
                            }
                            refinements.Add(new QueryByRightsHolder(rightsHolderUuid.Value));
                        }

                        return Result<IQueryable<ItInterface>, OperationError>.Success(_itInterfaceService.GetAvailableInterfaces(refinements.ToArray()));
                    }
                );
        }
    }
}
