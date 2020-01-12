using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.Organization;

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

        public override IHttpActionResult Post(User entity)
        {
            return StatusCode(HttpStatusCode.MethodNotAllowed);
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
                // TODO check if user is allowed to add users to this organization
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
                if (!UserContext.HasRole(OrganizationRole.GlobalAdmin))
                {
                    ModelState.AddModelError(nameof(user.IsGlobalAdmin), "You don't have permission to create a global admin user.");
                }
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            user.ObjectOwnerId = UserId;
            user.LastChangedByUserId = UserId;

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
        /// Always returns 401 - Unauthorized. Please use /api/User/{id} from API - UserController instead.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override IHttpActionResult Delete(int key)
        {
            return Unauthorized();
        }

        private bool EmailExists(string email)
        {
            var matchingEmails = Repository.Get(x => x.Email == email);
            return matchingEmails.Any();
        }
    }
}
