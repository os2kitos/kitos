﻿using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Helpers;
using Core.Abstractions.Types;
using Core.ApplicationServices;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Organizations;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Cryptography;
using Newtonsoft.Json;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    public class AuthorizeController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly ICryptoService _cryptoService;
        private readonly IApplicationAuthenticationState _applicationAuthenticationState;

        public AuthorizeController(
            IUserRepository userRepository,
            IUserService userService,
            IOrganizationService organizationService,
            ICryptoService cryptoService,
            IApplicationAuthenticationState applicationAuthenticationState)
        {
            _userRepository = userRepository;
            _userService = userService;
            _organizationService = organizationService;
            _cryptoService = cryptoService;
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
        public HttpResponseMessage GetOrganizations([FromUri]string orderBy = null, [FromUri]bool? orderByAsc = true)
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

        //Post api/authorize/gettoken
        [HttpPost]
        [AllowAnonymous]
        [Route("api/authorize/GetToken")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<GetTokenResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [IgnoreCSRFProtection]
        [AllowRightsHoldersAccess]
        public HttpResponseMessage GetToken(LoginDTO loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest();
            }
            try
            {
                var result = AuthenticateUser(loginDto);

                if (result.Failed)
                {
                    return result.Error;
                }

                var user = result.Value;

                if (CanIssueTokenTo(user))
                {
                    Logger.Warn("User with Id {id} tried to use get a token for the API but was forbidden", user.Id);
                    return Forbidden();
                }

                var token = new TokenValidator().CreateToken(user);

                var response = new GetTokenResponseDTO
                {
                    Token = token.Value,
                    Email = loginDto.Email,
                    LoginSuccessful = true,
                    Expires = token.Expiration
                };

                Logger.Info($"Created token for user with Id {user.Id}");

                return Ok(response);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to create token");
                return LogError(e);
            }
        }

        private static bool CanIssueTokenTo(User user)
        {
            return user.HasApiAccess.GetValueOrDefault(false) == false;
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
                var result = AuthenticateUser(loginDto);

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
        public HttpResponseMessage PostLogout(bool? logout)
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
        
        private Result<User, HttpResponseMessage> AuthenticateUser(LoginDTO loginDto)
        {
            if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
            {
                Logger.Info("AUTH FAILED: Attempt to login with bad credentials for {hashEmail}", _cryptoService.Encrypt(loginDto.Email ?? ""));
                {
                    return Unauthorized();
                }
            }

            var user = _userRepository.GetByEmail(loginDto.Email);
            if (user == null)
            {
                Logger.Error("AUTH FAILED: User found during membership validation but could not be found by email: {hashEmail}", _cryptoService.Encrypt(loginDto.Email));
                {
                    return Unauthorized();
                }
            }

            if (user.CanAuthenticate())
            {
                return user;
            }

            Logger.Info("'AUTH FAILED: Non-global admin' User with id {userId} and no organization rights denied access", user.Id);
            {
                return Unauthorized();
            }
        }

        private static bool CookieAlreadySet(string cookieToken)
        {
            return !string.IsNullOrEmpty(cookieToken);
        }
    }
}