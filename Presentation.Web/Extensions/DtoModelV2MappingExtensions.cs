using System;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Extensions
{
    public static class DtoModelV2MappingExtensions
    {
        public static IdentityNamePairResponseDTO MapIdentityNamePairDTO<T>(this T source) where T : IHasUuid, IHasName
        {
            return new(source.Uuid, source.Name);
        }

        public static IdentityNamePairResponseDTO MapIdentityNamePairDTO(this TaskRef source)
        {
            return new(source.Uuid, source.TaskKey);
        }

        public static IdentityNamePairResponseDTO MapIdentityNamePairDTO(this User source)
        {
            return new(source.Uuid, source.GetFullName());
        }

        public static ShallowOrganizationResponseDTO MapShallowOrganizationResponseDTO(this Organization organization)
        {
            return new(organization.Uuid, organization.Name, organization.GetActiveCvr());
        }

        public static RecommendedArchiveDutyChoice ToDTOType(this ArchiveDutyRecommendationTypes? domainType)
        {
            switch (domainType)
            {
                case null:
                case ArchiveDutyRecommendationTypes.Undecided:
                    return RecommendedArchiveDutyChoice.Undecided;
                case ArchiveDutyRecommendationTypes.B:
                    return RecommendedArchiveDutyChoice.B;
                case ArchiveDutyRecommendationTypes.K:
                    return RecommendedArchiveDutyChoice.K;
                case ArchiveDutyRecommendationTypes.NoRecommendation:
                    return RecommendedArchiveDutyChoice.NoRecommendation;
                default:
                    throw new ArgumentOutOfRangeException(nameof(domainType), domainType, null);
            }
        }
    }
}