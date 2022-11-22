using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Core.ApplicationServices.ScheduledJobs;
using Core.BackgroundJobs.Model.Maintenance;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Time;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.BackgroundJobs
{
    public class ScheduleFkOrgUpdatesBackgroundJobTest : WithAutoFixture
    {
        private readonly ScheduleFkOrgUpdatesBackgroundJob _sut;
        private readonly Mock<IHangfireApi> _hangfireApiMock;
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IOperationClock> _operationClockMock;
        private readonly DateTime _now;

        public ScheduleFkOrgUpdatesBackgroundJobTest()
        {
            _hangfireApiMock = new Mock<IHangfireApi>();
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _now = DateTime.UtcNow;
            _operationClockMock = new Mock<IOperationClock>();
            _operationClockMock.Setup(x => x.Now).Returns(_now);
            _sut = new ScheduleFkOrgUpdatesBackgroundJob(
                _hangfireApiMock.Object,
                _organizationRepositoryMock.Object,
                Mock.Of<ILogger>(),
                null,
                null,
                null,
                null,
                null,
                _operationClockMock.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_Enqueues_Update_Jobs_For_Subscribing_Organizations()
        {
            //Arrange
            var expectedResult1 = CreateOrganization(true, true);
            var expectedResult2 = CreateOrganization(true, true);
            var expectedResult3 = CreateOrganization(true, true);
            var expectedResult4 = CreateOrganization(true, true);

            var organizations = new[]
            {
                expectedResult1,
                CreateOrganization(true, false),
                CreateOrganization(false, false),
                expectedResult2,
                expectedResult3,
                expectedResult4
            };
            _organizationRepositoryMock.Setup(x => x.GetAll()).Returns(organizations.AsQueryable());

            //Act
            var result = await _sut.ExecuteAsync();

            //Assert
            Assert.True(result.Ok);
            _hangfireApiMock.Verify(x => x.Schedule(It.IsAny<Expression<Action>>(), It.IsAny<DateTimeOffset>()), Times.Exactly(4));
            VerifyJobScheduledAt(expectedResult1, _now); //first one runs immediately
            VerifyJobScheduledAt(expectedResult2,_now.AddMinutes(1)); //next two are scheduled to run in parallel 1 minute from now
            VerifyJobScheduledAt(expectedResult3, _now.AddMinutes(1));
            VerifyJobScheduledAt(expectedResult4, _now.AddMinutes(2)); //fourth is pushed another minute
        }

        private void VerifyJobScheduledAt(Organization expectedResult1, DateTime expectedStart)
        {
            _hangfireApiMock.Verify(x => x.Schedule(It.Is<Expression<Action>>(expr => MatchExpectedJobCall(expr, expectedResult1)), expectedStart), Times.Once);
        }

        public Organization CreateOrganization(bool connected, bool subscribing)
        {
            var stsOrganizationConnection = connected || subscribing
                ? new StsOrganizationConnection
                {
                    Connected = connected,
                    SubscribeToUpdates = subscribing
                }
                : null;

            return new Organization
            {
                Id = A<int>(),
                StsOrganizationConnection = stsOrganizationConnection,
            };
        }

        private static bool MatchExpectedJobCall(Expression<Action> jobCall, Organization expectedResult1)
        {
            var body = jobCall.Body as MethodCallExpression;
            Assert.NotNull(body);
            dynamic bodyArgument = body.Arguments[0];
            ConstantExpression arg = bodyArgument.Expression;
            var actualOrgId = arg.Value;
            var actualUuid = (Guid)actualOrgId.GetType().GetField("uuid").GetValue(actualOrgId);

            return actualUuid == expectedResult1.Uuid;
        }
    }
}
