using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Ninject.Extensions.Logging;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    //TODO refactor this mess
    public class AuthorizeController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly ILogger _logger;

        public AuthorizeController(IUserRepository userRepository, IUserService userService, IOrganizationService organizationService, ILogger logger)
        {
            _userRepository = userRepository;
            _userService = userService;
            _organizationService = organizationService;
            _logger = logger;
        }

        public HttpResponseMessage GetLogin()
        {
            _logger.Debug("GetLogin called for {user}", KitosUser);
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
            var loginInformation = new { loginDto.Email, Password = "********", LoginSuccessful = false };

            try
            {
                if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
                {
                    throw new ArgumentException();
                }

                var user = _userRepository.GetByEmail(loginDto.Email);

                FormsAuthentication.SetAuthCookie(user.Id.ToString(), loginDto.RememberMe);

                var response = CreateLoginResponse(user);

                loginInformation = new { loginDto.Email, Password = "********", LoginSuccessful = true };

                _logger.Info("Uservalidation: Successful {@loginInformation}", loginInformation);

                return Created(response);
            }
            catch (ArgumentException)
            {
                _logger.Info("Uservalidation: Not valid. {@loginInformation}", loginInformation);

                return Unauthorized("Bad credentials");
            }
            catch (Exception e)
            {
                _logger.Info("Uservalidation: Error. {@loginInformation}", loginInformation);

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
