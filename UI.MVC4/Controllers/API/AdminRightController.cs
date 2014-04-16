using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class AdminRightController : BaseApiController
    {
        private readonly IAdminService _adminService;
        private readonly IGenericRepository<Organization> _organizationRepository;

        public AdminRightController(IAdminService adminService, IGenericRepository<Organization> organizationRepository)
        {
            _adminService = adminService;
            _organizationRepository = organizationRepository;
        }

        //get all admin rights
        //returns IEnumerable<RightOutputDTO>
        public HttpResponseMessage Get()
        {
            try
            {
                //gotta be global admin to do this
                if (!IsGlobalAdmin()) return Unauthorized();

                var rights = _adminService.GetAdminRights();
                var dtos = AutoMapper.Mapper.Map<IEnumerable<AdminRight>, IEnumerable<RightOutputDTO>>(rights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        //returns RightOutputDTO
        public HttpResponseMessage Post(RightInputDTO inputDTO)
        {
            try
            {
                //gotta be global admin to do this
                if (!IsGlobalAdmin()) return Unauthorized();

                //we only allow posting a local admin role
                var localAdminRole = AdminService.GetLocalAdminRole();
                if (inputDTO.RoleId != localAdminRole.Id) return NotAllowed();

                var org = _organizationRepository.GetByKey(inputDTO.ObjectId);
                var user = UserRepository.GetByKey(inputDTO.UserId);
                
                var right = _adminService.MakeLocalAdmin(user, org);

                var outputDTO = AutoMapper.Mapper.Map<AdminRight, RightOutputDTO>(right);

                return Created(outputDTO); //TODO add url

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Delete(RightInputDTO inputDTO)
        {
            try
            {
                //gotta be global admin to do this
                if (!IsGlobalAdmin()) return Unauthorized();

                var org = _organizationRepository.GetByKey(inputDTO.ObjectId);
                var user = UserRepository.GetByKey(inputDTO.UserId);

                _adminService.RemoveLocalAdmin(user, org);

                return Ok();

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
