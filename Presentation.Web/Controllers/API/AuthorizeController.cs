using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Models;

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
            Logger.Debug($"GetLogin called for {KitosUser}");
            try
            {
                var response = CreateLoginResponse(KitosUser);

                return Ok(response);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        // POST api/Authorize
        [AllowAnonymous]
        public HttpResponseMessage PostLogin(LoginDTO loginDto)
        {
            var loginInfo = new { Email = "", Password = "", LoginSuccessful = false };

            if (loginDto != null)
                loginInfo = new { Email = loginDto.Email, Password = "********", LoginSuccessful = false };

            try
            {
                if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
                {
                    throw new ArgumentException();
                }

                var user = _userRepository.GetByEmail(loginDto.Email);
                FormsAuthentication.SetAuthCookie(user.Id.ToString(), loginDto.RememberMe);
                var response = CreateLoginResponse(user);
                loginInfo = new { loginDto.Email, Password = "********", LoginSuccessful = true };
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
        private LoginResponseDTO CreateLoginResponse(User user)
        {
            var userDto = AutoMapper.Mapper.Map<User, UserDTO>(user);

            // getting all the organizations that the user is member of
            var organizations = _organizationService.GetOrganizations(user).ToList();
            // getting the default org units (one or null for each organization)
            var defaultUnits = organizations.Select(org => _organizationService.GetDefaultUnit(org, user));

            // creating DTOs
            var orgsDto = organizations.Zip(defaultUnits, (org, defaultUnit) => new OrganizationAndDefaultUnitDTO()
            {
                Organization = AutoMapper.Mapper.Map<Organization, OrganizationDTO>(org),
                DefaultOrgUnit = AutoMapper.Mapper.Map<OrganizationUnit, OrgUnitSimpleDTO>(defaultUnit)
            });

            return new LoginResponseDTO()
            {
                User = userDto,
                Organizations = orgsDto
            };
        }
    }
}
