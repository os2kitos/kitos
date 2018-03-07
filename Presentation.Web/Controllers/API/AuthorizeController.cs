using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure;
using Presentation.Web.Models;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

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
            var org = _organizationService.GetOrganizations(user).Single(o=>o.Id == orgId);
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

        // POST api/Authorize
        [AllowAnonymous]
        public HttpResponseMessage PostLogin(LoginDTO loginDto)
        {
            var loginInfo = new { Token="", Email = "", Password = "", LoginSuccessful = false };

            if (loginDto != null)
                loginInfo = new { Token = loginInfo.Token, Email = loginDto.Email, Password = "********", LoginSuccessful = false };

            try
            {
                User user;
                if (!string.IsNullOrEmpty(loginDto.Token))
                {
                    user = LoginWithToken(loginDto.Token);
                    if (user == null)
                    {
                        throw new ArgumentException();
                    }

                }
                else
                {
                    if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
                    {
                        throw new ArgumentException();
                    }

                    user = _userRepository.GetByEmail(loginDto.Email);
                }

                var Secret = "db3OIsj+BXE9NZDy0t8W3TcNekrF+2d/1sFnWG4HnV8TZY30iTOdtVWJG8abWvB1GlOgJuQZdcF2Luqm/hccMw==";
                var symmetricKey = Convert.FromBase64String(Secret);

                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                  {
                        new Claim(ClaimTypes.Name, loginDto.Email)
                    }),

                    //   Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),

                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
                };

                var stoken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(stoken);




                FormsAuthentication.SetAuthCookie(user.Id.ToString(), loginDto.RememberMe);
                var response = Map<User, UserDTO>(user);
                loginInfo = new {loginInfo.Token, loginDto.Email, Password = "********", LoginSuccessful = true };
                Logger.Info($"Uservalidation: Successful {loginInfo}");

                return Created(response);
            }
            catch (ArgumentException)
            {
                Logger.Info($"Uservalidation: Unsuccessful. {loginInfo}");

                return Unauthorized("Bad credentials");
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

        // helper function
        private LoginResponseDTO CreateLoginResponse(User user, IEnumerable<Organization> organizations)
        {
            var userDto = AutoMapper.Mapper.Map<User, UserDTO>(user);

            // getting the default org units (one or null for each organization)
            var defaultUnits = organizations.Select(org => _organizationService.GetDefaultUnit(org, user));

            // creating DTOs
            var orgsDto = organizations.Zip(defaultUnits, (org, defaultUnit) => new OrganizationAndDefaultUnitDTO()
            {
                Organization = AutoMapper.Mapper.Map<Organization, OrganizationDTO>(org),
                DefaultOrgUnit = AutoMapper.Mapper.Map<OrganizationUnit, OrgUnitSimpleDTO>(defaultUnit)
            });


            var response = new LoginResponseDTO()
            {
                User = userDto,
                Organizations = orgsDto
            };

            return response;
        }
    }
}
