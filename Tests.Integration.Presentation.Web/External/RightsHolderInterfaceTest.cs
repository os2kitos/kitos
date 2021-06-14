using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Infrastructure.Services.Cryptography;
using Presentation.Web.Models;
using Presentation.Web.Models.External.V2.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.External
{
    public class RightsHolderInterfaceTest : WithAutoFixture
    {
        private readonly CryptoService _cryptoService = new();


        [Fact]
        public async Task Can_Get_Interfaces()
        {
            //Arrange
            var rightsholderOrg = new Organization()
            {
                Name = A<string>(),
                ObjectOwnerId = TestEnvironment.DefaultUserId,
                LastChangedByUserId = TestEnvironment.DefaultUserId,
                TypeId = TestEnvironment.DefaultOrganizationTypeId
            };
            DatabaseAccess.MutateEntitySet<Organization>(x => x.Insert(rightsholderOrg));
            var rightsHolderOrgId = DatabaseAccess.MapFromEntitySet<Organization, int>(x => x.AsQueryable().Where(x => x.Name == rightsholderOrg.Name).Select(x => x.Id).FirstOrDefault());

            var email = $"{A<string>()}@test.dk";
            var password = A<string>();
            var salt = A<string>();
            var encryptedPwd = _cryptoService.Encrypt(password + salt);
            var roles = new List<OrganizationRole>();
            roles.Add(OrganizationRole.RightsHolderAccess);
            roles.Add(OrganizationRole.User);

            var user = await UserHelper.CreateUserWithRoles(email, password, salt, encryptedPwd, rightsHolderOrgId, roles.ToArray());

            var loginUser = new LoginDTO()
            {
                Email = user.Email,
                Password = password
            };
            var pageSize = 100; //Get as many as we can
            var pageNumber = 0; //Always takes the first page;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), rightsHolderOrgId, AccessModifier.Public);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), rightsHolderOrgId, AccessModifier.Public));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id);

            //Act
            var result = await InterfaceV2Helper.GetRightsholderInterfacesAsync(loginUser, pageSize, pageNumber);

            //Assert
            var interfaceDTO = Assert.Single(result.Where(x => x.Name.Equals(itInterface.Name)));
            CheckDTOValues(system, itInterface, interfaceDTO);
        }

        

        [Fact]
        public async Task Can_Get_Interface()
        {
            //Arrange
            var email = $"{A<string>()}@test.dk";
            var password = A<string>();
            var salt = A<string>();
            var encryptedPwd = _cryptoService.Encrypt(password + salt);
            var roles = new List<OrganizationRole>();
            roles.Add(OrganizationRole.RightsHolderAccess);
            roles.Add(OrganizationRole.User);

            var user = await UserHelper.CreateUserWithRoles(email, password, salt, encryptedPwd, TestEnvironment.DefaultOrganizationId, roles.ToArray());

            var loginUser = new LoginDTO()
            {
                Email = user.Email,
                Password = password
            };

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var itInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(A<string>(), A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public));
            await InterfaceExhibitHelper.SendCreateExhibitRequest(system.Id, itInterface.Id);

            //Act
            var result = await InterfaceV2Helper.GetRightsholderInterfaceAsync(loginUser, itInterface.Uuid);

            //Assert
            CheckDTOValues(system, itInterface, result);
        }

        private static void CheckDTOValues(ItSystemDTO system, ItInterfaceDTO itInterface, ItInterfaceResponseDTO interfaceDTO)
        {
            Assert.Equal(itInterface.Name, interfaceDTO.Name);
            Assert.Equal(system.Name, interfaceDTO.ExposedBySystem.Name);
            Assert.Equal(system.Uuid, interfaceDTO.ExposedBySystem.Uuid);
            Assert.Equal(itInterface.Uuid, interfaceDTO.Uuid);
            Assert.Equal(itInterface.Description, interfaceDTO.Description);
            Assert.Equal(itInterface.ItInterfaceId, interfaceDTO.InterfaceId);
            Assert.Equal(itInterface.Url, interfaceDTO.UrlReference);
            Assert.Equal(itInterface.Version, interfaceDTO.Version);
        }
    }
}
