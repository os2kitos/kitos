using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class GlobalAdminController : BaseApiController
    {
        private readonly IOrganizationalUserContext _organizationalUserContext;
        private readonly IGenericRepository<User> _userRepository;

        public GlobalAdminController(IOrganizationalUserContext organizationalUserContext, IGenericRepository<User> userRepository)
        {
            _organizationalUserContext = organizationalUserContext;
            _userRepository = userRepository;
        }

        private bool HasAccess()
        {
            return _organizationalUserContext.IsGlobalAdmin();
        }

        public HttpResponseMessage Get()
        {
            try
            {
                if (!HasAccess())
                {
                    return Forbidden();
                }

                var users = _userRepository.Get(u => u.IsGlobalAdmin);

                var dtos = Mapper.Map<IEnumerable<UserDTO>>(users);

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

                var user = _userRepository.GetByKey(dto.UserId);

                //if already global admin, return conflict
                if (user.IsGlobalAdmin)
                {
                    return Conflict(user.Name + " is already global admin");
                }

                user.IsGlobalAdmin = true;
                _userRepository.Save();

                var outDto = Mapper.Map<UserDTO>(user);

                return Created(outDto);

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

                    var user = _userRepository.GetByKey(userId);

                    user.IsGlobalAdmin = false;
                    _userRepository.Save();

                    var outDto = Mapper.Map<UserDTO>(user);

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
