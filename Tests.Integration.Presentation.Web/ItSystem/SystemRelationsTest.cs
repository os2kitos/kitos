using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Presentation.Web.Models.SystemRelations;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.ItSystem
{
    public class SystemRelationsTest : WithAutoFixture
    {
        [Fact]
        public async Task Post_SystemRelation_Returns_201()
        {
            //Arrange
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organizationId, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(CreateName(), organizationId, AccessModifier.Public);
            var usage1 = await ItSystemHelper.TakeIntoUseAsync(system1.Id, organizationId);
            var usage2 = await ItSystemHelper.TakeIntoUseAsync(system2.Id, organizationId);
            var targetInterface = await InterfaceHelper.CreateInterface(InterfaceHelper.CreateInterfaceDto(CreateName(), CreateName(), null, organizationId, AccessModifier.Public));
            await InterfaceExhibitHelper.CreateExhibit(system2.Id, targetInterface.Id);
            var contract = await ItContractHelper.CreateContract(CreateName(), organizationId);

            var targetFrequencyTypeId = DatabaseAccess.MapFromEntitySet<RelationFrequencyType, int>(repo =>
            {
                var first = repo
                    .AsQueryable()
                    .First(x => x.IsEnabled);
                return first.Id;
            });

            var input = new CreateSystemRelationDTO
            {
                SourceUsageId = usage1.Id,
                TargetUsageId = usage2.Id,
                ContractId = contract.Id,
                InterfaceId = targetInterface.Id,
                Description = A<string>(),
                LinkName = A<string>(),
                LinkUrl = A<string>(),
                FrequencyTypeId = targetFrequencyTypeId
            };

            //Act
            using (var response = await ItSystemHelper.SendPostRelationAsync(input))
            {
                //Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var relations = (await ItSystemHelper.GetRelationsAsync(usage1.Id)).ToList();
                Assert.Equal(1,relations.Count);
            }
        }

        //TODO: Test that it system can be deleted without any nasty bindings

        private string CreateName()
        {
            return $"Relations_{A<Guid>():N}";
        }
    }
}
