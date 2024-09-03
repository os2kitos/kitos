using Core.ApplicationServices.Model.Organizations.Write;
using Presentation.Web.Models.API.V2.Request.OrganizationUnit;

namespace Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits.Mapping
{
    public interface IOrganizationUnitWriteModelMapper
    {
        OrganizationUnitUpdateParameters FromPOST(CreateOrganizationUnitRequestDTO request);
        OrganizationUnitUpdateParameters FromPATCH(UpdateOrganizationUnitRequestDTO request);
    }
}
