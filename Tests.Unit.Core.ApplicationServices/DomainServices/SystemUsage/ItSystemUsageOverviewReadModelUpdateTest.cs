using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Options;
using Core.DomainServices.SystemUsage;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SystemUsage
{
    public class ItSystemUsageOverviewReadModelUpdateTest : WithAutoFixture
    {

        private readonly Mock<IOptionsService<ItSystem, BusinessType>> _businessTypeService;

        private readonly Mock<IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel>> _roleAssignmentRepository;
        private readonly Mock<IGenericRepository<ItSystemUsageOverviewTaskRefReadModel>> _taskRefRepository;
        private readonly ItSystemUsageOverviewReadModelUpdate _sut;

        public ItSystemUsageOverviewReadModelUpdateTest()
        {
            _businessTypeService = new Mock<IOptionsService<ItSystem, BusinessType>>();
            _taskRefRepository = new Mock<IGenericRepository<ItSystemUsageOverviewTaskRefReadModel>>();
            _roleAssignmentRepository = new Mock<IGenericRepository<ItSystemUsageOverviewRoleAssignmentReadModel>>();
            _sut = new ItSystemUsageOverviewReadModelUpdate(
                _roleAssignmentRepository.Object,
                _taskRefRepository.Object,
                _businessTypeService.Object);
        }

        [Fact]
        public void Apply_Generates_Correct_Read_Model()
        {
            //Arrange
            var user = new User
            {
                Id = A<int>(),
                Name = A<string>(),
                LastName = A<string>(),
                Email = $"{A<string>()}@{A<string>()}.dk"
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
                Uuid = A<Guid>(),
                BelongsTo = new Organization
                {
                    Id = A<int>(),
                    Name = A<string>()
                },
                BusinessType = new BusinessType
                {
                    Id = A<int>(),
                    Name = A<string>()
                },
                TaskRefs = new List<TaskRef>
                {
                    new TaskRef
                    {
                        Id = A<int>(),
                        TaskKey = A<string>(),
                        Description = A<string>()
                    }
                }
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
                },
                Reference = new ExternalReference
                {
                    Id = A<int>(),
                    Title = A<string>(),
                    ExternalReferenceId = A<string>(),
                    URL = A<string>()
                }
            };

            // Add ResponsibleOrganizationUnit
            var organizationUnit = new OrganizationUnit
            {
                Id = A<int>(),
                Name = A<string>()
            };
            var systemUsageOrgUnitUsage = new ItSystemUsageOrgUnitUsage
            {
                OrganizationUnit = organizationUnit,
                OrganizationUnitId = organizationUnit.Id,
                ItSystemUsage = systemUsage,
                ItSystemUsageId = systemUsage.Id
            };
            systemUsage.ResponsibleUsage = systemUsageOrgUnitUsage;

            _businessTypeService.Setup(x => x.GetOption(system.OrganizationId, system.BusinessType.Id)).Returns(Maybe<(BusinessType, bool)>.Some((system.BusinessType, true)));


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
            Assert.Equal(system.BelongsTo.Id, readModel.ItSystemRightsHolderId);
            Assert.Equal(system.BelongsTo.Name, readModel.ItSystemRightsHolderName);
            Assert.Equal(system.BusinessType.Id, readModel.ItSystemBusinessTypeId);
            Assert.Equal(system.BusinessType.Name, readModel.ItSystemBusinessTypeName);

            //Parent System
            Assert.Equal(parentSystem.Name, readModel.ParentItSystemName);
            Assert.Equal(parentSystem.Id, readModel.ParentItSystemId);

            //Assigned Roles
            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            Assert.Equal(user.Id, roleAssignment.UserId);
            Assert.Equal(user.GetFullName(), roleAssignment.UserFullName);
            Assert.Equal(right.RoleId, roleAssignment.RoleId);
            Assert.Equal(user.Email, roleAssignment.Email);

            //Responsible Organization Unit
            Assert.Equal(organizationUnit.Id, readModel.ResponsibleOrganizationUnitId);
            Assert.Equal(organizationUnit.Name, readModel.ResponsibleOrganizationUnitName);

            //KLE
            Assert.Equal(system.TaskRefs.First().TaskKey, readModel.ItSystemKLEIdsAsCsv);
            Assert.Equal(system.TaskRefs.First().Description, readModel.ItSystemKLENamesAsCsv);
            var taskRef = Assert.Single(readModel.ItSystemTaskRefs);
            Assert.Equal(system.TaskRefs.First().TaskKey, taskRef.KLEId);
            Assert.Equal(system.TaskRefs.First().Description, taskRef.KLEName);

            //Reference
            Assert.Equal(systemUsage.Reference.Title, readModel.LocalReferenceTitle);
            Assert.Equal(systemUsage.Reference.URL, readModel.LocalReferenceUrl);
            Assert.Equal(systemUsage.Reference.ExternalReferenceId, readModel.LocalReferenceDocumentId);
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
