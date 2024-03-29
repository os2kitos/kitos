﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Organization.DomainEvents;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class GlobalAdminController : BaseApiController
    {
        private readonly IOrganizationalUserContext _organizationalUserContext;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IDomainEvents _domainEvents;

        public GlobalAdminController(IOrganizationalUserContext organizationalUserContext, IGenericRepository<User> userRepository, IDomainEvents domainEvents)
        {
            _organizationalUserContext = organizationalUserContext;
            _userRepository = userRepository;
            _domainEvents = domainEvents;
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
                _domainEvents.Raise(new AdministrativeAccessRightsChanged(dto.UserId));

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
                    _domainEvents.Raise(new AdministrativeAccessRightsChanged(userId));

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
