using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    //TODO refactor this into OrganizationRightsController
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GlobalAdminController : BaseApiController
    {
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
                return LogError(e);
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
                user.LastChanged = DateTime.UtcNow;
                user.LastChangedByUser = KitosUser;
                UserRepository.Save();

                var outDto = AutoMapper.Mapper.Map<UserDTO>(user);

                return Created(outDto); //TODO location?

            }
            catch (Exception e)
            {
                return LogError(e);
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
                return LogError(e);
            }
        }
    }
}
