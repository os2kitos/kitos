using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.KLE;
using Moq;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public class KLEApplicationServiceTest
    {
        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.User, false)]
        private void GetKLEStatus_Authorizes_And_Returns_Valid_KLEStatus(OrganizationRole role, bool isOk)
        {
            // Arrange
            var mockKLEStandardRepository = new Mock<IKLEStandardRepository>();
            var expectedPublishedDate = DateTime.Parse("01-11-2019");
            var kleStatus = new KLEStatus
            {
                UpToDate = false,
                Published = expectedPublishedDate
            };
            var lastUpdate = DateTime.Parse("01-01-1970");
            var mockUpdateHistoryItemRepository = new Mock<IKLEUpdateHistoryItemRepository>();
            mockUpdateHistoryItemRepository.Setup(r => r.GetLastUpdated()).Returns(lastUpdate);
            mockKLEStandardRepository.Setup(r => r.GetKLEStatus(lastUpdate)).Returns(kleStatus);
            var mockOrganizationalUserContext = new Mock<IOrganizationalUserContext>();
            mockOrganizationalUserContext.Setup(r => r.HasRole(role)).Returns(true);
            // Act
            var sut = new KLEApplicationService(mockOrganizationalUserContext.Object, mockKLEStandardRepository.Object, mockUpdateHistoryItemRepository.Object);
            // Assert
            var result = sut.GetKLEStatus();
            Assert.Equal(isOk, result.Ok);
            mockKLEStandardRepository.Verify();
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true, 1)]
        [InlineData(OrganizationRole.User, false, 0)]
        private void GetKLEChangeSummary_Authorizes_And_Returns_Valid_Number_Of_Changes(OrganizationRole role, bool isOk, int expectedNumberOfChanges)
        {
            // Arrange
            var mockKLEStandardRepository = new Mock<IKLEStandardRepository>();
            mockKLEStandardRepository.Setup(r => r.GetKLEChangeSummary()).Returns(new List<KLEChange>
            {
                new KLEChange { ChangeType = KLEChangeType.Added, TaskKey = "dummy", UpdatedDescription = "dummy"}
            });
            var mockOrganizationalUserContext = new Mock<IOrganizationalUserContext>();
            mockOrganizationalUserContext.Setup(r => r.HasRole(role)).Returns(true);
            var mockUpdateHistoryItemRepository = new Mock<IKLEUpdateHistoryItemRepository>();
            // Act
            var sut = new KLEApplicationService(mockOrganizationalUserContext.Object, mockKLEStandardRepository.Object, mockUpdateHistoryItemRepository.Object);
            var result = sut.GetKLEChangeSummary();
            // Assert
            Assert.Equal(isOk, result.Ok);
            Assert.Equal(expectedNumberOfChanges, isOk ? result.Value.Count() : 0);
        }

        [Theory]
        [InlineData(OrganizationRole.GlobalAdmin, true)]
        [InlineData(OrganizationRole.User, false)]
        private void UpdateKLE_Authorizes_And_Updates(OrganizationRole role, bool isOk)
        {
            // Arrange
            var mockOrganizationalUserContext = new Mock<IOrganizationalUserContext>();
            mockOrganizationalUserContext.Setup(r => r.HasRole(role)).Returns(true);
            const int activeOrganizationId = 1;
            mockOrganizationalUserContext.Setup(r => r.ActiveOrganizationId).Returns(activeOrganizationId);
            const int userId = 1;
            mockOrganizationalUserContext.Setup(r => r.UserId).Returns(userId);
            var mockKLEStandardRepository = new Mock<IKLEStandardRepository>();
            var publishedDate = DateTime.Today;
            mockKLEStandardRepository
                .Setup(r => r.UpdateKLE(userId, activeOrganizationId))
                .Returns(publishedDate);
            mockKLEStandardRepository.Setup(r => r.GetKLEStatus(It.IsAny<DateTime>())).Returns(
                new KLEStatus
                {
                    UpToDate = false,
                    Published = publishedDate
                }
            );
            mockKLEStandardRepository.Setup(r => r.GetKLEChangeSummary()).Returns(new List<KLEChange>
            {
                new KLEChange { ChangeType = KLEChangeType.Added, TaskKey = "dummy", UpdatedDescription = "dummy"}
            });
            var mockUpdateHistoryItemRepository = new Mock<IKLEUpdateHistoryItemRepository>();
            // Act
            var sut = new KLEApplicationService(mockOrganizationalUserContext.Object, mockKLEStandardRepository.Object, mockUpdateHistoryItemRepository.Object);
            var result = sut.UpdateKLE();
            // Assert
            Assert.Equal(isOk, result.Ok);
            mockUpdateHistoryItemRepository.Verify(r => r.Insert(publishedDate, userId), isOk ? Times.Once() : Times.Never());
        }

        [Fact]
        private void UpdateKLE_GivenGlobalAdmin_And_UpToDateKLE_DoesNotUpdate()
        {
            // Arrange
            var mockOrganizationalUserContext = new Mock<IOrganizationalUserContext>();
            mockOrganizationalUserContext.Setup(r => r.HasRole(OrganizationRole.GlobalAdmin)).Returns(true);
            const int activeOrganizationId = 1;
            mockOrganizationalUserContext.Setup(r => r.ActiveOrganizationId).Returns(activeOrganizationId);
            const int userId = 1;
            mockOrganizationalUserContext.Setup(r => r.UserId).Returns(userId);
            var mockKLEStandardRepository = new Mock<IKLEStandardRepository>();
            var publishedDate = DateTime.Today;
            mockKLEStandardRepository
                .Setup(r => r.UpdateKLE(userId, activeOrganizationId))
                .Returns(publishedDate);
            mockKLEStandardRepository.Setup(r => r.GetKLEStatus(It.IsAny<DateTime>())).Returns(
                new KLEStatus
                {
                    UpToDate = true,
                    Published = publishedDate
                }
            );
            var mockUpdateHistoryItemRepository = new Mock<IKLEUpdateHistoryItemRepository>();
            // Act
            var sut = new KLEApplicationService(mockOrganizationalUserContext.Object, mockKLEStandardRepository.Object, mockUpdateHistoryItemRepository.Object);
            var result = sut.UpdateKLE();
            // Assert
            Assert.False(result.Ok);
            mockUpdateHistoryItemRepository.Verify(r => r.Insert(publishedDate, userId), Times.Never());
        }
    }
}
