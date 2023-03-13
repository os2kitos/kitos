using Core.DomainModel;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface IEntityWithDeactivatedStatusMapper
    {
        IEnumerable<IdentityNamePairWithDeactivatedStatusDTO> Map<T>(IEnumerable<T> systems) where T : IHasUuid, IHasName, IEntityWithEnabledStatus;
    }
}
