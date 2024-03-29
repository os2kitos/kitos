﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainServices;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class OrganizationRightController : GenericApiController<OrganizationRight, OrganizationRightDTO>
    {
        private readonly IGenericRepository<OrganizationRight> _rightRepository;
        private readonly IOrganizationRightsService _organizationRightsService;

        public OrganizationRightController(
            IGenericRepository<OrganizationRight> rightRepository,
            IOrganizationRightsService organizationRightsService)
            : base(rightRepository)
        {
            _rightRepository = rightRepository;
            _organizationRightsService = organizationRightsService;
        }

        public virtual HttpResponseMessage GetAllLocalAdmins(bool? allLocalAdmins)
        {
            try
            {
                if (GetCrossOrganizationReadAccessLevel() < CrossOrganizationDataReadAccessLevel.All)
                {
                    return Forbidden();
                }

                return _rightRepository
                    .AsQueryable()
                    .Where(x => x.Role == OrganizationRole.LocalAdmin)
                    .AsEnumerable()
                    .Select(Map<OrganizationRight, OrganizationRightDTO>)
                    .ToList()
                    .Transform(Ok);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Returns all rights for an object
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <returns>List of all rights</returns>
        protected IEnumerable<OrganizationRight> GetRightsQuery(int id)
        {
            return _rightRepository.Get(right => right.OrganizationId == id);
        }

        [NonAction]
        public override HttpResponseMessage Post(int organizationId, OrganizationRightDTO dto) => throw new NotSupportedException();

        public HttpResponseMessage Post(OrganizationRightDTO dto)
        {
            var organizationRight = Map<OrganizationRightDTO, OrganizationRight>(dto);

            try
            {
                var result = _organizationRightsService.AssignRole(dto.OrganizationId, organizationRight.UserId, organizationRight.Role);

                return result.Ok ?
                    NewObjectCreated(result.Value) :
                    FromOperationFailure(result.Error);
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to add right", e);
                return LogError(e);
            }
        }

        /// <summary>
        /// Delete a right from the object
        /// </summary>
        /// <param name="id">ID of organization to remove from</param>
        /// <param name="rId">ID of role</param>
        /// <param name="uId">ID of user in role</param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public HttpResponseMessage Delete(int id, [FromUri] int rId, [FromUri] int uId, int organizationId)
        {
            try
            {
                var result = _organizationRightsService.RemoveRole(id, uId, (OrganizationRole)rId);

                return result.Ok ?
                    Ok() :
                    FromOperationFailure(result.Error);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [NonAction]
        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj) => throw new NotSupportedException();
    }
}
