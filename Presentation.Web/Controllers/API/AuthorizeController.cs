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

namespace Presentation.Web.Controllers.API
{
    public class AuthorizeController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        public AuthorizeController(IUserRepository userRepository, IUserService userService, IOrganizationService organizationService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _organizationService = organizationService;
        }

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
        public HttpResponseMessage GetOrganizations()
        {
            var user = KitosUser;
            var orgs = _organizationService.GetOrganizations(user);
            var dtos = AutoMapper.Mapper.Map<IEnumerable<Organization>, IEnumerable<OrganizationSimpleDTO>>(orgs);
            return Ok(dtos);
        }

        [Route("api/authorize/GetOrganization({orgId})")]
        public HttpResponseMessage GetOrganization(int orgId)
        {
            var user = KitosUser;
            var org = _organizationService.GetOrganizations(user).Single(o => o.Id == orgId);
            var defaultUnit = _organizationService.GetDefaultUnit(org, user);
            var dto = new OrganizationAndDefaultUnitDTO()
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
        public HttpResponseMessage GetToken(LoginDTO loginDto) {

            try
            {
                if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
                {
                    Logger.Info("Attempt to login with bad credentials");
                    return Unauthorized("Bad credentials");
                }

                var user = _userRepository.GetByEmail(loginDto.Email);

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
                Logger.Error(e,"Failed to create token");
                return LogError(e);
            }
        }

        // POST api/Authorize
        [AllowAnonymous]
        public HttpResponseMessage PostLogin(LoginDTO loginDto)
        {
            var loginInfo = new { Email = "", LoginSuccessful = false };

            if (loginDto == null)
            {
                return BadRequest();
            }
            loginInfo = new { Email = loginDto.Email, LoginSuccessful = false };

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
