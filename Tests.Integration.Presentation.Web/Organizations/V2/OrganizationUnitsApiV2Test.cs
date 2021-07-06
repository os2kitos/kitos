using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;
using Presentation.Web.Models;
using Presentation.Web.Models.External.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationUnitsApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task GET_OrganizationUnits()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var taskRef = DatabaseAccess.MapFromEntitySet<TaskRef, TaskRef>(refs => refs.AsQueryable().First());
            var rootUnitId = DatabaseAccess.MapFromEntitySet<OrganizationUnit, int>(units => units.AsQueryable().ByOrganizationId(organization.Id).First(x => x.ParentId == null).Id);
            var (_, token) = await CreateApiUser(organization);
            var unit1 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName());
            var unit1_1 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName(), unit1.Id);
            var unit2 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName());

            //Task ref must be set on the parent for the using to be available on children
            await TaskUsageHelper.CreateTaskUsageAsync(rootUnitId, taskRef.Id);
            await TaskUsageHelper.CreateTaskUsageAsync(unit2.Id, taskRef.Id);

            //Act
            var units = (await OrganizationUnitV2Helper.GetOrganizationUnitsAsync(token, organization.Uuid)).ToList();

            //Assert
            Assert.Equal(4, units.Count); //Organizational hierarchy always contains at least 1 (the root created with the organization and then comes the user defined units)
            var root = Assert.Single(units.Where(x => x.Name == organization.Name));
            Assert.Null(root.ParentOrganizationUnit);
            Assert.NotNull(root.Kle.SingleOrDefault(x => x.Uuid == taskRef.Uuid));
            AssertCreatedOrganizationUnit(units, unit1, (root.Uuid, root.Name));
            AssertCreatedOrganizationUnit(units, unit1_1, (unit1.Uuid, unit1.Name));
            AssertCreatedOrganizationUnit(units, unit2, (root.Uuid, root.Name), taskRef);
        }

        [Fact]
        public async Task GET_OrganizationUnits_With_NameFilter()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var rootUnitUnfo = DatabaseAccess.MapFromEntitySet<OrganizationUnit, (Guid, string)>(units => units.AsQueryable().ByOrganizationId(organization.Id).First(x => x.Parent == null).Transform(root => (root.Uuid, root.Name)));
            var (_, token) = await CreateApiUser(organization);
            var nameContent = $"_S_{A<uint>() % 10000}_E_";
            var matched1 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, $"{nameContent}{CreateName()}");
            await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName(), matched1.Id); //unmatched
            var matched2 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, $"{CreateName()}{nameContent}");

            //Act
            var units = (await OrganizationUnitV2Helper.GetOrganizationUnitsAsync(token, organization.Uuid, nameQuery: nameContent)).ToList();

            //Assert
            Assert.Equal(2, units.Count);
            AssertCreatedOrganizationUnit(units, matched1, rootUnitUnfo);
            AssertCreatedOrganizationUnit(units, matched2, rootUnitUnfo);
        }

        [Fact]
        public async Task Cannot_GET_OrganizationUnits_If_Not_Member_Of_Organization()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var (_, token) = await CreateApiUser(organization);
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            //Act
            using var response = await OrganizationUnitV2Helper.SendGetOrganizationUnitsAsync(token, defaultOrgUuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_OrganizationUnits_Of_Unknown_Organization()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var unknownOrg = A<Guid>();

            //Act
            using var response = await OrganizationUnitV2Helper.SendGetOrganizationUnitsAsync(token.Token, unknownOrg);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GET_OrganizationUnit()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var rootUnitUnfo = DatabaseAccess.MapFromEntitySet<OrganizationUnit, (Guid, string)>(units => units.AsQueryable().ByOrganizationId(organization.Id).First(x => x.Parent == null).Transform(root => (root.Uuid, root.Name)));
            var (_, token) = await CreateApiUser(organization);
            var unit = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, CreateName());

            //Act
            var dto = await OrganizationUnitV2Helper.GetOrganizationUnitAsync(token, organization.Uuid, unit.Uuid);

            //Assert
            AssertCreatedOrganizationUnit(dto, unit, rootUnitUnfo);
        }

        [Fact]
        public async Task Cannot_GET_OrganizationUnit_If_Not_Member_Of_Organization()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync();
            var organization2 = await CreateOrganizationAsync();
            var (_, token) = await CreateApiUser(organization2);
            var unit = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization1.Id, CreateName());

            //Act - try to get unit from organization where I have no access
            using var response = await OrganizationUnitV2Helper.SendGetOrganizationUnitAsync(token, organization1.Uuid, unit.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_OrganizationUnit_Of_Unknown_Organization()
        {
            //Arrange
            var token = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act - try to get unit from organization where I have no access
            using var response = await OrganizationUnitV2Helper.SendGetOrganizationUnitAsync(token.Token, Guid.NewGuid(), Guid.NewGuid());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_GET_OrganizationUnits_Of_Unknown_Unit()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var (_, token) = await CreateApiUser(organization);
            var unknownOrgUnit = A<Guid>();

            //Act
            using var response = await OrganizationUnitV2Helper.SendGetOrganizationUnitAsync(token, organization.Uuid, unknownOrgUnit);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<(User user, string token)> CreateApiUser(Organization organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private async Task<Organization> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            return await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, organizationName, null, A<OrganizationTypeKeys>(), AccessModifier.Public);
        }

        private string CreateName()
        {
            return $"{nameof(OrganizationUnitsApiV2Test)}{A<Guid>():N}";
        }

        private string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }

        private static void AssertCreatedOrganizationUnit(IEnumerable<OrganizationUnitResponseDTO> allUnits, OrgUnitDTO expectedUnit, (Guid Uuid, string Name) expectedRoot, params TaskRef[] kle)
        {
            var dto = Assert.Single(allUnits.Where(x => x.Uuid == expectedUnit.Uuid));
            AssertCreatedOrganizationUnit(dto, expectedUnit, expectedRoot, kle);
        }

        private static void AssertCreatedOrganizationUnit(OrganizationUnitResponseDTO dto, OrgUnitDTO expectedUnit, (Guid Uuid, string Name) expectedRoot, params TaskRef[] kle)
        {
            Assert.Equal(expectedRoot.Uuid, dto.ParentOrganizationUnit?.Uuid);
            Assert.Equal(expectedRoot.Name, dto.ParentOrganizationUnit?.Name);
            Assert.Equal(expectedUnit.Ean, dto.Ean);
            Assert.Equal(expectedUnit.LocalId, dto.UnitId);
            Assert.Equal(kle.Select(x => (x.Uuid, x.TaskKey)), dto.Kle.Select(x => (x.Uuid, x.Name)));
        }
    }
}
