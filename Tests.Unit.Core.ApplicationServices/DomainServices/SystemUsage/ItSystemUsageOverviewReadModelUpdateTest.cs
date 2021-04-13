using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.SystemUsage;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SystemUsage
{
    public class ItSystemUsageOverviewReadModelUpdateTest : WithAutoFixture
    {

        private readonly ItSystemUsageOverviewReadModelUpdate _sut;

        public ItSystemUsageOverviewReadModelUpdateTest()
        {
            _sut = new ItSystemUsageOverviewReadModelUpdate();
        }

        [Fact]
        public void Apply_Generates_Correct_Read_Model()
        {
            //Arrange
            var system = new ItSystem
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                Name = A<string>(),
                Disabled = A<bool>()
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = system
            };

            var readModel = new ItSystemUsageOverviewReadModel();
            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            //System usage
            Assert.Equal(systemUsage.Id, readModel.SourceEntityId);
            Assert.Equal(systemUsage.OrganizationId, readModel.OrganizationId);

            //System
            Assert.Equal(system.Name, readModel.Name);
            Assert.Equal(system.Disabled, readModel.ItSystemDisabled);


        }
    }
}
