using System;
using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices;
using Core.DomainServices.SystemUsage;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SystemUsage
{
    public class ItSystemUsageOverviewReadModelUpdateTest : WithAutoFixture
    {

        private readonly Mock<IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel>> _roleAssignmentRepository;
        private readonly ItSystemUsageOverviewReadModelUpdate _sut;

        public ItSystemUsageOverviewReadModelUpdateTest()
        {
            _roleAssignmentRepository = new Mock<IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel>>();
            _sut = new ItSystemUsageOverviewReadModelUpdate(
                _roleAssignmentRepository.Object);
        }

        [Fact]
        public void Apply_Generates_Correct_Read_Model()
        {
            //Arrange
            var user = new User
            {
                Id = A<int>(),
                Name = A<string>(),
                LastName = A<string>()
            };
            var right = new ItSystemRight
            {
                Id = A<int>(),
                User = user,
                UserId = user.Id,
                RoleId = A<int>()
            };
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
                LocalSystemId = A<string>(),
                Rights = new List<ItSystemRight>
                {
                    right
                }
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
            Assert.Equal(system.Uuid, readModel.ItSystemUuid);

            //Parent System
            Assert.Equal(parentSystem.Name, readModel.ParentItSystemName);
            Assert.Equal(parentSystem.Id, readModel.ParentItSystemId);

            //Assigned Roles
            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            Assert.Equal(user.Id, roleAssignment.UserId);
            Assert.Equal($"{user.Name} {user.LastName}", roleAssignment.UserFullName);
            Assert.Equal(right.RoleId, roleAssignment.RoleId);
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
