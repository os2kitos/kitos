using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public class RegistrationHierarchyNodeMapper 
    {
        public static IEnumerable<RegistrationHierarchyNodeWithDisabledStatusResponseDTO> MapHierarchyToDtosWithDisabledStatus<TEntity>(IEnumerable<TEntity> hierarchy) where TEntity : class, IHierarchy<TEntity>, IHasUuid, IHasName, IEntityWithEnabledStatus
        {
            return hierarchy
                .Select(x => new RegistrationHierarchyNodeWithDisabledStatusResponseDTO
                {
                    Node = x.MapIdentityNamePairDTO(), 
                    Parent = x.Parent?.MapIdentityNamePairDTO(),
                    Deactivated = x.Disabled
                })
                .ToList();
        }
    }
}