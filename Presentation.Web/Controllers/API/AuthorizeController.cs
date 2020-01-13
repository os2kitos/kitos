using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure;
using Presentation.Web.Models;
using System.Collections.Generic;
using System.Net;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class AuthorizeController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;

        public AuthorizeController(
            IUserRepository userRepository,
            IUserService userService,
            IOrganizationService organizationService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _organizationService = organizationService;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<UserDTO>))]
        public HttpResponseMessage GetLogin()
        {
            var user = KitosUser;
            Logger.Debug($"GetLogin called for {user}");
            try
            {
                var response = Map<User, UserDTO>(user);

                return Ok(response);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [Route("api/authorize/GetOrganizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<OrganizationSimpleDTO>>))]
        public HttpResponseMessage GetOrganizations()
        {
            var user = KitosUser;
            var orgs = _organizationService.GetOrganizations(user);
            var dtos = AutoMapper.Mapper.Map<IEnumerable<Organization>, IEnumerable<OrganizationSimpleDTO>>(orgs);
            return Ok(dtos);
        }

        [Route("api/authorize/GetOrganization({orgId})")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<OrganizationAndDefaultUnitDTO>))]
        public HttpResponseMessage GetOrganization(int orgId)
        {
            var user = KitosUser;
            var org = _organizationService.GetOrganizations(user).Single(o => o.Id == orgId);
            var defaultUnit = _organizationService.GetDefaultUnit(org, user);
            var dto = new OrganizationAndDefaultUnitDTO
            {
                Organization = AutoMapper.Mapper.Map<Organization, OrganizationDTO>(org),
                DefaultOrgUnit = AutoMapper.Mapper.Map<OrganizationUnit, OrgUnitSimpleDTO>(defaultUnit)
            };
            return Ok(dto);
        }

        private User LoginWithToken(string token)
        {
            User user = null;
            var principal = new TokenValidator().Validate(token);
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                Logger.Info($"Uservalidation: Could not validate token.");
                throw new ArgumentException();
            }

            if (principal.Claims.Any(c => c.Type == ClaimTypes.Email || c.Type == ClaimTypes.NameIdentifier))
            {

                var emailClaim = principal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email);
                var uuidClaim = principal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (uuidClaim != null && !String.IsNullOrEmpty(uuidClaim.Value))
                {
                    user = _userRepository.GetByUuid(uuidClaim.Value);
                }
                if (user == null && emailClaim != null)
                {
                    user = _userRepository.GetByEmail(emailClaim.Value);
                    if (user != null && !String.IsNullOrEmpty(uuidClaim.Value))
                    {
                        user.UniqueId = uuidClaim.Value;
                        _userRepository.Update(user);
                        _userRepository.Save();
                    }
                }
            }
            return user;
        }
        //Post api/authorize/gettoken
        [HttpPost]
        [AllowAnonymous]
        [Route("api/authorize/GetToken")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<GetTokenResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
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
                if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
                {
                    Logger.Info("Attempt to login with bad credentials");
                    return Unauthorized("Bad credentials");
                }

                var user = _userRepository.GetByEmail(loginDto.Email);
                if (user == null)
                {
                    Logger.Error("User found during membership validation but could not be found by email: {email}", loginDto.Email);
                    return BadRequest();
                }

                if (!user.HasApiAccess.GetValueOrDefault())
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
                    Expires = token.Expiration,
                    ActiveOrganizationId = token.ActiveOrganizationId
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

        // POST api/Authorize
        [AllowAnonymous]
        public HttpResponseMessage PostLogin(LoginDTO loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest();
            }

            var loginInfo = new { Email = loginDto.Email, LoginSuccessful = false };

            try
            {
                User user;
                if (!string.IsNullOrEmpty(loginDto.Token))
                {
                    user = LoginWithToken(loginDto.Token);
                    if (user == null)
                    {
                        Logger.Info($"Uservalidation: Unsuccessful login with token. {loginInfo}");
                        return Unauthorized("Invalid token");
                    }
                }
                else
                {
                    if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
                    {
                        Logger.Info($"Uservalidation: Unsuccessful login with credentials. {loginInfo}");
                        return Unauthorized("Bad credentials");
                    }

                    user = _userRepository.GetByEmail(loginDto.Email);
                    if (user == null)
                    {
                        Logger.Error($"User found during membership validation but could not be found by email: {loginDto.Email}");
                        return BadRequest();
                    }
                }

                FormsAuthentication.SetAuthCookie(user.Id.ToString(), loginDto.RememberMe);
                var response = Map<User, UserDTO>(user);
                loginInfo = new { loginDto.Email, LoginSuccessful = true };
                Logger.Info($"Uservalidation: Successful {loginInfo}");

                return Created(response);
            }
            catch (Exception e)
            {
                Logger.Info($"Uservalidation: Error. {loginInfo}");

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
    }
}
