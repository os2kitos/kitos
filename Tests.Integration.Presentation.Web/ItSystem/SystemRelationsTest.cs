using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
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
            using (var response = await ItSystemHelper.SendPostRelationAsync(input))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var relations = (await ItSystemHelper.GetRelationsAsync(input.SourceUsageId)).ToList();
                Assert.Equal(1, relations.Count);
                var dto = relations.Single();
                Assert.Equal(input.SourceUsageId, dto.Source.Id);
                Assert.Equal(input.TargetUsageId, dto.Destination.Id);
                Assert.Equal(input.Description, dto.Description);
                Assert.Equal(input.Reference, dto.Reference);
                Assert.Equal(input.ContractId, dto.Contract?.Id);
                Assert.Equal(input.InterfaceId, dto.Interface?.Id);
                Assert.Equal(input.FrequencyTypeId, dto.FrequencyType?.Id);
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
                SourceUsageId = usage1.Id,
                TargetUsageId = usage2.Id,
                Description = A<string>(),
                Reference = A<string>(),
            };

            using (var response = await ItSystemHelper.SendPostRelationAsync(input))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var createdRelation = await response.ReadResponseBodyAsKitosApiResponseAsync<SystemRelationDTO>();

                //Act
                using (var deleteResponse = await ItSystemHelper.SendDeleteRelationAsync(usage1.Id, createdRelation.Id))
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

            using (await ItSystemHelper.SendPostRelationAsync(input))
            using (var deletionResponse = await ItSystemHelper.SendRemoveUsageAsync(input.SourceUsageId, OrganizationId))
            using (var getAfterDeleteResponse = await ItSystemHelper.SendGetRelationAsync(input.SourceUsageId, OrganizationId))
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
            var availableDestinationSystems = (await ItSystemHelper.GetAvailableDestinationSystemsAsync(sourceUsage.Id, prefix))?.ToList();

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
            var options = await ItSystemHelper.GetAvailableOptionsAsync(input.SourceUsageId, input.TargetUsageId);

            //Assert
            Assert.NotNull(options);
            var interfaceDTO = Assert.Single(options.AvailableInterfaces);
            Assert.Equal(input.InterfaceId.Value, interfaceDTO.Id);
            Assert.Contains(options.AvailableContracts.Select(x => x.Id), x => x == input.ContractId);
            Assert.Contains(options.AvailableFrequencyTypes.Select(x => x.Id), x => x == input.FrequencyTypeId);
        }

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
                SourceUsageId = usage1.Id,
                TargetUsageId = usage2.Id,
                ContractId = contract.Select<int?>(x => x.Id).GetValueOrDefault(),
                InterfaceId = targetInterface.Select<int?>(x => x.Id).GetValueOrDefault(),
                Description = A<string>(),
                Reference = A<string>(),
                FrequencyTypeId = targetFrequencyTypeId
            };
            return input;
        }

        private string CreateName()
        {
            return $"Relations_{A<Guid>():N}";
        }
    }
}
