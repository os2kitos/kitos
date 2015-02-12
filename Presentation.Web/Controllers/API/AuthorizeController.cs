using System;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    //TODO refactor this mess
    public class AuthorizeController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        

        public AuthorizeController(IUserRepository userRepository, IUserService userService,  IOrganizationService organizationService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _organizationService = organizationService;
        }

        private LoginResponseDTO CreateLoginResponse(User user)
        {
            var userDto = AutoMapper.Mapper.Map<User, UserDTO>(user);

            var orgsAndDefaultUnits = _organizationService.GetByUser(user);
            var orgsAndDefaultUnitsDto = orgsAndDefaultUnits.Select(x => new LoginOrganizationDTO()
            {
                Organization = AutoMapper.Mapper.Map<Organization, OrganizationDTO>(x.Key),
                DefaultOrgUnit = AutoMapper.Mapper.Map<OrganizationUnit, OrgUnitSimpleDTO>(x.Value)
            }).ToList();

            return new LoginResponseDTO()
            {
                User = userDto,
                Organizations = orgsAndDefaultUnitsDto
            };

        }

        public HttpResponseMessage GetLogin()
        { 
            try
            {
                var response = CreateLoginResponse(KitosUser);

                return Ok(response);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        // POST api/Authorize
        public HttpResponseMessage PostLogin(LoginDTO loginDto)
        {
            try
            {
                if (!Membership.ValidateUser(loginDto.Email, loginDto.Password))
                    throw new ArgumentException();

                var user = _userRepository.GetByEmail(loginDto.Email);

                FormsAuthentication.SetAuthCookie(user.Id.ToString(), loginDto.RememberMe);

                var response = CreateLoginResponse(user);

                return Created(response);
            }
            catch (ArgumentException)
            {
                return Unauthorized("Bad credentials");
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostLogout(bool? logout)
        {
            try
            {
                FormsAuthentication.SignOut();
                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }


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
                return Error(e);
            }
        }
    }
}
