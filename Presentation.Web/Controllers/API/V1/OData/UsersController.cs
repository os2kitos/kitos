using System;
using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using System.Linq;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization.Permissions;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Core.DomainServices.Generic;
using Core.DomainServices.Authorization;
using Core.DomainModel.Organization;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class UsersController : BaseEntityController<User>
    {
        private readonly IUserService _userService;
        private readonly IGenericRepository<User> _repository;
        private readonly IEntityIdentityResolver _entityIdentityResolver;

        public UsersController(
            IGenericRepository<User> repository,
            IUserService userService, IEntityIdentityResolver entityIdentityResolver)
            : base(repository)
        {
            _userService = userService;
            _entityIdentityResolver = entityIdentityResolver;
            _repository = repository;
        }

        [NonAction]
        public override IHttpActionResult Post(int organizationId, User entity) => throw new NotSupportedException();

        protected override Maybe<IHttpActionResult> ValidatePatch(Delta<User> delta, User entity)
        {
            var error = base.ValidatePatch(delta, entity);
            if (error.IsNone)
            {
                var changedPropertyNames = delta.GetChangedPropertyNames().ToHashSet();
                if (AttemptToChangeStakeHolderAccess(delta, entity, changedPropertyNames))
                {
                    if (!AuthorizationContext.HasPermission(new AdministerGlobalPermission(GlobalPermission.StakeHolderAccess)))
                    {
                        error = Forbidden();
                    }
                }

                if (AttemptToChangeGlobalAdminFlag(delta, entity, changedPropertyNames))
                {
                    if (!AuthorizationContext.HasPermission(new AdministerGlobalPermission(GlobalPermission.GlobalAdmin)))
                    {
                        error = Forbidden();
                    }
                }

                if (AttemptToChangeUuid(delta, entity, changedPropertyNames))
                {
                    return BadRequest("Uuid cannot be changed");
                }
            }

            return error;
        }

        private static bool AttemptToChangeUuid(Delta<User> delta, User entity, HashSet<string> changedPropertyNames)
        {
            const string uuidName = nameof(Core.DomainModel.User.Uuid);

            return changedPropertyNames.Contains(uuidName) && delta.TryGetPropertyValue(uuidName,
                out var uuid) && ((Guid)uuid) != entity.Uuid;
        }

        private static bool AttemptToChangeGlobalAdminFlag(Delta<User> delta, User entity, ICollection<string> changedPropertyNames)
        {
            const string isGlobalAdminName = nameof(Core.DomainModel.User.IsGlobalAdmin);

            return changedPropertyNames.Contains(isGlobalAdminName) && delta.TryGetPropertyValue(isGlobalAdminName,
                out var globalAdmin) && ((bool)globalAdmin) != entity.IsGlobalAdmin;
        }

        private static bool AttemptToChangeStakeHolderAccess(Delta<User> delta, User entity, ICollection<string> changedPropertyNames)
        {
            const string stakeHolderAccess = nameof(Core.DomainModel.User.HasStakeHolderAccess);

            return changedPropertyNames.Contains(stakeHolderAccess) && delta.TryGetPropertyValue(nameof(stakeHolderAccess),
                out var hasStakeHolderAccess) && ((bool)hasStakeHolderAccess) != entity.HasStakeHolderAccess;
        }

        [HttpPost]
        public IHttpActionResult Create(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = null;
            if (parameters.ContainsKey("user"))
            {
                user = parameters["user"] as User;
                Validate(user); // this will set the ModelState if not valid - it doesn't http://stackoverflow.com/questions/39484185/model-validation-in-odatacontroller
            }

            var organizationId = 0;
            if (parameters.ContainsKey("organizationId"))
            {
                organizationId = (int)parameters["organizationId"];
            }

            var sendMailOnCreation = false;
            if (parameters.ContainsKey("sendMailOnCreation"))
            {
                sendMailOnCreation = (bool)parameters["sendMailOnCreation"];
            }

            if (user?.Email != null && EmailExists(user.Email))
            {
                ModelState.AddModelError(nameof(user.Email), "Email is already in use.");
            }

            // user is being created as global admin
            if (user?.IsGlobalAdmin == true)
            {
                // only other global admins can create global admin users
                if (!AuthorizationContext.HasPermission(new AdministerGlobalPermission(GlobalPermission.GlobalAdmin)))
                {
                    ModelState.AddModelError(nameof(user.IsGlobalAdmin), "You don't have permission to create a global admin user.");
                }
            }

            if (user?.HasStakeHolderAccess == true)
            {
                // only global admins can create stakeholder access
                if (!AuthorizationContext.HasPermission(new AdministerGlobalPermission(GlobalPermission.StakeHolderAccess)))
                {
                    ModelState.AddModelError(nameof(user.HasStakeHolderAccess), "You don't have permission to issue stakeholder access.");
                }
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdUser = _userService.AddUser(user, sendMailOnCreation, organizationId);

            return Created(createdUser);
        }
        [HttpGet]
        public IHttpActionResult IsEmailAvailable(string email)
        {
            var available = EmailExists(email) == false;

            return Ok(available);
        }

        [ODataRoute("GetUserByEmail(email={email})")]
        public IHttpActionResult GetUserByEmail(string email)
        {
            var userToReturn = this._repository.AsQueryable().FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (userToReturn != null)
            {
                return Ok(userToReturn);
            }
            return NotFound();
        }

        [ODataRoute("GetUserByEmailAndOrganizationRelationship(email={email},organizationId={organizationId})")]
        public IHttpActionResult GetUserByEmailAndOrganizationId(string email, int organizationId)
        {
            var userToReturn = this._repository.AsQueryable().FirstOrDefault(u =>
                u.Email.ToLower() == email.ToLower() &&
                u.OrganizationRights.Any(r => r.OrganizationId == organizationId));

            if (userToReturn != null)
            {
                return Ok(userToReturn);
            }
            return NotFound();
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("GetUsersByUuid(organizationUuid={organizationUuid})")]
        public IHttpActionResult GetUsersByUuid(Guid organizationUuid)
        {
            var idResult = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            if (idResult.IsNone)
                return NotFound();

            var id = idResult.Value;
            var accessLevel = GetOrganizationReadAccessLevel(id);
            if (accessLevel < OrganizationDataReadAccessLevel.Public)
            {
                return Forbidden();
            }

            var result = _repository.AsQueryable().Where(m => m.OrganizationRights.Any(r => r.OrganizationId == id));
            return Ok(result);
        }


        /// <summary>
        /// Always returns 405 - Unauthorized. Please use /api/User/{id} from API - UserController instead.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();

        private bool EmailExists(string email)
        {
            var matchingEmails = Repository.Get(x => x.Email == email);
            return matchingEmails.Any();
        }
    }
}
