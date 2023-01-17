using System;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Users
{
    public class UserPermissionsAPITest : WithAutoFixture
    {
        private string CreateName() => $"{nameof(UserPermissionsAPITest)}{A<Guid>():N}";
        private string CreateEmail() => $"{A<Guid>():N}@kitos.dk";

        [Theory]
        [InlineData(OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.ContractModuleAdmin, false)]
        [InlineData(OrganizationRole.SystemModuleAdmin, false)]
        [InlineData(OrganizationRole.OrganizationModuleAdmin, false)]
        [InlineData(OrganizationRole.User, false)]
        public async Task Can_Get_Permissions(OrganizationRole role, bool expectCanDelete)
        {
            //Arrange
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var (_, _, loginCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), role, CreateName(), CreateName(), organization.Id);

            //Act
            var permissionsDto = await UserHelper.GetUserAdministrationPermissionsAsync(organization.Uuid, loginCookie);

            //Assert
            Assert.Equal(expectCanDelete, permissionsDto.AllowDelete);
        }
    }
}
