using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Request.OrganizationUnit;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Organization;
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
            var (_, token) = await CreateApiUser(organization);
            var unit1 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName());
            var unit1_1 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName(), unit1.Id);
            var unit2 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName());

            //Act
            var units = (await OrganizationUnitV2Helper.GetOrganizationUnitsAsync(token, organization.Uuid)).ToList();

            //Assert
            Assert.Equal(4, units.Count); //Organizational hierarchy always contains at least 1 (the root created with the organization and then comes the user defined units)
            var root = Assert.Single(units.Where(x => x.Name == organization.Name));
            Assert.Null(root.ParentOrganizationUnit);
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
            var matched1 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, $"{nameContent}{CreateName()}");
            await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName(), matched1.Id); //unmatched
            var matched2 = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, $"{CreateName()}{nameContent}");

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
            var unit = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, CreateName());

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
            var unit = await OrganizationHelper.CreateOrganizationUnitAsync(organization1.Id, CreateName());

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

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, true, true, true, true, true, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, true, true, true, true, true, true, true, true)]
        [InlineData(OrganizationRole.User, true, false, false, false, false, false, false, false)]
        public async Task Can_Get_Specific_Organization_Unit_Permissions(OrganizationRole role, bool canBeRead, bool canBeModified, bool canBeRenamed, bool canEanBeModified, bool canDeviceIdBeModified, bool canBeRearranged, bool canBeDeleted, bool canEditRegistrations)
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var email = CreateEmail();
            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(email, role, organization.Id);
            var unit = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, A<string>());

            //Act
            var accessRights = await OrganizationV2Helper.GetUnitAccessRights(organization.Uuid, unit.Uuid, cookie);

            //Assert
            Assert.NotNull(accessRights);

            Assert.Equal(canBeRead, accessRights.CanBeRead);
            Assert.Equal(canBeModified, accessRights.CanBeModified);
            Assert.Equal(canBeRenamed, accessRights.CanBeRenamed);
            Assert.Equal(canEanBeModified, accessRights.CanEanBeModified);
            Assert.Equal(canDeviceIdBeModified, accessRights.CanDeviceIdBeModified);
            Assert.Equal(canBeRearranged, accessRights.CanBeRearranged);
            Assert.Equal(canBeDeleted, accessRights.CanBeDeleted);
            Assert.Equal(canEditRegistrations, accessRights.CanEditRegistrations);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, true, true, true, true, true, true, true)]
        [InlineData(OrganizationRole.LocalAdmin, true, true, true, true, true, true, true, true)]
        [InlineData(OrganizationRole.User, true, false, false, false, false, false, false, false)]
        public async Task Can_Get_All_Organization_Unit_Permissions(OrganizationRole role, bool canBeRead, bool canBeModified, bool canBeRenamed, bool canEanBeModified, bool canDeviceIdBeModified, bool canBeRearranged, bool canBeDeleted, bool canEditRegistrations)
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var email = CreateEmail();
            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(email, role, organization.Id);
            var unit = await OrganizationHelper.CreateOrganizationUnitAsync(organization.Id, A<string>());

            //Act
            var accessRightsList = await OrganizationV2Helper.GetUnitAccessRightsForOrganization(organization.Uuid, cookie);

            //Assert
            var accessRightsWithUnit = accessRightsList.FirstOrDefault(x => x.OrganizationUnit.Uuid == unit.Uuid);
            Assert.NotNull(accessRightsWithUnit);
            var accessRights = accessRightsWithUnit.UnitAccessRights;

            Assert.Equal(canBeRead, accessRights.CanBeRead);
            Assert.Equal(canBeModified, accessRights.CanBeModified);
            Assert.Equal(canBeRenamed, accessRights.CanBeRenamed);
            Assert.Equal(canEanBeModified, accessRights.CanEanBeModified);
            Assert.Equal(canDeviceIdBeModified, accessRights.CanDeviceIdBeModified);
            Assert.Equal(canBeRearranged, accessRights.CanBeRearranged);
            Assert.Equal(canBeDeleted, accessRights.CanBeDeleted);
            Assert.Equal(canEditRegistrations, accessRights.CanEditRegistrations);
        }

        [Fact]
        public async Task Can_Create_OrganizationUnit()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            var units = await OrganizationUnitV2Helper.GetOrganizationUnitsAsync(token.Token, organization.Uuid);
            var parentUnit = Assert.Single(units);

            var request = CreateCreateRequest(parentUnit.Uuid);

            //Act
            var result = await OrganizationUnitV2Helper.CreateUnitAsync(organization.Uuid, request);

            Assert.Equal(request.Name, result.Name);
            Assert.Equal(parentUnit.Name, result.ParentOrganizationUnit.Name);
            Assert.Equal(parentUnit.Uuid, result.ParentOrganizationUnit.Uuid);
            Assert.Equal(parentUnit.Origin, result.Origin);
        }

        [Fact]
        public async Task Can_Update_OrganizationUnit()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            var units = await OrganizationUnitV2Helper.GetOrganizationUnitsAsync(token.Token, organization.Uuid);
            var parentUnit = Assert.Single(units);

            var request1 = CreateCreateRequest(parentUnit.Uuid);
            var request2 = CreateCreateRequest(parentUnit.Uuid);

            var testUnitFutureParent = await OrganizationUnitV2Helper.CreateUnitAsync(organization.Uuid, request1);
            var testUnitFutureChild = await OrganizationUnitV2Helper.CreateUnitAsync(organization.Uuid, request2);

            var patchRequest = new UpdateOrganizationUnitRequestDTO
            {
                Name = A<string>(),
                Origin = testUnitFutureChild.Origin == OrganizationUnitOriginChoice.Kitos
                    ? OrganizationUnitOriginChoice.STSOrganisation
                    : OrganizationUnitOriginChoice.Kitos,
                ParentUuid = testUnitFutureParent.Uuid
            };

            //Act
            var result =
                await OrganizationUnitV2Helper.PatchUnitAsync(organization.Uuid, testUnitFutureChild.Uuid,
                    patchRequest);

            //Assert
            Assert.Equal(patchRequest.Name, result.Name);
            Assert.Equal(patchRequest.Origin, result.Origin);
            Assert.Equal(patchRequest.ParentUuid, result.ParentOrganizationUnit.Uuid);
        }

        [Fact]
        public async Task Can_Delete_OrganizationUnit()
        {
            //Arrange
            var organization = await CreateOrganizationAsync();
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            var units = await OrganizationUnitV2Helper.GetOrganizationUnitsAsync(token.Token, organization.Uuid);
            var parentUnit = Assert.Single(units);

            var createRequest = CreateCreateRequest(parentUnit.Uuid);

            var testUnit = await OrganizationUnitV2Helper.CreateUnitAsync(organization.Uuid, createRequest);

            //Act
            var deleteResult = await OrganizationUnitV2Helper.DeleteUnitAsync(organization.Uuid, testUnit.Uuid);

            //Assert
            Assert.Equal(HttpStatusCode.OK, deleteResult.StatusCode);

        }

        private async Task<(User user, string token)> CreateApiUser(OrganizationDTO organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private async Task<OrganizationDTO> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            return await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, organizationName, null, A<OrganizationTypeKeys>(), AccessModifier.Public);
        }

        private CreateOrganizationUnitRequestDTO CreateCreateRequest(Guid parentUuid)
        {
            return new CreateOrganizationUnitRequestDTO
            {
                Name = A<string>(),
                Origin = A<OrganizationUnitOriginChoice>(),
                ParentUuid = parentUuid
            };
        }

        private string CreateName()
        {
            return $"{nameof(OrganizationUnitsApiV2Test)}æøå{A<Guid>():N}";
        }

        private string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }

        private static void AssertCreatedOrganizationUnit(IEnumerable<OrganizationUnitResponseDTO> allUnits, OrgUnitDTO expectedUnit, (Guid Uuid, string Name) expectedRoot, params TaskRef[] kle)
        {
            var dto = Assert.Single(allUnits.Where(x => x.Uuid == expectedUnit.Uuid));
            AssertCreatedOrganizationUnit(dto, expectedUnit, expectedRoot);
        }

        private static void AssertCreatedOrganizationUnit(OrganizationUnitResponseDTO dto, OrgUnitDTO expectedUnit, (Guid Uuid, string Name) expectedRoot)
        {
            Assert.Equal(expectedRoot.Uuid, dto.ParentOrganizationUnit?.Uuid);
            Assert.Equal(expectedRoot.Name, dto.ParentOrganizationUnit?.Name);
            Assert.Equal(expectedUnit.Ean, dto.Ean);
            Assert.Equal(expectedUnit.LocalId, dto.UnitId);
        }
    }
}
