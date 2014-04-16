using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class GlobalAdminController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public GlobalAdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public HttpResponseMessage Get()
        {
            try
            {
                if (!IsGlobalAdmin()) return Unauthorized();

                var users = UserRepository.Get(u => u.IsGlobalAdmin);

                var dtos = AutoMapper.Mapper.Map<IEnumerable<UserDTO>>(users);

                return Ok(dtos);

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Post(CreateGlobalAdminDTO dto)
        {
            try
            {
                if (!IsGlobalAdmin()) return Unauthorized();

                var user = UserRepository.GetByKey(dto.UserId);

                //if already global admin, return conflict
                if (user.IsGlobalAdmin) return Conflict(user.Name + " is already global admin");

                user.IsGlobalAdmin = true;
                UserRepository.Save();

                var outDto = AutoMapper.Mapper.Map<UserDTO>(user);
                
                return Created(outDto); //TODO location?

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Delete([FromUri] int userId)
        {
            try
            {
                if (!IsGlobalAdmin()) return Unauthorized();

                var user = UserRepository.GetByKey(userId);

                user.IsGlobalAdmin = false;
                UserRepository.Save();

                var outDto = AutoMapper.Mapper.Map<UserDTO>(user);

                return Ok(outDto);

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        //private readonly IUserService _userService;

        //public GlobalAdminController(IUserService userService)
        //{
        //    _userService = userService;
        //}

        //public HttpResponseMessage Post(CreateGlobalAdminDTO item)
        //{
        //    try
        //    {
        //        var user = UserRepository.GetByKey(item.UserId);

        //        user.IsGlobalAdmin = true;

        //        UserRepository.Save();

        //        return Created(AutoMapper.Mapper.Map<User, UserDTO>(user), new Uri(Request.RequestUri + "/" + user.Id));
        //    }
        //    catch (Exception e)
        //    {
        //        return Error(e);
        //    }
        //}
    }
}
