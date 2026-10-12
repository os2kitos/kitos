using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Core.ApplicationServices;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Authentication.Commands;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Commands;
using Core.DomainModel.Extensions;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthenticationScheme = Core.DomainModel.Users.AuthenticationScheme;

namespace Presentation.Web.Controllers.API.V1.Auth
{
    [InternalApi]
    public class AuthorizeController : ExtendedApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IApplicationAuthenticationState _applicationAuthenticationState;
        private readonly ICommandBus _commandBus;
        private readonly IOrganizationalUserContext _userContext;

        public AuthorizeController(
            IUserRepository userRepository,
            IUserService userService,
            IOrganizationService organizationService,
            IApplicationAuthenticationState applicationAuthenticationState,
            ICommandBus commandBus,
            IOrganizationalUserContext userContext)
        {
            _userRepository = userRepository;
            _userService = userService;
            _organizationService = organizationService;
            _applicationAuthenticationState = applicationAuthenticationState;
            _commandBus = commandBus;
            _userContext = userContext;
        }

        [HttpGet("api/authorize")]
        public IActionResult GetLogin()
        {
            var user = _userRepository.GetById(_userContext.UserId);
            Logger.Debug($"GetLogin called for {user}");
            if (user == null)
                return Unauthorized();

            var response = Map<User, UserDTO>(user);
            return Ok(response);
        }

        [HttpGet("api/authorize/GetOrganizations")]
        public IActionResult GetOrganizations([FromQuery] string? orderBy = null, [FromQuery] bool? orderByAsc = true)
        {
            var orgs = GetOrganizationsWithMembershipAccess();

            if (!string.IsNullOrEmpty(orderBy))
            {
                if (!string.Equals(orderBy, nameof(OrganizationSimpleDTO.Name)))
                    return BadRequest($"Incorrect {nameof(orderBy)} Property name");

                orgs = orderByAsc.GetValueOrDefault(true) ? orgs.OrderBy(org => org.Name)
                    : orgs.OrderByDescending(org => org.Name);
            }

            var dtos = Map<IEnumerable<Organization>, IEnumerable<OrganizationSimpleDTO>>(orgs.ToList());
            return Ok(dtos);
        }

        [InternalApi]
        [HttpGet("api/authorize/GetOrganizations/{userId}")]
        public IActionResult GetUserOrganizations(int userId)
        {
            return _organizationService.GetUserOrganizations(userId)
                .Select(x => x.OrderBy(user => user.Id))
                .Select(x => x.ToList())
                .Select(Map<IEnumerable<Organization>, IEnumerable<OrganizationSimpleDTO>>)
                .Match(Ok, FromOperationError);
        }

        private IQueryable<Organization> GetOrganizationsWithMembershipAccess()
        {
            var orgs = _organizationService.SearchAccessibleOrganizations();

            //Global admin should se everything but regular users should only see organizations which they are a member of
            if (!_userContext.IsGlobalAdmin())
                orgs = orgs.ByIds(_userContext.OrganizationIds.ToList());
            return orgs;
        }

        [HttpGet("api/authorize/GetOrganization({orgId})")]
        public IActionResult GetOrganization(int orgId)
        {
            var user = _userRepository.GetById(_userContext.UserId);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            var org = GetOrganizationsWithMembershipAccess().SingleOrDefault(o => o.Id == orgId);
            if (org == null)
            {
                return BadRequest("User is not associated with organization");
            }
            var defaultUnit = _organizationService.GetDefaultUnit(org, user);
            var dto = new OrganizationAndDefaultUnitDTO
            {
                Organization = Map<Organization, OrganizationDTO>(org),
                DefaultOrgUnit = defaultUnit != null ? Map<OrganizationUnit, OrgUnitSimpleDTO>(defaultUnit) : null
            };
            return Ok(dto);
        }

        // POST api/Authorize
        [AllowAnonymous]
        [HttpPost("api/authorize")]
        public IActionResult PostLogin([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loginInfo = new { UserId = -1, LoginSuccessful = false };

            try
            {
                var command = new ValidateUserCredentialsCommand(loginDto.Email, loginDto.Password, AuthenticationScheme.Cookie);
                var validationResult = _commandBus.ExecuteWithResult<ValidateUserCredentialsCommand, User>(command);

                if (validationResult.Failed)
                {
                    return Unauthorized();
                }

                var user = validationResult.Value;
                if (user.GetOrganizationIdsWhereRoleIsAssigned(OrganizationRole.RightsHolderAccess).Any())
                {
                    loginInfo = new { UserId = user.Id, LoginSuccessful = true };
                    Logger?.Information($"Rightsholder user blocked from login {loginInfo}");
                    return Forbidden("Rights holders cannot login to KITOS. Please use the token endpoint at 'api/authorize/GetToken'");
                }

                _applicationAuthenticationState.SetAuthenticatedUser(user, loginDto.RememberMe ? AuthenticationScope.Persistent : AuthenticationScope.Session);

                var response = Map<User, UserDTO>(user);
                loginInfo = new { UserId = user.Id, LoginSuccessful = true };
                Logger?.Information($"Uservalidation: Successful {loginInfo}");

                return Created(response);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [AllowAnonymous]
        [HttpPost("api/authorize/log-out")]
        public IActionResult PostLogout()
        {
            try
            {
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                    .GetAwaiter().GetResult();
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [AllowAnonymous]
        [AllowRightsHoldersAccess]
        [HttpPost("api/authorize/resetpassword")]
        public IActionResult PostResetpassword(bool? resetPassword, [FromBody] ResetPasswordDTO dto)
        {
            try
            {
                var resetRequest = _userService.GetPasswordReset(dto.RequestId);

                _userService.ResetPassword(resetRequest, dto.NewPassword);

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/authorize/antiforgery")]
        public IActionResult GetAntiForgeryToken()
        {
            var tokenValue = Guid.NewGuid().ToString("N");

            Response.Cookies.Append(
                Constants.CSRFValues.CookieName,
                tokenValue,
                new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Path = "/",
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromMinutes(Constants.CSRFValues.CookieExpirationMinutes)
                });

            return Content(JsonConvert.SerializeObject(tokenValue), "application/json");
        }
    }
}

