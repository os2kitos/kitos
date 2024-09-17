using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;


namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public class OrganizationMapper: IOrganizationMapper
    {
        public OrganizationResponseDTO ToDTO(Organization organization)
        {
            return new(organization.Uuid, organization.Name, organization.GetActiveCvr(), MapOrganizationType(organization));
        }

        private OrganizationType MapOrganizationType(Organization organization)
        {
            return organization.Type.Id switch
            {
                (int)OrganizationTypeKeys.Virksomhed => OrganizationType.Company,
                (int)OrganizationTypeKeys.Kommune => OrganizationType.Municipality,
                (int)OrganizationTypeKeys.AndenOffentligMyndighed => OrganizationType.OtherPublicAuthority,
                (int)OrganizationTypeKeys.Interessefællesskab => OrganizationType.CommunityOfInterest,
                _ => throw new ArgumentOutOfRangeException(nameof(organization.Type.Id), "Unknown organization type key")
            };
        }

        
    }
}