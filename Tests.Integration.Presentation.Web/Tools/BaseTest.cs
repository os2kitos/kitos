using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.User;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Request.Interface;
using Presentation.Web.Models.API.V2.Request.OrganizationUnit;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Request.User;
using Presentation.Web.Models.API.V2.Response.Contract;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Response.Interface;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.System;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Integration.Presentation.Web.Tools.Internal.Organizations;
using Tests.Integration.Presentation.Web.Tools.Internal.Users;
using Tests.Toolkit.Patterns;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class BaseTest : WithAutoFixture
    {
        private string _token;
        private readonly Random _random = new Random();

        public readonly Guid DefaultOrgUuid =
            DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

        public readonly Guid SecondOrgUuid =
            DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.SecondOrganizationId);

        public async Task<ShallowOrganizationResponseDTO> CreateOrganizationAsync(string name = null, string cvr = null,
            OrganizationType type = OrganizationType.Municipality)
        {
            var defaultRequest = new OrganizationCreateRequestDTO
            {
                Name = name ?? A<string>(),
                Cvr = cvr ?? CreateCvr(),
                Type = type
            };
            using var response = OrganizationInternalV2Helper.CreateOrganization(defaultRequest);
            return await response;
        }

        public async Task<DataProcessingRegistrationResponseDTO> CreateDPRAsync(Guid orgUuid, string name = null)
        {
            var token = await GetGlobalToken();
            var request = new CreateDataProcessingRegistrationRequestDTO
            {
                Name = name ?? A<string>(),
                OrganizationUuid = orgUuid,
            };
            return await DataProcessingRegistrationV2Helper.PostAsync(token, request);
        }

        public async Task<ItContractResponseDTO> CreateItContractAsync(Guid organizationUuid, string name = null)
        {
            return await ItContractV2Helper.PostContractAsync(await GetGlobalToken(), new CreateNewContractRequestDTO
            {
                OrganizationUuid = organizationUuid,
                Name = name ?? A<string>()
            });
        }

        public async Task<ItSystemResponseDTO> CreateItSystemAsync(Guid orgGuid, string name = null,
            RegistrationScopeChoice scope = RegistrationScopeChoice.Global, Guid? rightsHolderUuid = null)
        {
            var request = new CreateItSystemRequestDTO
            {
                Name = name ?? A<string>(),
                OrganizationUuid = orgGuid,
                Scope = scope,
                RightsHolderUuid = rightsHolderUuid
            };
            return await ItSystemV2Helper.CreateSystemAsync(await GetGlobalToken(), request);
        }

        public async Task<ItSystemUsageResponseDTO> TakeSystemIntoUsageAsync(Guid systemUuid, Guid orgUuid)
        {
            var request = new CreateItSystemUsageRequestDTO
            {
                SystemUuid = systemUuid,
                OrganizationUuid = orgUuid
            };
            return await ItSystemUsageV2Helper.PostAsync(await GetGlobalToken(), request);
        }

        public async Task TakeMultipleSystemsIntoUsageAsync(Guid systemUuid, params Guid[] organizationUuids)
        {
            foreach (var orgUuid in organizationUuids)
            {
                await TakeSystemIntoUsageAsync(systemUuid, orgUuid);
            }
        }

        public async Task<OrganizationUnitResponseDTO> CreateOrganizationUnitAsync(Guid organizationUuid,
            string name = null, Guid? parentUnitUuid = null)
        {
            var rootUnit = await OrganizationUnitV2Helper.GetRootUnit(organizationUuid);
            var request = new CreateOrganizationUnitRequestDTO
            {
                Name = name ?? A<string>(),
                ParentUuid = parentUnitUuid ?? rootUnit.Uuid
            };
            return await OrganizationUnitV2Helper.CreateUnitAsync(organizationUuid, request);
        }

        public async Task<ItInterfaceResponseDTO> CreateItInterfaceAsync(Guid organizationUuid, string name = null,
            RegistrationScopeChoice scope = RegistrationScopeChoice.Global, string interfaceId = null)
        {
            var request = new CreateItInterfaceRequestDTO
            {
                Name = name ?? A<string>(),
                OrganizationUuid = organizationUuid,
                Scope = scope,
                InterfaceId = interfaceId ?? A<string>()
            };
            return await InterfaceV2Helper.CreateItInterfaceAsync(await GetGlobalToken(), request);
        }

        public async Task<UserResponseDTO> CreateUserAsync(Guid organizationUuid, string email = null, bool apiAccess = false)
        {
            var request = new CreateUserRequestDTO
            {
                FirstName = A<string>(),
                LastName = A<string>(),
                Email = email ?? CreateEmail(),
                HasApiAccess = apiAccess,
                Roles = new List<OrganizationRoleChoice> { OrganizationRoleChoice.User }
            };
            return await UsersV2Helper.CreateUser(organizationUuid, request);
        }

        public async Task<ItSystemUsageResponseDTO> CreateSystemAndTakeItIntoUsage(Guid organizationUuid)
        {
            var system = await CreateItSystemAsync(organizationUuid);
            return await TakeSystemIntoUsageAsync(system.Uuid, organizationUuid);
        }

        public string CreateCvr()
        {
            return _random.Next(0, 100_000_000)
                .ToString("D8");
        }

        public string CreateEmail()
        {
            return $"{A<string>()}@kitos.dk";
        }

        public async Task<string> GetGlobalToken()
        {
            if (_token != null)
            {
                return _token;
            }

            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            _token = token.Token;
            return _token;
        }
    }
}
