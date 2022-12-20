using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Users
{
    public class OrganizationRightApiTest : WithAutoFixture
    {
        private string CreateName() => $"{nameof(OrganizationRightApiTest)}{A<Guid>():N}";
        private string CreateEmail() => $"{A<Guid>():N}@kitos.dk";
        [Theory]
        [MemberData(nameof(GetCreationTestData))]
        public async Task Test_POST_OrganizationRight(OrganizationRole actorRole, OrganizationRole roleToAssign, bool expectSuccess)
        {
            //Arrange
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var (_, _, actorLoginCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), actorRole, CreateName(), CreateName(), organization.Id);
            var targetUser = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, CreateName(), CreateName()); //Create in the default org, so it is created before we try to assign it

            //Act
            using var response = await HttpApi.SendAssignRoleToUserAsync(targetUser.userId, roleToAssign, organization.Id, optionalLoginCookie: actorLoginCookie);

            //Assert
            Assert.Equal(expectSuccess, response.StatusCode == HttpStatusCode.Created);
        }

        public class OrganizationRightCreatedDTO
        {
            public int Id { get; set; }
        }

        [Theory]
        [MemberData(nameof(GetCreationTestData))]
        public async Task Test_DELETE_OrganizationRight(OrganizationRole actorRole, OrganizationRole roleToDelete, bool expectSuccess)
        {
            //Arrange
            var globalAdminCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, CreateName(), "", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            var (_, _, actorLoginCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), actorRole, CreateName(), CreateName(), organization.Id);
            var targetUser = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, CreateName(), CreateName()); //Create in the default org, so it is created before we try to assign it
            using var createResponse = await HttpApi.SendAssignRoleToUserAsync(targetUser.userId, roleToDelete, organization.Id, optionalLoginCookie: globalAdminCookie); //global admin assigns it so we can check if the actor can remove it
            var rightId = DatabaseAccess.MapFromEntitySet<OrganizationRight,int>(all=>all.AsQueryable().ByOrganizationId(organization.Id).First(x=>x.User.Id == targetUser.userId && x.Role == roleToDelete).Id);

            //Act
            using var response = await HttpApi.SendRemoveRoleToUserAsync(rightId, organization.Id, actorLoginCookie);

            //Assert
            Assert.Equal(expectSuccess ? HttpStatusCode.NoContent : HttpStatusCode.Forbidden, response.StatusCode);
        }

        public static IEnumerable<object[]> GetCreationTestData()
        {
            //LOCAL ADMIN
            yield return new object[] { OrganizationRole.LocalAdmin, OrganizationRole.User, true };
            yield return new object[] { OrganizationRole.LocalAdmin, OrganizationRole.SystemModuleAdmin, true };
            yield return new object[] { OrganizationRole.LocalAdmin, OrganizationRole.OrganizationModuleAdmin, true };
            yield return new object[] { OrganizationRole.LocalAdmin, OrganizationRole.ContractModuleAdmin, true };
            yield return new object[] { OrganizationRole.LocalAdmin, OrganizationRole.LocalAdmin, true };

            //SYSTEM ADMIN
            yield return new object[] { OrganizationRole.SystemModuleAdmin, OrganizationRole.User, false };
            yield return new object[] { OrganizationRole.SystemModuleAdmin, OrganizationRole.SystemModuleAdmin, false };
            yield return new object[] { OrganizationRole.SystemModuleAdmin, OrganizationRole.OrganizationModuleAdmin, false };
            yield return new object[] { OrganizationRole.SystemModuleAdmin, OrganizationRole.ContractModuleAdmin, false };
            yield return new object[] { OrganizationRole.SystemModuleAdmin, OrganizationRole.LocalAdmin, false };

            //CONTRACT ADMIN
            yield return new object[] { OrganizationRole.ContractModuleAdmin, OrganizationRole.User, false };
            yield return new object[] { OrganizationRole.ContractModuleAdmin, OrganizationRole.SystemModuleAdmin, false };
            yield return new object[] { OrganizationRole.ContractModuleAdmin, OrganizationRole.OrganizationModuleAdmin, false };
            yield return new object[] { OrganizationRole.ContractModuleAdmin, OrganizationRole.ContractModuleAdmin, false };
            yield return new object[] { OrganizationRole.ContractModuleAdmin, OrganizationRole.LocalAdmin, false };

            //ORG ADMIN
            yield return new object[] { OrganizationRole.OrganizationModuleAdmin, OrganizationRole.User, true };
            yield return new object[] { OrganizationRole.OrganizationModuleAdmin, OrganizationRole.SystemModuleAdmin, false };
            yield return new object[] { OrganizationRole.OrganizationModuleAdmin, OrganizationRole.OrganizationModuleAdmin, true };
            yield return new object[] { OrganizationRole.OrganizationModuleAdmin, OrganizationRole.ContractModuleAdmin, false };
            yield return new object[] { OrganizationRole.OrganizationModuleAdmin, OrganizationRole.LocalAdmin, false };

            //REGULAR
            yield return new object[] { OrganizationRole.User, OrganizationRole.User, false };
            yield return new object[] { OrganizationRole.User, OrganizationRole.SystemModuleAdmin, false };
            yield return new object[] { OrganizationRole.User, OrganizationRole.OrganizationModuleAdmin, false };
            yield return new object[] { OrganizationRole.User, OrganizationRole.ContractModuleAdmin, false };
            yield return new object[] { OrganizationRole.User, OrganizationRole.LocalAdmin, false };
        }

    }
}
