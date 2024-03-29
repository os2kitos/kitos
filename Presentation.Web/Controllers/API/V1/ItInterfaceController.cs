﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Interface;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class ItInterfaceController : GenericApiController<ItInterface, ItInterfaceDTO>
    {
        private readonly IItInterfaceService _itInterfaceService;

        public ItInterfaceController(
            IGenericRepository<ItInterface> repository,
            IItInterfaceService itInterfaceService)
            : base(repository)
        {
            _itInterfaceService = itInterfaceService;
        }

        //DELETE api/ItInterface
        public override HttpResponseMessage Delete(int id, int organizationId)
        {
            try
            {
                return _itInterfaceService
                    .Delete(id)
                    .Match(_ => Ok(), FromOperationFailure);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [NonAction]
        public override HttpResponseMessage Post(int organizationId, ItInterfaceDTO dto) => throw new NotSupportedException();

        public HttpResponseMessage Post(ItInterfaceDTO dto)
        {
            try
            {
                var itInterfaceId = dto.ItInterfaceId ?? string.Empty;
                var result = _itInterfaceService.CreateNewItInterface(dto.OrganizationId, dto.Name, itInterfaceId, null, dto.AccessModifier);
                return result.Ok ?
                    Created(Map(result.Value), new Uri(Request.RequestUri + "/" + result.Value.Id)) :
                    FromOperationError(result.Error);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            // try get name value
            JToken nameToken;
            obj.TryGetValue("name", out nameToken);
            if (nameToken != null)
            {
                var orgId = Repository.GetByKey(id).OrganizationId;
                if (!IsAvailable(nameToken.Value<string>(), orgId))
                    return Conflict("Name is already taken!");
            }

            var itInterface = Repository.GetByKey(id);

            if (itInterface == null)
            {
                return NotFound();
            }

            if (obj.TryGetValue(nameof(ItInterface.Uuid), StringComparison.OrdinalIgnoreCase, out var uuidToken) &&
                uuidToken.ToObject<Guid>() != itInterface.Uuid)
            {
                return BadRequest("Cannot change uuid");
            }

            if (obj.TryGetValue(nameof(ItInterface.AccessModifier), StringComparison.OrdinalIgnoreCase, out var newAccessModifuer) &&
                Enum.TryParse<AccessModifier>(newAccessModifuer.Value<string>(), out var newModifier) &&
                newModifier != itInterface.AccessModifier)
            {
                if (!AuthorizationContext.HasPermission(new VisibilityControlPermission(itInterface)))
                    return Forbidden("Not allowed to change visibility of entity");
            }

            return base.Patch(id, organizationId, obj);
        }

        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "It Interface name must be new")]
        public HttpResponseMessage GetNameAvailable(string checkname, int orgId)
        {
            try
            {
                var accessLevel = GetOrganizationReadAccessLevel(orgId);
                if (accessLevel < OrganizationDataReadAccessLevel.Public)
                {
                    return Forbidden();
                }
                return IsAvailable(checkname, orgId) ? Ok() : Conflict("Name is already taken!");
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "It Interface Id and name must be unique")]
        public HttpResponseMessage GetItInterfaceNameUniqueConstraint(string checkitinterfaceid, string checkname, int orgId)
        {
            try
            {
                var accessLevel = GetOrganizationReadAccessLevel(orgId);
                if (accessLevel < OrganizationDataReadAccessLevel.Public)
                {
                    return Forbidden();
                }
                return IsItInterfaceIdAndNameUnique(checkitinterfaceid, checkname, orgId) ? Ok() : Conflict("Name and ItInterfaceId is already taken by a single interface!");
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        private bool IsItInterfaceIdAndNameUnique(string itInterfaceId, string name, int orgId)
        {
            if (itInterfaceId == "undefined") itInterfaceId = null;
            var system = Repository.Get(x => x.ItInterfaceId == (itInterfaceId ?? string.Empty) && x.Name == name && x.OrganizationId == orgId);
            return !system.Any();
        }

        private bool IsAvailable(string name, int orgId)
        {
            var system = Repository.Get(x => x.Name == name && x.OrganizationId == orgId);
            return !system.Any();
        }
    }
}
