using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.KLE;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Repositories.TaskRefs;
using Moq;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public class KLEApplicationServiceTest
    {
        private readonly Mock<IKLEStandardRepository> _mockKleStandardRepository;
        private readonly KLEApplicationService _sut;
        private readonly Mock<IOrganizationalUserContext> _mockOrganizationalUserContext;
        private readonly Mock<IKLEUpdateHistoryItemRepository> _mockUpdateHistoryItemRepository;
        private readonly Mock<ITaskRefRepository> _taskRefRepositoryMock;

        public KLEApplicationServiceTest()
        {
            _mockKleStandardRepository = new Mock<IKLEStandardRepository>();
            _mockUpdateHistoryItemRepository = new Mock<IKLEUpdateHistoryItemRepository>();
            _mockOrganizationalUserContext = new Mock<IOrganizationalUserContext>();
            _taskRefRepositoryMock = new Mock<ITaskRefRepository>();
            _sut = new KLEApplicationService(_mockOrganizationalUserContext.Object, _mockKleStandardRepository.Object, _mockUpdateHistoryItemRepository.Object, _taskRefRepositoryMock.Object);

        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.User, false)]
        public void GetKLEStatus_Authorizes_And_Returns_Valid_KLEStatus(OrganizationRole role, bool isOk)
        {
            // Arrange
            var expectedPublishedDate = DateTime.Parse("01-11-2019");
            var kleStatus = new KLEStatus
            {
                UpToDate = false,
                Published = expectedPublishedDate
            };
            var lastUpdate = DateTime.Parse("01-01-1970");
            _mockUpdateHistoryItemRepository.Setup(r => r.GetLastUpdated()).Returns(lastUpdate);
            _mockKleStandardRepository.Setup(r => r.GetKLEStatus(lastUpdate)).Returns(kleStatus);
            _mockOrganizationalUserContext.Setup(r => r.IsGlobalAdmin()).Returns(role == OrganizationRole.GlobalAdmin);

            // Act
            var result = _sut.GetKLEStatus();

            // Assert
            Assert.Equal(isOk, result.Ok);
            _mockKleStandardRepository.Verify();
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, 1)]
        [InlineData(OrganizationRole.User, false, 0)]
        public void GetKLEChangeSummary_Authorizes_And_Returns_Valid_Number_Of_Changes(OrganizationRole role, bool isOk, int expectedNumberOfChanges)
        {
            // Arrange
            _mockKleStandardRepository.Setup(r => r.GetKLEChangeSummary()).Returns(new List<KLEChange>
            {
                new KLEChange { ChangeType = KLEChangeType.Added, TaskKey = "dummy", UpdatedDescription = "dummy"}
            }.OrderBy(c => c.TaskKey));
            _mockOrganizationalUserContext.Setup(r => r.IsGlobalAdmin()).Returns(role == OrganizationRole.GlobalAdmin);

            // Act
            var result = _sut.GetKLEChangeSummary();

            // Assert
            Assert.Equal(isOk, result.Ok);
            Assert.Equal(expectedNumberOfChanges, isOk ? result.Value.Count() : 0);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.User, false)]
        public void UpdateKLE_Authorizes_And_Updates(OrganizationRole role, bool isOk)
        {
            // Arrange
            _mockOrganizationalUserContext.Setup(r => r.IsGlobalAdmin()).Returns(role == OrganizationRole.GlobalAdmin);
            const int activeOrganizationId = 1;
            var publishedDate = DateTime.Today;
            _mockKleStandardRepository
                .Setup(r => r.UpdateKLE(activeOrganizationId))
                .Returns(publishedDate);
            _mockKleStandardRepository.Setup(r => r.GetKLEStatus(It.IsAny<DateTime>())).Returns(
                new KLEStatus
                {
                    UpToDate = false,
                    Published = publishedDate
                }
            );
            _mockKleStandardRepository.Setup(r => r.GetKLEChangeSummary()).Returns(new List<KLEChange>
            {
                new KLEChange { ChangeType = KLEChangeType.Added, TaskKey = "dummy", UpdatedDescription = "dummy"}
            }.OrderBy(c => c.TaskKey));
            
            // Act
            var result = _sut.UpdateKLE(activeOrganizationId);

            // Assert
            Assert.Equal(isOk, result.Ok);
            _mockUpdateHistoryItemRepository.Verify(r => r.Insert(publishedDate), isOk ? Times.Once() : Times.Never());
        }

        [Fact]
        public void UpdateKLE_GivenGlobalAdmin_And_UpToDateKLE_DoesNotUpdate()
        {
            // Arrange
            _mockOrganizationalUserContext.Setup(r => r.IsGlobalAdmin()).Returns(true);
            const int activeOrganizationId = 1;
            var publishedDate = DateTime.Today;
            _mockKleStandardRepository
                .Setup(r => r.UpdateKLE(activeOrganizationId))
                .Returns(publishedDate);
            _mockKleStandardRepository.Setup(r => r.GetKLEStatus(It.IsAny<DateTime>())).Returns(
                new KLEStatus
                {
                    UpToDate = true,
                    Published = publishedDate
                }
            );
            
            // Act
            var result = _sut.UpdateKLE(activeOrganizationId);
            
            // Assert
            Assert.False(result.Ok);
            _mockUpdateHistoryItemRepository.Verify(r => r.Insert(publishedDate), Times.Never());
        }
    }
}
