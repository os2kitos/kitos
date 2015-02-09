using System.Net.Http;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItProjectPhaseController : GenericApiController<ItProjectPhase, ItProjectPhaseDTO>
    {
        public ItProjectPhaseController(IGenericRepository<ItProjectPhase> repository) 
            : base(repository)
        {
        }

        //public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        //{
        //    // try get AccessModifier value
        //    JToken accessModToken;
        //    obj.TryGetValue("accessModifier", out accessModToken);
        //    // only global admin can set access mod to public
        //    if (accessModToken != null && accessModToken.ToObject<AccessModifier>() == AccessModifier.Public && !KitosUser.IsGlobalAdmin)
        //    {
        //        return Unauthorized();
        //    }
        //    return base.Patch(id, organizationId, obj);
        //}
    }
}
