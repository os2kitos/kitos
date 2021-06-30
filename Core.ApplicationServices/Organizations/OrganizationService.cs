using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.DataAccess;
using Serilog;

namespace Core.ApplicationServices.Organizations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IOrganizationRepository _repository;
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrganizationalUserContext _userContext;
        private readonly ILogger _logger;
        private readonly IOrganizationRoleService _organizationRoleService;
        private readonly ITransactionManager _transactionManager;

        public OrganizationService(
            IGenericRepository<Organization> orgRepository,
            IGenericRepository<OrganizationRight> orgRightRepository,
            IGenericRepository<User> userRepository,
            IAuthorizationContext authorizationContext,
            IOrganizationalUserContext userContext,
            ILogger logger,
            IOrganizationRoleService organizationRoleService,
            ITransactionManager transactionManager, 
            IOrganizationRepository repository)
        {
            _orgRepository = orgRepository;
            _orgRightRepository = orgRightRepository;
            _userRepository = userRepository;
            _authorizationContext = authorizationContext;
            _userContext = userContext;
            _logger = logger;
            _organizationRoleService = organizationRoleService;
            _transactionManager = transactionManager;
            _repository = repository;
        }

        /// <summary>
        /// lists the organizations the user is a member of
        /// </summary>
        /// <param name="user"></param>
        /// <returns>a list of organizations that the user is a member of</returns>
        public IEnumerable<Organization> GetOrganizations(User user)
        {
            if (user.IsGlobalAdmin) return _orgRepository.Get();
            return _orgRepository
                .Get(o => o.Rights.Any(r => r.OrganizationId == o.Id && r.UserId == user.Id));
        }

        //returns the default org unit for that user inside that organization
        //or null if none has been chosen
        public OrganizationUnit GetDefaultUnit(Organization organization, User user)
        {
            return _orgRightRepository
                .Get(r => r.OrganizationId == organization.Id && r.UserId == user.Id)
                .Select(r => r.DefaultOrgUnit)
                .FirstOrDefault();
        }

        public void SetDefaultOrgUnit(User user, int orgId, int orgUnitId)
        {
            var right = _orgRightRepository.Get(r => r.UserId == user.Id && r.OrganizationId == orgId).First();
            right.DefaultOrgUnitId = orgUnitId;

            _orgRightRepository.Update(right);
            _orgRightRepository.Save();
        }

        /// <summary>
        /// Remove all organization rights from a user.
        /// </summary>
        /// <param name="organizationId">The organization the user should be removed from.</param>
        /// <param name="userId">The user to be removed.</param>
        public Result<Organization, OperationFailure> RemoveUser(int organizationId, int userId)
        {
            var organization = _orgRepository.GetByKey(organizationId);
            if (organization == null)
            {
                return OperationFailure.NotFound;
            }

            if (!_authorizationContext.AllowModify(organization))
            {
                return OperationFailure.Forbidden;
            }

            var rights = _orgRightRepository
                .AsQueryable()
                .Where(x => x.UserId == userId && x.OrganizationId == organizationId)
                .AsEnumerable();

            foreach (var right in rights)
            {
                _orgRightRepository.DeleteByKey(right.Id);
            }
            _orgRightRepository.Save();

            return organization;
        }

        public bool CanChangeOrganizationType(Organization organization, OrganizationTypeKeys organizationType)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }
            return
                _authorizationContext.AllowModify(organization) &&
                _authorizationContext.HasPermission(new DefineOrganizationTypePermission(organizationType, organization.Id));
        }

        public Result<Organization, OperationFailure> CreateNewOrganization(Organization newOrg)
        {
            if (newOrg == null)
            {
                throw new ArgumentNullException(nameof(newOrg));
            }
            var user = _userRepository.GetByKey(_userContext.UserId);

            if (user == null)
            {
                return OperationFailure.Forbidden;
            }

            //Setup defaults
            newOrg.Uuid = newOrg.Uuid == Guid.Empty ? Guid.NewGuid() : newOrg.Uuid;
            newOrg.Config = Config.Default(user);
            newOrg.OrgUnits.Add(new OrganizationUnit
            {
                Name = newOrg.Name,
            });

            if (newOrg.IsCvrInvalid())
            {
                _logger.Error("Invalid cvr {cvr} provided for org with name {name}", newOrg.Cvr, newOrg.Name);
                return OperationFailure.BadInput;
            }

            if (!_userContext.OrganizationIds.Any(id => _authorizationContext.AllowCreate<Organization>(id)))
            {
                return OperationFailure.Forbidden;
            }

            if (newOrg.TypeId > 0)
            {
                var organizationType = (OrganizationTypeKeys)newOrg.TypeId;
                var allowOrganizationTypeCreation = _userContext.OrganizationIds.Any(id => _authorizationContext.HasPermission(new DefineOrganizationTypePermission(organizationType, id)));
                if (!allowOrganizationTypeCreation)
                {
                    return OperationFailure.Forbidden;
                }
            }
            else
            {
                //Invalid org key
                return OperationFailure.BadInput;
            }

            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                newOrg = _orgRepository.Insert(newOrg);
                _orgRepository.Save();

                if (newOrg.TypeId == (int)OrganizationTypeKeys.Interessefællesskab)
                {
                    _organizationRoleService.MakeLocalAdmin(user, newOrg);
                    _organizationRoleService.MakeUser(user, newOrg);
                }

                transaction.Commit();
                return newOrg;
            }
        }

        public Result<IQueryable<Organization>, OperationError> GetAllOrganizations()
        {
            if(_authorizationContext.GetCrossOrganizationReadAccess() != CrossOrganizationDataReadAccessLevel.All)
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            return Result<IQueryable<Organization>, OperationError>.Success(_repository.GetAll());
        }
    }
}
