using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits.Mapping
{
    public interface IOrganizationUnitResponseModelMapper
    {
        OrganizationUnitResponseDTO ToUnitDto(OrganizationUnit unit);
    }
}