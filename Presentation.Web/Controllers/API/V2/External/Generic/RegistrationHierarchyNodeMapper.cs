using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;
using Presentation.Web.Models.API.V2.Response.System;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public class RegistrationHierarchyNodeMapper 
    {
        public static IEnumerable<ItSystemHierarchyNodeResponseDTO> MapSystemHierarchyToDtos(IEnumerable<ItSystem> hierarchy)
        {
            return hierarchy
                .Select(x => new ItSystemHierarchyNodeResponseDTO
                {
                    Node = x.MapIdentityNamePairDTO(), 
                    Parent = x.Parent?.MapIdentityNamePairDTO(),
                    Deactivated = x.Disabled,
                    IsInUse = x.Usages.Any()
                })
                .ToList();
        }

        public static IEnumerable<RegistrationHierarchyNodeWithActivationStatusResponseDTO> MapContractHierarchyToDtos(IEnumerable<ItContract> hierarchy)
        {
            return hierarchy
                .Select(x => new RegistrationHierarchyNodeWithActivationStatusResponseDTO
                {
                    Node = x.MapIdentityNamePairDTO(),
                    Parent = x.Parent?.MapIdentityNamePairDTO(),
                    Deactivated = x.IsActive == false
                })
                .ToList();
        }
    }
}