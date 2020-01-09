using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class GlobalAdminController : BaseApiController
    {
        private readonly IOrganizationalUserContext _organizationalUserContext;

        public GlobalAdminController(
            IAuthorizationContext authorizationContext, 
            IOrganizationalUserContext organizationalUserContext)
            : base(authorizationContext)
        {
            _organizationalUserContext = organizationalUserContext;
        }

        private bool HasAccess()
        {
            return _organizationalUserContext.HasRole(OrganizationRole.GlobalAdmin);
        }

        public HttpResponseMessage Get()
        {
            try
            {
                if (!HasAccess())
                {
                    return Forbidden();
                }

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
                if (!HasAccess())
                {
                    return Forbidden();
                }

                var user = UserRepository.GetByKey(dto.UserId);

                //if already global admin, return conflict
                if (user.IsGlobalAdmin)
                {
                    return Conflict(user.Name + " is already global admin");
                }

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
                if (HasAccess())
                {
                    if (_organizationalUserContext.UserId == userId)
                    {
                        return BadRequest("Cannot remove own global admin rights");
                    }

                    var user = UserRepository.GetByKey(userId);

                    user.IsGlobalAdmin = false;
                    UserRepository.Save();

                    var outDto = AutoMapper.Mapper.Map<UserDTO>(user);

                    return Ok(outDto);
                }

                return Forbidden();

            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
