using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceDTO>>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
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

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItInterfaceDTO>>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
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

        public HttpResponseMessage GetExcel(bool? csv, int organizationId)
        {
            try
            {
                var interfaces = Repository.Get(
                    x =>
                        // global admin sees all within the context
                        KitosUser.IsGlobalAdmin &&
                        x.OrganizationId == organizationId ||
                        // object owner sees his own objects
                        x.ObjectOwnerId == KitosUser.Id ||
                        // it's public everyone can see it
                        x.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        x.AccessModifier == AccessModifier.Local &&
                        x.OrganizationId == organizationId
                    // it systems doesn't have roles so private doesn't make sense
                    // only object owners will be albe to see private objects
                    );
                var dtos = Map(interfaces);

                var list = new List<dynamic>();
                var header = new ExpandoObject() as IDictionary<string, Object>;
                header.Add("Snitflade", "Snitflade");
                header.Add("Public", "(P)");
                header.Add("Snitfladetype", "Snitfladetype");
                header.Add("Interface", "Grænseflade");
                header.Add("Metode", "Metode");
                header.Add("TSA", "TSA");
                header.Add("Udstillet af", "Udstillet af");
                header.Add("Rettighedshaver", "Rettighedshaver");
                header.Add("Oprettet af", "Oprettet af");
                list.Add(header);
                foreach (var itInterface in dtos)
                {
                    var obj = new ExpandoObject() as IDictionary<string, Object>;
                    obj.Add("Snitflade", itInterface.Name);
                    obj.Add("Public", itInterface.AccessModifier == AccessModifier.Public ? "(P)" : "");
                    obj.Add("Snitfladetype", itInterface.InterfaceTypeName);
                    obj.Add("Interface", itInterface.InterfaceName);
                    obj.Add("Metode", itInterface.MethodName);
                    obj.Add("TSA", itInterface.TsaName);
                    obj.Add("Udstillet af", itInterface.ExhibitedByItSystemName);
                    obj.Add("Rettighedshaver", itInterface.BelongsToName);
                    obj.Add("Oprettet af", itInterface.OrganizationName);
                    list.Add(obj);
                }
                var s = list.ToCsv();
                var bytes = Encoding.Unicode.GetBytes(s);
                var stream = new MemoryStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);

                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileNameStar = "snitfladekatalog.csv", DispositionType = "ISO-8859-1" };
                return result;
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Get interfaces by name that aren't already used by the system in question
        /// </summary>
        /// <param name="q"></param>
        /// <param name="orgId"></param>
        /// <param name="sysId"></param>
        /// <returns>Available interfaces</returns>
        //
        // Udkommenteret ifm. OS2KITOS-663
        //public HttpResponseMessage GetSearchExclude(string q, int orgId, int sysId)
        //{
        //    try
        //    {
        //        var interfaces = Repository.Get(
        //            s =>
        //                // filter by name
        //                s.Name.Contains(q) &&
        //                // filter (remove) interfaces already used by the system
        //                s.CanBeUsedBy.Count(x => x.ItSystemId == sysId) == 0 &&
        //                // global admin sees all within the context
        //                (KitosUser.IsGlobalAdmin &&
        //                 s.OrganizationId == orgId ||
        //                 // object owner sees his own objects
        //                 s.ObjectOwnerId == KitosUser.Id ||
        //                 // it's public everyone can see it
        //                 s.AccessModifier == AccessModifier.Public ||
        //                 // everyone in the same organization can see normal objects
        //                 s.AccessModifier == AccessModifier.Local &&
        //                 s.OrganizationId == orgId)
        //                // it systems doesn't have roles so private doesn't make sense
        //                // only object owners will be albe to see private objects
        //            );
        //        var dtos = Map(interfaces);
        //        return Ok(dtos);
        //    }
        //    catch (Exception e)
        //    {
        //        return LogError(e);
        //    }
        //}

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
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
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
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
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
