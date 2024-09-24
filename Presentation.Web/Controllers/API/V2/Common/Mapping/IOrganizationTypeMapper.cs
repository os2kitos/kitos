using Presentation.Web.Models.API.V2.Types.Organization;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface IOrganizationTypeMapper
    {
        OrganizationType MapOrganizationType(Core.DomainModel.Organization.OrganizationType type);
    }
}