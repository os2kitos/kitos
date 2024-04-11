using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public class RegistrationHierarchyNodeMapper 
    {
        public static IEnumerable<RegistrationHierarchyNodeWithActivationStatusResponseDTO> MapHierarchyToDtosWithDisabledStatus<TEntity>(IEnumerable<TEntity> hierarchy) where TEntity : class, IHierarchy<TEntity>, IHasUuid, IHasName, IEntityWithEnabledStatus
        {
            return hierarchy
                .Select(x => new RegistrationHierarchyNodeWithActivationStatusResponseDTO
                {
                    Node = x.MapIdentityNamePairDTO(), 
                    Parent = x.Parent?.MapIdentityNamePairDTO(),
                    Deactivated = x.Disabled
                })
                .ToList();
        }

        public static IEnumerable<RegistrationHierarchyNodeWithActivationStatusResponseDTO> MapHierarchyToDtos<TEntity>(IEnumerable<TEntity> hierarchy) where TEntity : class, IHierarchy<TEntity>, IHasUuid, IHasName
        {
            return hierarchy
                .Select(x => new RegistrationHierarchyNodeWithActivationStatusResponseDTO
                {
                    Node = x.MapIdentityNamePairDTO(),
                    Parent = x.Parent?.MapIdentityNamePairDTO(),
                })
                .ToList();
        }
    }
}