using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.KLE;
using Core.DomainModel.KLE;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Repositories.TaskRefs;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public class KLEApplicationServiceTest : WithAutoFixture
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
                new KLEChange { ChangeType = KLEChangeType.Added, TaskKey = A<string>(), UpdatedDescription = A<string>()}
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
            _mockKleStandardRepository.Setup(r => r.GetKLEStatus(It.IsAny<Maybe<DateTime>>())).Returns(
                new KLEStatus
                {
                    UpToDate = false,
                    Published = publishedDate
                }
            );
            _mockKleStandardRepository.Setup(r => r.GetKLEChangeSummary()).Returns(new List<KLEChange>
            {
                new KLEChange { ChangeType = KLEChangeType.Added, TaskKey = A<string>(), UpdatedDescription = A<string>()}
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
            _mockKleStandardRepository.Setup(r => r.GetKLEStatus(It.IsAny<Maybe<DateTime>>())).Returns(
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

        [Fact]
        public void SearchKle_Returns_Filtered_Kle()
        {
            //Arrange
            var expectedResult = new TaskRef[] { new(), new(), new() }.AsQueryable();
            var filter = new Mock<IDomainQuery<TaskRef>>();
            _taskRefRepositoryMock.Setup(x => x.Query(filter.Object)).Returns(expectedResult);
            Maybe<DateTime> updatedAt = A<DateTime>();
            _mockUpdateHistoryItemRepository.Setup(x => x.GetLastUpdated()).Returns(updatedAt);

            //Act
            var result = _sut.SearchKle(filter.Object);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(expectedResult, result.Value.contents);
            Assert.Equal(updatedAt, result.Value.updateReference);
        }

        [Fact]
        public void GetKle_Returns_Ok()
        {
            //Arrange
            var uuid = A<Guid>();
            TaskRef expectedMatch = new() { Uuid = uuid };
            var all = new[] { new(), expectedMatch, new() }.AsQueryable();
            Maybe<DateTime> updatedAt = A<DateTime>();
            _taskRefRepositoryMock.Setup(x => x.Query()).Returns(all);
            _mockUpdateHistoryItemRepository.Setup(x => x.GetLastUpdated()).Returns(updatedAt);

            //Act
            var result = _sut.GetKle(uuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Same(expectedMatch, result.Value.kle);
            Assert.Equal(updatedAt, result.Value.updateReference);
        }

        [Fact]
        public void GetKle_Returns_NotFound()
        {
            //Arrange
            var all = new TaskRef[] { new(), new() }.AsQueryable();
            Maybe<DateTime> updatedAt = A<DateTime>();
            _taskRefRepositoryMock.Setup(x => x.Query()).Returns(all);
            _mockUpdateHistoryItemRepository.Setup(x => x.GetLastUpdated()).Returns(updatedAt);

            //Act
            var result = _sut.GetKle(A<Guid>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }
    }
}
