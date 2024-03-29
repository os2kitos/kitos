﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Extensions;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class OrganizationUnitController : GenericHierarchyApiController<OrganizationUnit, OrgUnitDTO>
    {
        private readonly IOrgUnitService _orgUnitService;
        private readonly IOrganizationUnitService _organizationUnitService;
        private readonly IOrganizationalUserContext _userContext;

        public OrganizationUnitController(
            IGenericRepository<OrganizationUnit> repository,
            IOrgUnitService orgUnitService,
            IOrganizationUnitService organizationUnitService,
            IOrganizationalUserContext userContext)
            : base(repository)
        {
            _orgUnitService = orgUnitService;
            _organizationUnitService = organizationUnitService;
            _userContext = userContext;
        }

        public HttpResponseMessage Post(OrgUnitDTO dto) => base.Post(dto.OrganizationId, dto);

        protected override void PrepareNewObject(OrganizationUnit item)
        {
            item.Uuid = Guid.NewGuid();
            base.PrepareNewObject(item);
        }

        [NonAction]
        public override HttpResponseMessage Post(int organizationId, OrgUnitDTO dto) => throw new NotSupportedException();

        /// <summary>
        /// Returns every OrganizationUnit that the user can select as the default unit
        /// </summary>
        /// <param name="byUser">Routing qualifier</param>
        /// <returns></returns>
        public HttpResponseMessage GetByUser(bool? byUser, int organizationId)
        {
            try
            {
                if (GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.Public)
                    return Forbidden();

                var userId = _userContext.UserId;
                var orgUnits = Repository
                    .Get(x => x.Rights.Any(y => y.UserId == userId) && x.OrganizationId == organizationId)
                    .SelectMany(unit => unit.FlattenHierarchy())
                    .Distinct()
                    .ToList();

                return Ok(Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitSimpleDTO>>(orgUnits));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetByOrganization(int organization)
        {
            try
            {
                var root = Repository
                    .AsQueryable()
                    .ByOrganizationId(organization)
                    .FirstOrDefault(unit => unit.Parent == null);

                if (root == null) return NotFound();

                if (!AllowRead(root))
                {
                    return Forbidden();
                }

                var item = Map<OrganizationUnit, OrgUnitDTO>(root);

                return Ok(item);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetByOrganizationFlat(int organizationId)
        {
            try
            {
                if (GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.Public)
                    return Forbidden();

                var orgUnit =
                    Repository
                        .AsQueryable()
                        .ByOrganizationId(organizationId)
                        .ToList();

                return Ok(Map(orgUnit));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            try
            {
                var unit = Repository
                    .AsQueryable()
                    .ById(id);
                if (unit == null)
                {
                    return BadRequest($"Unit with id: {id} was not found");
                }

                var accessRightsResult = _organizationUnitService.GetAccessRights(unit.Organization.Uuid, unit.Uuid);
                if (accessRightsResult.Failed)
                {
                    return FromOperationError(accessRightsResult.Error);
                }
                var accessRights = accessRightsResult.Value;

                if (obj.TryGetValue("parentId", out var jtoken))
                {
                    var parentId = jtoken.Value<int>();

                    if (parentId != unit.ParentId.GetValueOrDefault())
                    {
                        if (!accessRights.CanBeRearranged)
                        {
                            return BadRequest("Unit cannot change its parent");
                        }
                    }
                    //if the new parent is actually a descendant of the item, don't update - this would create a loop!
                    if (_orgUnitService.DescendsFrom(parentId, id))
                    {
                        return Conflict("OrgUnit loop detected");
                    }
                }
                if (obj.TryGetValue("name", out jtoken))
                {
                    var newName = jtoken.Value<string>();

                    if (newName != unit.Name)
                    {
                        if (!accessRights.CanBeRenamed)
                        {
                            return BadRequest("Unit cannot be renamed");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return LogError(e);
            }
            return base.Patch(id, organizationId, obj);
        }

        [NonAction]
        public override HttpResponseMessage Put(int id, int organizationId, JObject jObject) => throw new NotSupportedException();

        [NonAction]
        public override HttpResponseMessage Delete(int id, int organizationId) => throw new NotSupportedException();
    }
}
