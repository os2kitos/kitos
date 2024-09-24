using Core.DomainModel.Organization;
using System;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public class OrganizationTypeMapper : IOrganizationTypeMapper
{
    public OrganizationType MapOrganizationType(Core.DomainModel.Organization.OrganizationType type)
    {
        var id = type.Id;
        return id switch
        {
            (int)OrganizationTypeKeys.Virksomhed => OrganizationType.Company,
            (int)OrganizationTypeKeys.Kommune => OrganizationType.Municipality,
            (int)OrganizationTypeKeys.AndenOffentligMyndighed => OrganizationType.OtherPublicAuthority,
            (int)OrganizationTypeKeys.Interessefællesskab => OrganizationType.CommunityOfInterest,
            _ => throw new ArgumentOutOfRangeException(id.ToString(),
                "Unknown organization type key")
        };
    }
}