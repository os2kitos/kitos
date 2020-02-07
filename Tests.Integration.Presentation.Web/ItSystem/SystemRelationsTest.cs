using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using ExpectedObjects;
using Presentation.Web.Models;
using Presentation.Web.Models.SystemRelations;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class SystemRelationsTest : WithAutoFixture
    {
        private const int OrganizationId = TestEnvironment.DefaultOrganizationId;

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        public async Task Post_SystemRelation_Returns_201(bool withContract, bool withInterface, bool withFrequency)
        {
            //Arrange
            var input = await PrepareFullRelationAsync(withContract, withInterface, withFrequency);

            //Act
            using (var response = await SystemRelationHelper.SendPostRelationAsync(input))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var relationsFrom = (await SystemRelationHelper.SendGetRelationsFromAsync(input.FromUsageId)).ToList();
                var relationsTo = (await SystemRelationHelper.SendGetRelationsToAsync(input.ToUsageId)).ToList();
                var fromDto = Assert.Single(relationsFrom);
                var toDto = Assert.Single(relationsTo);

                fromDto.ToExpectedObject().ShouldMatch(toDto); //Same relation should yield same data at the dto level

                Assert.Equal(input.FromUsageId, fromDto.FromUsage.Id);
                Assert.Equal(input.ToUsageId, fromDto.ToUsage.Id);
                Assert.Equal(input.Description, fromDto.Description);
                Assert.Equal(input.Reference, fromDto.Reference);
                Assert.Equal(input.ContractId, fromDto.Contract?.Id);
                Assert.Equal(input.InterfaceId, fromDto.Interface?.Id);
                Assert.Equal(input.FrequencyTypeId, fromDto.FrequencyType?.Id);
            }
        }

        [Fact]
        public async Task Delete_SystemRelation_Returns_204()
        {
            //Arrange
            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), OrganizationId, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), OrganizationId, AccessModifier.Public);
            var usage1 = await ItSystemHelper.TakeIntoUseAsync(system1.Id, OrganizationId);
            var usage2 = await ItSystemHelper.TakeIntoUseAsync(system2.Id, OrganizationId);

            var input = new CreateSystemRelationDTO
            {
                FromUsageId = usage1.Id,
                ToUsageId = usage2.Id,
                Description = A<string>(),
                Reference = A<string>(),
            };

            using (var response = await SystemRelationHelper.SendPostRelationAsync(input))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var createdRelation = await response.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();

                //Act
                using (var deleteResponse = await SystemRelationHelper.SendDeleteRelationAsync(usage1.Id, createdRelation.Id))
                {
                    //Assert
                    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
                }
            }
        }

        [Fact]
        public async Task Can_Delete_SystemUsageWithRelations()
        {
            //Arrange
            var input = await PrepareFullRelationAsync(false, false, false);

            using (await SystemRelationHelper.SendPostRelationAsync(input))
            using (var deletionResponse = await ItSystemHelper.SendRemoveUsageAsync(input.FromUsageId, OrganizationId))
            using (var getAfterDeleteResponse = await SystemRelationHelper.SendGetRelationAsync(input.FromUsageId, OrganizationId))
            {
                Assert.Equal(HttpStatusCode.OK, deletionResponse.StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Can_Get_AvailableDestinationSystems()
        {
            //Arrange
            var prefix = CreateName();
            var source = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), OrganizationId, AccessModifier.Public);
            var target1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(prefix + 1, OrganizationId, AccessModifier.Public);
            var target2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(prefix + 2, OrganizationId, AccessModifier.Public);
            var ignoredSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), OrganizationId, AccessModifier.Public);
            var sourceUsage = await ItSystemHelper.TakeIntoUseAsync(source.Id, OrganizationId);
            var targetUsage1 = await ItSystemHelper.TakeIntoUseAsync(target1.Id, OrganizationId);
            var targetUsage2 = await ItSystemHelper.TakeIntoUseAsync(target2.Id, OrganizationId);
            await ItSystemHelper.TakeIntoUseAsync(ignoredSystem.Id, OrganizationId);

            //Act
            var availableDestinationSystems = (await SystemRelationHelper.GetAvailableDestinationSystemsAsync(sourceUsage.Id, prefix))?.ToList();

            //Assert
            Assert.NotNull(availableDestinationSystems);
            Assert.Equal(2, availableDestinationSystems.Count);
            Assert.True(new[] { targetUsage1.Id, targetUsage2.Id }.SequenceEqual(availableDestinationSystems.Select(x => x.Id)));
        }

        [Fact]
        public async Task Can_Get_AvailableOptions()
        {
            //Arrange
            var input = await PrepareFullRelationAsync(true, true, true);


            //Act
            var options = await SystemRelationHelper.GetAvailableOptionsAsync(input.FromUsageId, input.ToUsageId);

            //Assert
            Assert.NotNull(options);
            var interfaceDTO = Assert.Single(options.AvailableInterfaces);
            Assert.Equal(input.InterfaceId.Value, interfaceDTO.Id);
            Assert.Contains(options.AvailableContracts.Select(x => x.Id), x => x == input.ContractId);
            Assert.Contains(options.AvailableFrequencyTypes.Select(x => x.Id), x => x == input.FrequencyTypeId);
        }

        [Fact]
        public async Task Can_Edit_SystemUsageWithRelations()
        {
            //Arrange
            var input = await PrepareFullRelationAsync(true, false, true);
            await SystemRelationHelper.SendPostRelationAsync(input);
            var relations = await SystemRelationHelper.SendGetRelationsFromAsync(input.FromUsageId);
            var edited = await PrepareEditedRelationAsync(relations.Single());

            //Act
            using (var response = await SystemRelationHelper.SendPatchRelationAsync(edited))
            {
                //Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var relationDTO = await response.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();
                Assert.Equal(input.FromUsageId, relationDTO.FromUsage.Id);
                Assert.Equal(edited.ToUsage.Id, relationDTO.ToUsage.Id);
                Assert.Equal(edited.Description, relationDTO.Description);
                Assert.Equal(edited.Reference, relationDTO.Reference);
                Assert.Equal(edited.Interface.Id, relationDTO.Interface.Id);
            }
        }

        [Fact]
        public async Task Changing_Exposing_System_On_Interface_Clears_InterfaceField_In_All_Relations_To_Old_Exposing_System()
        {
            //Arrange
            var newExhibitor = await ItSystemHelper.CreateItSystemInOrganizationAsync(A<string>(), TestEnvironment.DefaultOrganizationId, AccessModifier.Public);
            var input = await PrepareFullRelationAsync(false, false, true);

            //Act
            using (var response = await SystemRelationHelper.SendPostRelationAsync(input))
            using (var changeExposingSystem = await InterfaceExhibitHelper.SendCreateExhibitRequest(newExhibitor.Id, input.InterfaceId.GetValueOrDefault()))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(HttpStatusCode.Created, changeExposingSystem.StatusCode);
                var relation = await response.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();
                Assert.NotNull(relation.Interface);
                using (var getAfterDeleteResponse = await SystemRelationHelper.SendGetRelationAsync(input.FromUsageId, relation.Id))
                {
                    Assert.Equal(HttpStatusCode.OK, getAfterDeleteResponse.StatusCode);
                    var relationAfterChange = await getAfterDeleteResponse.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();
                    Assert.Null(relationAfterChange.Interface);
                }
            }
        }

        [Fact]
        public async Task Changing_Exposing_System_To_Same_As_Existing_Does_Nothing()
        {
            //Arrange
            var input = await PrepareFullRelationAsync(false, false, true);
            var toUsage = await ItSystemHelper.GetItSystemUsage(input.ToUsageId);

            //Act
            using (var response = await SystemRelationHelper.SendPostRelationAsync(input))
            using (var changeExposingSystem = await InterfaceExhibitHelper.SendCreateExhibitRequest(toUsage.ItSystemId, input.InterfaceId.GetValueOrDefault()))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(HttpStatusCode.Created, changeExposingSystem.StatusCode);
                var relation = await response.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();
                Assert.NotNull(relation.Interface);
                using (var getAfterDeleteResponse = await SystemRelationHelper.SendGetRelationAsync(input.FromUsageId, relation.Id))
                {
                    Assert.Equal(HttpStatusCode.OK, getAfterDeleteResponse.StatusCode);
                    var relationAfterChange = await getAfterDeleteResponse.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();
                    Assert.NotNull(relationAfterChange.Interface); //interface should not have been cleared since the same exhibitor was provided as the existing --> no change
                }
            }
        }

        [Fact]
        [Description("Even if we already get the reset from the Exhibit removal (exhibit blocks deletion of interfaces), we still want to make sure that delete works if this changes")]
        public async Task Deleting_Interface_Clears_InterfaceField_In_All_Relations_To_Old_Exposing_System()
        {
            //Arrange
            var input = await PrepareFullRelationAsync(false, false, true);

            //Act
            using (var response = await SystemRelationHelper.SendPostRelationAsync(input))

            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var relation = await response.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();
                Assert.NotNull(relation.Interface);

                using (var removeExhibitResponse = await InterfaceExhibitHelper.SendRemoveExhibitRequest(input.InterfaceId.GetValueOrDefault())) //Must remove exposition to allow deletion
                using (var deleteInterfaceResponse = await InterfaceHelper.SendDeleteInterfaceRequestAsync(input.InterfaceId.GetValueOrDefault()))
                using (var getAfterDeleteResponse = await SystemRelationHelper.SendGetRelationAsync(input.FromUsageId, relation.Id))
                {
                    //Assert
                    Assert.Equal(HttpStatusCode.OK, removeExhibitResponse.StatusCode);
                    Assert.Equal(HttpStatusCode.OK, deleteInterfaceResponse.StatusCode);
                    Assert.Equal(HttpStatusCode.OK, getAfterDeleteResponse.StatusCode);
                    var relationAfterChange = await getAfterDeleteResponse.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();
                    Assert.Null(relationAfterChange.Interface);
                }
            }
        }

        [Fact]
        public async Task Deleting_Contract_Clears_ContractField_In_All_AssociatedRelations()
        {
            //Arrange
            var input = await PrepareFullRelationAsync(true, false, false);

            //Act
            using (var response = await SystemRelationHelper.SendPostRelationAsync(input))

            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var relation = await response.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();
                Assert.NotNull(relation.Contract);

                using (var deleteContractRequest = await ItContractHelper.SendDeleteContractRequestAsync(input.ContractId.GetValueOrDefault()))
                using (var getAfterDeleteResponse = await SystemRelationHelper.SendGetRelationAsync(input.FromUsageId, relation.Id))
                {
                    //Assert
                    Assert.Equal(HttpStatusCode.OK, deleteContractRequest.StatusCode);
                    Assert.Equal(HttpStatusCode.OK, getAfterDeleteResponse.StatusCode);
                    var relationAfterChange = await getAfterDeleteResponse.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();
                    Assert.Null(relationAfterChange.Contract);
                }
            }
        }

        #region Helpers

        private async Task<CreateSystemRelationDTO> PrepareFullRelationAsync(bool withContract, bool withFrequency, bool withInterface)
        {
            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), OrganizationId, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), OrganizationId, AccessModifier.Public);
            var usage1 = await ItSystemHelper.TakeIntoUseAsync(system1.Id, OrganizationId);
            var usage2 = await ItSystemHelper.TakeIntoUseAsync(system2.Id, OrganizationId);
            var targetInterface = Maybe<ItInterfaceDTO>.None;
            if (withInterface)
            {
                targetInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(CreateName(), CreateName(), null, OrganizationId, AccessModifier.Public));
                await InterfaceExhibitHelper.CreateExhibit(system2.Id, targetInterface.Value.Id);
            }

            var contract = withContract ? await ItContractHelper.CreateContract(CreateName(), OrganizationId) : Maybe<ItContractDTO>.None;

            var targetFrequencyTypeId = withFrequency ? DatabaseAccess.MapFromEntitySet<RelationFrequencyType, int>(repo =>
            {
                var first = repo
                    .AsQueryable()
                    .First(x => x.IsEnabled);
                return first.Id;
            }) : default(int?);

            var input = new CreateSystemRelationDTO
            {
                FromUsageId = usage1.Id,
                ToUsageId = usage2.Id,
                ContractId = contract.Select<int?>(x => x.Id).GetValueOrDefault(),
                InterfaceId = targetInterface.Select<int?>(x => x.Id).GetValueOrDefault(),
                Description = A<string>(),
                Reference = A<string>(),
                FrequencyTypeId = targetFrequencyTypeId
            };
            return input;
        }

        private async Task<SystemRelationDTO> PrepareEditedRelationAsync(SystemRelationDTO created)
        {
            var system3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), OrganizationId, AccessModifier.Public);
            var usage3 = await ItSystemHelper.TakeIntoUseAsync(system3.Id, OrganizationId);
            var targetInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(CreateName(), CreateName(), null, OrganizationId, AccessModifier.Public));
            var interfaceExhibitDTO = await InterfaceExhibitHelper.CreateExhibit(system3.Id, targetInterface.Id);

            return new SystemRelationDTO(
                created.Id,
                created.FromUsage,
                new NamedEntityDTO(usage3.Id, usage3.LocalCallName),
                new NamedEntityDTO(interfaceExhibitDTO.ItInterfaceId, interfaceExhibitDTO.ItInterfaceName),
                null, // contract
                null, // frquencytype
                "", // description
                "" // reference
                );
        }

        private string CreateName()
        {
            return $"Relations_{A<Guid>():N}";
        }

        #endregion
    }
}
