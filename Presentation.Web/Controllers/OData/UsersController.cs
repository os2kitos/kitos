using System;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using System.Linq;
using System.Web.Http;
using Core.ApplicationServices.Authorization.Permissions;
using Infrastructure.Services.Types;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    public class UsersController : BaseEntityController<User>
    {
        private readonly IUserService _userService;
        private readonly IGenericRepository<User> _repository;

        public UsersController(
            IGenericRepository<User> repository,
            IUserService userService)
            : base(repository)
        {
            _userService = userService;
            _repository = repository;
        }

        [NonAction]
        public override IHttpActionResult Post(int organizationId, User entity) => throw new NotSupportedException();

        protected override Maybe<IHttpActionResult> ValidatePatch(Delta<User> delta, User entity)
        {
            var error = base.ValidatePatch(delta, entity);
            if (error.IsNone)
            {
                if (AttemptToChangeStakeHolderAccess(delta, entity))
                {
                    if (!AuthorizationContext.HasPermission(new AdministerGlobalPermission(GlobalPermission.StakeHolderAccess)))
                    {
                        error = Forbidden();
                    }
                }

                if (AttemptToChangeGlobalAdminFlag(delta, entity))
                {
                    if (!AuthorizationContext.HasPermission(new AdministerGlobalPermission(GlobalPermission.GlobalAdmin)))
                    {
                        error = Forbidden();
                    }
                }

                if (AttemptToChangeUuid(delta))
                {
                    return BadRequest("Uuid cannot be changed");
                }
            }

            return error;
        }

        private bool AttemptToChangeUuid(Delta<User> delta)
        {
            return delta.TryGetPropertyValue(nameof(Core.DomainModel.User.Uuid),out _);
        }

        private static bool AttemptToChangeGlobalAdminFlag(Delta<User> delta, User entity)
        {
            return delta.TryGetPropertyValue(nameof(Core.DomainModel.User.IsGlobalAdmin),
                out var globalAdmin) && ((bool)globalAdmin) != entity.IsGlobalAdmin;
        }

        private static bool AttemptToChangeStakeHolderAccess(Delta<User> delta, User entity)
        {
            return delta.TryGetPropertyValue(nameof(Core.DomainModel.User.HasStakeHolderAccess),
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
