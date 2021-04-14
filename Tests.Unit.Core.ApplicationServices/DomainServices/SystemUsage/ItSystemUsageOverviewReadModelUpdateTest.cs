using System;
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
            var parentSystem = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var system = new ItSystem
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                Name = A<string>(),
                Disabled = A<bool>(),
                Parent = parentSystem,
                Uuid = A<Guid>()
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = system,
                Active = A<bool>(),
                ExpirationDate = DateTime.Now.AddDays(-1),
                Version = A<string>(),
                LocalCallName = A<string>(),
                LocalSystemId = A<string>()
            };

            var readModel = new ItSystemUsageOverviewReadModel();
            //Act
            _sut.Apply(systemUsage, readModel);

            //Assert
            //System usage
            Assert.Equal(systemUsage.Id, readModel.SourceEntityId);
            Assert.Equal(systemUsage.OrganizationId, readModel.OrganizationId);
            Assert.Equal(systemUsage.IsActive, readModel.IsActive);
            Assert.Equal(systemUsage.Version, readModel.Version);
            Assert.Equal(systemUsage.LocalCallName, readModel.LocalCallName);
            Assert.Equal(systemUsage.LocalSystemId, readModel.LocalSystemId);

            //System
            Assert.Equal(system.Name, readModel.Name);
            Assert.Equal(system.Disabled, readModel.ItSystemDisabled);
            Assert.Equal(system.Uuid.ToString("D"), readModel.ItSystemUuid);

            //Parent System
            Assert.Equal(parentSystem.Name, readModel.ParentItSystemName);
            Assert.Equal(parentSystem.Id, readModel.ParentItSystemId);
        }

        [Fact]
        public void Apply_Generates_Read_Model_With_IsActive_False_When_ExpirationDate_Is_Earlier_Than_Today()
        {
            //Act
            var readModel = Test_IsActive_Based_On_ExpirationDate(DateTime.Now.AddDays(-A<int>()));

            //Assert
            Assert.False(readModel.IsActive);
        }

        [Fact]
        public void Apply_Generates_Read_Model_With_IsActive_False_When_ExpirationDate_Is_Today()
        {
            //Act
            var readModel = Test_IsActive_Based_On_ExpirationDate(DateTime.Now);

            //Assert
            Assert.False(readModel.IsActive);
        }

        [Fact]
        public void Apply_Generates_Read_Model_With_IsActive_False_When_ExpirationDate_Is_Later_Than_Today()
        {
            //Act
            var readModel = Test_IsActive_Based_On_ExpirationDate(DateTime.Now.AddDays(A<int>()));

            //Assert
            Assert.False(readModel.IsActive);
        }

        [Fact]
        public void Apply_Generates_Read_Model_When_No_Parent_System()
        {
            //Arrange
            var system = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
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
            Assert.Null(readModel.ParentItSystemName);
            Assert.Null(readModel.ParentItSystemId);
        }

        private ItSystemUsageOverviewReadModel Test_IsActive_Based_On_ExpirationDate(DateTime expirationDate)
        {
            var system = new ItSystem
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var systemUsage = new ItSystemUsage
            {
                Id = A<int>(),
                OrganizationId = A<int>(),
                ItSystem = system,
                Active = false,
                ExpirationDate = DateTime.Now.AddDays(-1)
            };

            var readModel = new ItSystemUsageOverviewReadModel();

            _sut.Apply(systemUsage, readModel);

            return readModel;
        }
    }
}
