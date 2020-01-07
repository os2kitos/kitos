using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [ControllerEvaluationCompleted]
    public class ItInterfaceController : GenericContextAwareApiController<ItInterface, ItInterfaceDTO>
    {
        private readonly IItInterfaceService _itInterfaceService;

        public ItInterfaceController(IGenericRepository<ItInterface> repository, IItInterfaceService itInterfaceService)
            : base(repository)
        {
            _itInterfaceService = itInterfaceService;
        }

        // Udkommenteret ifm. OS2KITOS-663
        //DELETE api/ItInterface
        public override HttpResponseMessage Delete(int id, int organizationId)
        {
            try
            {
                var item = Repository.GetByKey(id);

                // Udkommenteret ifm. OS2KITOS-663
                // check if the itinterface has any usages, if it does it's may not be deleted
                //if (item.ExhibitedBy != null || item.CanBeUsedBy.Any())
                if (item.ExhibitedBy != null)
                    return Conflict("Cannot delete an itinterface in use!");

                return base.Delete(id, organizationId);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override void DeleteQuery(ItInterface entity)
        {
            _itInterfaceService.Delete(entity.Id);
        }

        [DeprecatedApi]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceDTO>>))]
        public HttpResponseMessage GetSearch(string q, int orgId)
        {
            try
            {
                var interfaces = Repository.Get(
                    s =>
                        // filter by name
                        s.Name.Contains(q) &&
                        // global admin sees all within the context
                        (KitosUser.IsGlobalAdmin &&
                         s.OrganizationId == orgId ||
                         // object owner sees his own objects
                         s.ObjectOwnerId == KitosUser.Id ||
                         // it's public everyone can see it
                         s.AccessModifier == AccessModifier.Public ||
                         // everyone in the same organization can see normal objects
                         s.AccessModifier == AccessModifier.Local &&
                         s.OrganizationId == orgId)
                    // it systems doesn't have roles so private doesn't make sense
                    // only object owners will be albe to see private objects
                    );
                var dtos = Map(interfaces);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [DeprecatedApi]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceDTO>>))]
        public HttpResponseMessage GetCatalog(string q, int organizationId, [FromUri] PagingModel<ItInterface> pagingModel)
        {
            try
            {
                pagingModel.Where(s =>
                    // global admin sees all within the context
                    KitosUser.IsGlobalAdmin &&
                    s.OrganizationId == organizationId ||
                    // object owner sees his own objects
                    s.ObjectOwnerId == KitosUser.Id ||
                    // it's public everyone can see it
                    s.AccessModifier == AccessModifier.Public ||
                    // everyone in the same organization can see normal objects
                    s.AccessModifier == AccessModifier.Local &&
                    s.OrganizationId == organizationId
                    // it systems doesn't have roles so private doesn't make sense
                    // only object owners will be albe to see private objects
                    );

                if (!string.IsNullOrEmpty(q)) pagingModel.Where(s => s.Name.Contains(q));

                var interfaces = Page(Repository.AsQueryable(), pagingModel);

                var dtos = Map(interfaces);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public override HttpResponseMessage Post(ItInterfaceDTO dto)
        {
            try
            {
                if (!IsItInterfaceIdAndNameUnique(dto.ItInterfaceId, dto.Name, dto.OrganizationId))
                    return Conflict("ItInterface with same InterfaceId and Name is taken!");

                var item = Map(dto);

                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;
                item.ItInterfaceId = item.ItInterfaceId ?? "";
                item.Uuid = Guid.NewGuid();

                foreach (var dataRow in item.DataRows)
                {
                    dataRow.ObjectOwner = KitosUser;
                    dataRow.LastChangedByUser = KitosUser;
                }

                PostQuery(item);

                return Created(Map(item), new Uri(Request.RequestUri + "/" + item.Id));
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

            return base.Patch(id, organizationId, obj);
        }

        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "It Interface name must be new")]
        public HttpResponseMessage GetNameAvailable(string checkname, int orgId)
        {
            try
            {
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

        protected override bool HasWriteAccess(ItInterface obj, User user, int organizationId)
        {
            return HasWriteAccess();
        }

        protected bool HasWriteAccess()
        {
            return KitosUser.IsGlobalAdmin;
        }
    }
}
