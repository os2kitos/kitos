using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ReferenceController : GenericApiController<ExternalReference, ExternalReferenceDTO>
    {
        public ReferenceController(IGenericRepository<ExternalReference> repository)
            : base(repository)
        {
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            var reference = Repository.GetByKey(id);

            if (!CanModifyReference(reference))
            {
                return Forbidden();
            }

            var result = base.PatchQuery(reference, obj);

            return Ok(Map(result));

        }

        private bool CanModifyReference(ExternalReference entity)
        {
            if (entity.ObjectOwnerId == KitosUser.Id)
            {
                return true;
            }

            if (entity.ItContract != null && AllowModify(entity.ItContract))
            {
                return true;
            }

            if (entity.ItProject != null && AllowModify(entity.ItProject))
            {
                return true;
            }

            if (entity.ItSystem != null && AllowModify(entity.ItSystem))
            {
                return true;
            }

            if (entity.ItSystemUsage != null && AllowModify(entity.ItSystemUsage))
            {
                return true;
            }

            return false;
        }
    }
}