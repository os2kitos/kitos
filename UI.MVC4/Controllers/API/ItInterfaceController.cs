using System;
using System.Linq;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItInterfaceController : GenericApiController<ItInterface, ItInterfaceDTO>
    {
        public ItInterfaceController(IGenericRepository<ItInterface> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetSearch(string q, int orgId)
        {
            try
            {
                var interfaces = Repository.Get(
                    s =>
                        // filter by name
                        s.Name.Contains(q) &&
                        // global admin sees all within the context 
                        KitosUser.IsGlobalAdmin && s.OrganizationId == orgId ||
                        // object owner sees his own objects     
                        s.ObjectOwnerId == KitosUser.Id ||
                        // it's public everyone can see it
                        s.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        s.AccessModifier == AccessModifier.Normal &&
                        s.OrganizationId == orgId
                        // it systems doesn't have roles so private doesn't make sense
                        // only object owners will be albe to see private objects
                    );
                var dtos = Map(interfaces);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Get interfaces by name that aren't already used by the system in question
        /// </summary>
        /// <param name="q"></param>
        /// <param name="orgId"></param>
        /// <param name="sysId"></param>
        /// <returns>Available interfaces</returns>
        public HttpResponseMessage GetSearchExclude(string q, int orgId, int sysId)
        {
            try
            {
                var interfaces = Repository.Get(
                    s =>
                        // filter by name
                        s.Name.Contains(q) && 
                        // filter (remove) interfaces already used by the system
                        s.CanBeUsedBy.Count(x => x.ItSystemId == sysId) == 0 &&
                        // global admin sees all within the context 
                        KitosUser.IsGlobalAdmin && s.OrganizationId == orgId ||
                        // object owner sees his own objects     
                        s.ObjectOwnerId == KitosUser.Id ||
                        // it's public everyone can see it
                        s.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        s.AccessModifier == AccessModifier.Normal &&
                        s.OrganizationId == orgId
                        // it systems doesn't have roles so private doesn't make sense
                        // only object owners will be albe to see private objects
                    );
                var dtos = Map(interfaces);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public override HttpResponseMessage Post(ItInterfaceDTO dto)
        {
            try
            {
                var item = Map(dto);

                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;

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
                return Error(e);
            }
        }
    }
}
