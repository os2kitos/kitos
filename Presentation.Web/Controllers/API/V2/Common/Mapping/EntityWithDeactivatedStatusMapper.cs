using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public class EntityWithDeactivatedStatusMapper : IEntityWithDeactivatedStatusMapper
    {
        public IEnumerable<IdentityNamePairWithDeactivatedStatusDTO> Map<T>(IEnumerable<T> systems) where T : IHasUuid, IHasName, IEntityWithEnabledStatus
        {
            return systems.Select(x => x.MapIdentityNamePairWithDeactivatedStatusDTO()).ToList();
        }
    }
}