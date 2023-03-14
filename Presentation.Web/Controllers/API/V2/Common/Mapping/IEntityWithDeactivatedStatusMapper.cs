using Core.DomainModel;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using System.Collections.Generic;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface IEntityWithDeactivatedStatusMapper
    {
        IEnumerable<IdentityNamePairWithDeactivatedStatusDTO> Map<T>(IEnumerable<T> entities) where T : IHasUuid, IHasName, IEntityWithEnabledStatus;
    }
}
