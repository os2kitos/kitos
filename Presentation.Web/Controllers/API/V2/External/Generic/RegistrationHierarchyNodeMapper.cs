using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public class RegistrationHierarchyNodeMapper<TEntity> where TEntity : class, IHierarchy<TEntity>, IHasUuid, IHasName
    {
        public static IEnumerable<RegistrationHierarchyNodeResponseDTO> MapHierarchyToDtos(IEnumerable<TEntity> hierarchy)
        {
            return hierarchy
                .Select(x => new RegistrationHierarchyNodeResponseDTO
                {
                    Current = x.MapIdentityNamePairDTO(), 
                    Parent = x.Parent?.MapIdentityNamePairDTO()
                })
                .ToList();
        }
    }
}