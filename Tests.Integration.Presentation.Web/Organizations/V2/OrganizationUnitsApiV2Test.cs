using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
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
            var rootUnitId = DatabaseAccess.MapFromEntitySet<OrganizationUnit, int>(units => units.AsQueryable().ByOrganizationId(organization.Id).First(x=>x.ParentId == null).Id);
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
            Assert.NotNull(root.Kle.SingleOrDefault(x=>x.Uuid == taskRef.Uuid));
            AssertCreatedOrganizationUnit(units, unit1, (root.Uuid, root.Name));
            AssertCreatedOrganizationUnit(units, unit1_1, (unit1.Uuid, unit1.Name));
            AssertCreatedOrganizationUnit(units, unit2, (root.Uuid, root.Name), taskRef);
        }

        private void AssertCreatedOrganizationUnit(IEnumerable<OrganizationUnitResponseDTO> allUnits, OrgUnitDTO expectedUnit, (Guid Uuid, string Name) expectedRoot, params TaskRef[] kle)
        {
            var dto = Assert.Single(allUnits.Where(x => x.Uuid == expectedUnit.Uuid));
            Assert.Equal(expectedRoot.Uuid, dto.ParentOrganizationUnit?.Uuid);
            Assert.Equal(expectedRoot.Name, dto.ParentOrganizationUnit?.Name);
            Assert.Equal(expectedUnit.Ean, dto.Ean);
            Assert.Equal(expectedUnit.LocalId, dto.UnitId);
            Assert.Equal(kle.Select(x => (x.Uuid, x.TaskKey)), dto.Kle.Select(x => (x.Uuid, x.Name)));
        }

        [Fact]
        public async Task GET_OrganizationUnits_With_NameFilter()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task Cannot_GET_OrganizationUnits_If_Not_Member_Of_Organization()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public async Task Cannot_GET_OrganizationUnits_Of_Unknown_Organization()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public async Task GET_OrganizationUnit()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public async Task Cannot_GET_OrganizationUnit_If_Not_Member_Of_Organization()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public async Task Cannot_GET_OrganizationUnit_Of_Unknown_Organization()
        {

            throw new NotImplementedException();
        }

        [Fact]
        public async Task Cannot_GET_OrganizationUnits_Of_Unknown_Unit()
        {

            throw new NotImplementedException();
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
    }
}
