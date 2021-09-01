using AutoMapper;
using Core.DomainModel.ItSystem;
using Presentation.Web;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Infrastructure
{
    /// <summary>
    /// Use this testfixture to validate automapper mappings (use the singleton mapper instance field as compilation of mapping plans take time so we should only do it once)
    /// </summary>
    public class AutoMapperTest : WithAutoFixture
    {
        private static readonly IMapper Mapper = MappingConfig.CreateMapper();

        [Fact]
        public void Map_To_InterfaceDtoOnlyCreatesValidData()
        {
            var dto = new ItInterfaceDTO
            {
                OrganizationId = A<int>(),
                Name = A<string>(),
                ItInterfaceId = A<string>()
            };

            var itInterface = Mapper.Map<ItInterfaceDTO, ItInterface>(dto);

            Assert.NotNull(itInterface.AssociatedSystemRelations); //Set by ctor
            Assert.Null(itInterface.BrokenLinkReports);
            Assert.NotNull(itInterface.DataRows); //Set by ctor
            Assert.Null(itInterface.ExhibitedBy);
            Assert.Null(itInterface.Interface);
            Assert.Null(itInterface.InterfaceId);
            Assert.Null(itInterface.LastChangedByUser);
            Assert.Null(itInterface.LastChangedByUserId);
            Assert.Null(itInterface.Organization);
            Assert.Null(itInterface.ObjectOwnerId);
            Assert.Null(itInterface.ObjectOwner);

            //Check the expected mappings
            Assert.Equal(dto.OrganizationId, itInterface.OrganizationId);
            Assert.Equal(dto.ItInterfaceId, itInterface.ItInterfaceId);
            Assert.Equal(dto.Name, itInterface.Name);
        }
    }
}