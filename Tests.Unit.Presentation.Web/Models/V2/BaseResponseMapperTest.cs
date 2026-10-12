using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainModel;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class BaseResponseMapperTest : WithAutoFixture
    {
        [Fact]
        public void MapShallowOrganizationResponseWithDisabledStateDTO_Maps_Disabled_State()
        {
            var organization = new Organization
            {
                Uuid = A<Guid>(),
                Name = A<string>(),
                Cvr = A<string>(),
                Disabled = true
            };

            var dto = organization.MapShallowOrganizationResponseWithDisabledStateDTO();

            Assert.Equal(organization.Uuid, dto.Uuid);
            Assert.Equal(organization.Name, dto.Name);
            Assert.Equal(organization.Cvr, dto.Cvr);
            Assert.True(dto.Disabled);
        }

        protected static void AssertOrganization(Organization organization, ShallowOrganizationResponseDTO? shallowOrganizationDTO)
        {
            AssertIdentity(organization, shallowOrganizationDTO);
            Assert.Equal(organization.Cvr, shallowOrganizationDTO?.Cvr);
        }

        protected static void AssertOptionalOrganization(Organization? organization, ShallowOrganizationResponseDTO? shallowOrganizationDTO)
        {
            if (organization == null)
                Assert.Null(shallowOrganizationDTO);
            else
                AssertOrganization(organization, shallowOrganizationDTO);
        }

        protected static void AssertOptionalIdentity<T>(T? optionalExpectedIdentity, IdentityNamePairResponseDTO? actualIdentity) where T : IHasUuid, IHasName
        {
            if (optionalExpectedIdentity == null)
                Assert.Null(actualIdentity);
            else
                AssertIdentity(optionalExpectedIdentity, actualIdentity);
        }

        protected static void AssertIdentity<T>(T? sourceIdentity, IdentityNamePairResponseDTO? dto) where T : IHasUuid, IHasName
        {
            if (sourceIdentity == null)
            {
                Assert.Null(dto);
            }
            else
            {
                Assert.Equal(sourceIdentity.Name, dto?.Name);
                Assert.Equal(sourceIdentity.Uuid, dto?.Uuid);
            }
        }

        protected static void AssertUser(User user, IdentityNamePairResponseDTO? dtoValue)
        {
            Assert.Equal((user.GetFullName(), user.Uuid), (dtoValue?.Name, dtoValue?.Uuid));
        }

        protected static void AssertOptionalUser(User? user, IdentityNamePairResponseDTO? dtoValue)
        {
            if (user == null)
                Assert.Null(dtoValue);
            else
                AssertUser(user, dtoValue);
        }

        protected static void AssertIdentities<T>(IEnumerable<T> sourceIdentities, IEnumerable<IdentityNamePairResponseDTO> dto) where T : IHasUuid, IHasName
        {
            var orderedOptionalExpectedIdentities = sourceIdentities.OrderBy(x => x.Uuid).ToList();
            var orderedActualIdentities = dto.OrderBy(x => x.Uuid).ToList();

            Assert.Equal(orderedOptionalExpectedIdentities.Count, orderedActualIdentities.Count);

            foreach (var comparison in orderedOptionalExpectedIdentities
                         .Zip(orderedActualIdentities, (expected, actual) => new { expected, actual })
                         .ToList())
            {
                AssertIdentity(comparison.expected, comparison.actual);
            }
        }

        protected static void AssertOptionalIdentities<T>(IEnumerable<T>? optionalExpectedIdentities, IEnumerable<IdentityNamePairResponseDTO> actualIdentities) where T : IHasUuid, IHasName
        {
            if (optionalExpectedIdentities == null)
            {
                Assert.Null(actualIdentities);
            }
            else
            {
                var orderedOptionalExpectedIdentities = optionalExpectedIdentities.OrderBy(x => x.Uuid).ToList();
                var orderedActualIdentities = actualIdentities.OrderBy(x => x.Uuid).ToList();

                Assert.Equal(orderedOptionalExpectedIdentities.Count, orderedActualIdentities.Count);

                foreach (var comparison in orderedOptionalExpectedIdentities
                             .Zip(orderedActualIdentities, (expected, actual) => new { expected, actual })
                             .ToList())
                {
                    AssertOptionalIdentity(comparison.expected, comparison.actual);
                }
            }
        }
    }
}
