using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Security;
using Core.ApplicationServices;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Cryptography;
using Newtonsoft.Json;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;
using AuthenticationScheme = Core.DomainModel.Users.AuthenticationScheme;

namespace Presentation.Web.Controllers.API.V1.Auth
{
    [InternalApi]
    public class AuthorizeController : BaseAuthenticationController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IApplicationAuthenticationState _applicationAuthenticationState;

        public AuthorizeController(
            IUserRepository userRepository,
            IUserService userService,
            IOrganizationService organizationService,
            ICryptoService cryptoService,
            IApplicationAuthenticationState applicationAuthenticationState)
            :base(userRepository,cryptoService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _organizationService = organizationService;
            _applicationAuthenticationState = applicationAuthenticationState;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<UserDTO>))]
        public HttpResponseMessage GetLogin()
        {
            var user = _userRepository.GetById(UserId);
            Logger.Debug($"GetLogin called for {user}");
            var response = Map<User, UserDTO>(user);
            return Ok(response);
        }

        [Route("api/authorize/GetOrganizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<OrganizationSimpleDTO>>))]
        public HttpResponseMessage GetOrganizations([FromUri] string orderBy = null, [FromUri] bool? orderByAsc = true)
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
        [Route("api/authorize/GetOrganizations/{userId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<OrganizationSimpleDTO>>))]
        public HttpResponseMessage GetUserOrganizations(int userId)
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
            if (!UserContext.IsGlobalAdmin())
                orgs = orgs.ByIds(UserContext.OrganizationIds.ToList());
            return orgs;
        }

        [Route("api/authorize/GetOrganization({orgId})")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<OrganizationAndDefaultUnitDTO>))]
        public HttpResponseMessage GetOrganization(int orgId)
        {
            var user = _userRepository.GetById(UserId);
            var org = GetOrganizationsWithMembershipAccess().SingleOrDefault(o => o.Id == orgId);
            if (org == null)
            {
                return BadRequest("User is not associated with organization");
            }
            var defaultUnit = _organizationService.GetDefaultUnit(org, user);
            var dto = new OrganizationAndDefaultUnitDTO
            {
                Organization = Map<Organization, OrganizationDTO>(org),
                DefaultOrgUnit = Map<OrganizationUnit, OrgUnitSimpleDTO>(defaultUnit)
            };
            return Ok(dto);
        }

        // POST api/Authorize
        [AllowAnonymous]
        public HttpResponseMessage PostLogin(LoginDTO loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest();
            }

            var loginInfo = new { UserId = -1, LoginSuccessful = false };

            try
            {
                var result = AuthenticateUser(loginDto, AuthenticationScheme.Cookie);

                if (result.Failed)
                {
                    return result.Error;
                }

                var user = result.Value;
                if (user.GetOrganizationIdsWhereRoleIsAssigned(OrganizationRole.RightsHolderAccess).Any())
                {
                    loginInfo = new { UserId = user.Id, LoginSuccessful = true };
                    Logger.Info($"Rightsholder user blocked from login {loginInfo}");
                    return Forbidden("Rights holders cannot login to KITOS. Please use the token endpoint at 'api/authorize/GetToken'");
                }

                _applicationAuthenticationState.SetAuthenticatedUser(user, loginDto.RememberMe ? AuthenticationScope.Persistent : AuthenticationScope.Session);

                var response = Map<User, UserDTO>(user);
                loginInfo = new { UserId = user.Id, LoginSuccessful = true };
                Logger.Info($"Uservalidation: Successful {loginInfo}");

                return Created(response);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [AllowAnonymous]
        [Route("api/authorize/log-out")]
        public HttpResponseMessage PostLogout()
        {
            try
            {
                FormsAuthentication.SignOut();
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [AllowAnonymous]
        [AllowRightsHoldersAccess]
        public HttpResponseMessage PostResetpassword(bool? resetPassword, ResetPasswordDTO dto)
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
        public HttpResponseMessage GetAntiForgeryToken()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            var cookie = HttpContext.Current.Request.Cookies[Constants.CSRFValues.CookieName];

            AntiForgery.GetTokens(cookie == null ? "" : cookie.Value, out var cookieToken, out var formToken);

            response.Content = new StringContent(JsonConvert.SerializeObject(formToken), Encoding.UTF8, "application/json");

            if (CookieAlreadySet(cookieToken))
            {
                response.Headers.AddCookies(new[]
                {
                    new CookieHeaderValue(Constants.CSRFValues.CookieName, cookieToken)
                    {
                        Path = "/",
                        Secure = true,
                    }
                });
            }

            return response;
        }

        private static bool CookieAlreadySet(string cookieToken)
        {
            return !string.IsNullOrEmpty(cookieToken);
        }
    }
}
