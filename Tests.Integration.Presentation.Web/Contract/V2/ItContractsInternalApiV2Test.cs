using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using ExpectedObjects;
using Presentation.Web.Models.API.V1;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using System.Security.Cryptography;
using Tests.Integration.Presentation.Web.ItSystem.V2;

namespace Tests.Integration.Presentation.Web.Contract.V2
{
    public class ItContractsInternalApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task Can_Get_Available_DataProcessingRegistrations()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var registrationName = A<string>();
            var registration1 = await DataProcessingRegistrationHelper.CreateAsync(organizationId, registrationName + "1");
            var registration2 = await DataProcessingRegistrationHelper.CreateAsync(organizationId, registrationName + "2");
            var contract = await ItContractHelper.CreateContract(A<string>(), organizationId);

            //Act
            var dtos = (await ItContractV2Helper.GetAvailableDataProcessingRegistrationsAsync(contract.Uuid, registrationName)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            dtos.Select(x => new { x.Uuid, x.Name }).ToExpectedObject().ShouldMatch(new[] { new { registration1.Uuid, registration1.Name }, new { registration2.Uuid, registration2.Name } });
        }


        [Fact]
        public async Task Can_Get_Hierarchy()
        {
            //Arrange
            var (_, organization) = await CreateStakeHolderUserInNewOrganizationAsync();
            var (rootUuid, createdContracts) = CreateHierarchy(organization.Id);

            //Act
            var response = await ItContractV2Helper.GetHierarchyAsync(rootUuid);

            //Assert
            var hierarchy = response.ToList();
            Assert.Equal(createdContracts.Count, hierarchy.Count);

            foreach (var node in hierarchy)
            {
                var contract = Assert.Single(createdContracts, x => x.Uuid == node.Node.Uuid);
                if (contract.Uuid == rootUuid)
                {
                    Assert.Null(node.Parent);
                }
                else
                {
                    Assert.NotNull(node.Parent);
                    Assert.Equal(node.Parent.Uuid, contract.Parent.Uuid);
                }
            }
        }

        protected async Task<(string token, OrganizationDTO createdOrganization)> CreateStakeHolderUserInNewOrganizationAsync()
        {
            var organization = await CreateOrganizationAsync();

            var (_, _, token) = await HttpApi.CreateUserAndGetToken(CreateEmail(),
                OrganizationRole.User, organization.Id, true, true);
            return (token, organization);
        }

        private (Guid rootUuid, IReadOnlyList<ItContract> createdItContracts) CreateHierarchy(int orgId)
        {
            var rootContract = CreateContractAsync(orgId);
            var childContract = CreateContractAsync(orgId);
            var grandchildContract = CreateContractAsync(orgId);

            var createdSystems = new List<ItContract> { rootContract, childContract, grandchildContract };

            childContract.Children = new List<ItContract>
            {
                grandchildContract
            };
            rootContract.Children = new List<ItContract>
            {
                childContract
            };

            DatabaseAccess.MutateEntitySet<ItContract>(repository =>
            {
                repository.Insert(rootContract);
            });

            return (rootContract.Uuid, createdSystems);
        }

        protected async Task<OrganizationDTO> CreateOrganizationAsync()
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, organizationName, "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            return organization;
        }

        private ItContract CreateContractAsync(int orgId)
        {
            return new ItContract
            {
                Name = A<string>(),
                OrganizationId = orgId,
                ObjectOwnerId = TestEnvironment.DefaultUserId,
                LastChangedByUserId = TestEnvironment.DefaultUserId
            };
        }
        private string CreateEmail()
        {
            return $"{CreateName()}@kitos.dk";
        }

        private string CreateName()
        {
            return $"{nameof(ItContractsInternalApiV2Test)}{A<string>()}";
        }
    }
}
