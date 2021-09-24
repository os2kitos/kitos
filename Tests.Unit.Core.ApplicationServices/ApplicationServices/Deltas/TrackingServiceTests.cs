using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Tracking;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainModel.Tracking;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Moq;
using Tests.Toolkit.Extensions;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Deltas
{
    public class TrackingServiceTests : WithAutoFixture
    {
        private readonly TrackingService _sut;
        private readonly Mock<IGenericRepository<LifeCycleTrackingEvent>> _repositoryMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;
        private readonly Mock<IOrganizationalUserContext> _organizationUserContextMock;

        public TrackingServiceTests()
        {
            _repositoryMock = new Mock<IGenericRepository<LifeCycleTrackingEvent>>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _organizationUserContextMock = new Mock<IOrganizationalUserContext>();
            _sut = new TrackingService(_repositoryMock.Object, _authorizationContextMock.Object, _organizationUserContextMock.Object);
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            var outer = new Fixture();
            fixture.Inject<AccessModifier?>(AccessModifier.Local);
            fixture.Register(() => new Organization
            {
                Id = outer.Create<int>(),
                Uuid = outer.Create<Guid>()
            });
        }

        [Fact]
        public void Can_Query_With_No_Refinements_As_Full_CrossLevelAccess()
        {
            //Arrange
            var all = Many<LifeCycleTrackingEvent>().AsQueryable();
            _repositoryMock.Setup(x => x.AsQueryable()).Returns(all);
            ExpectCrossLevelOrganizationReadAccess(CrossOrganizationDataReadAccessLevel.All);

            //Act
            var result = _sut.QueryLifeCycleEvents();

            //Assert
            Assert.Same(all, result);
        }

        [Fact]
        public void Can_Query_With_No_Refinements_As_Public_CrossLevelAccess()
        {
            //Arrange
            var uniqueIds = CreateUniqueIds(12).ToList();
            var orgIds = uniqueIds.Take(2).ToList();
            var idsToSetOnFullSet = new Stack<int>(uniqueIds.Skip(2));
            var all = Many<LifeCycleTrackingEvent>(10).AsQueryable();

            all.ToList().ForEach(x => x.OptionalOrganizationReference.Id = idsToSetOnFullSet.Pop());
            all.Take(2).ToList().ForEach(x => x.OptionalOrganizationReference.Id = orgIds.First());
            all.Skip(2).Take(2).ToList().ForEach(x => x.OptionalOrganizationReference.Id = orgIds.Last());
            all.Skip(6).Take(2).ToList().ForEach(x => x.OptionalAccessModifier = AccessModifier.Public);

            _organizationUserContextMock.Setup(x => x.OrganizationIds).Returns(orgIds);
            _repositoryMock.Setup(x => x.AsQueryable()).Returns(all);
            ExpectCrossLevelOrganizationReadAccess(CrossOrganizationDataReadAccessLevel.Public);

            //Act
            var result = _sut.QueryLifeCycleEvents();

            //Assert - first 4 by org id relation, then two are filtered off due to visibility, then two are included because they are shared
            Assert.Equal(all.Take(4).Concat(all.Skip(6).Take(2)), result);
        }

        [Fact]
        public void Can_Query_With_No_Refinements_As_No_CrossLevelAccess()
        {
            //Arrange
            var uniqueIds = CreateUniqueIds(12).ToList();
            var orgIds = uniqueIds.Take(2).ToList();
            var idsToSetOnFullSet = new Stack<int>(uniqueIds.Skip(2));
            var all = Many<LifeCycleTrackingEvent>(10).AsQueryable();

            all.ToList().ForEach(x => x.OptionalOrganizationReference.Id = idsToSetOnFullSet.Pop());
            all.Take(1).ToList().ForEach(x => x.OptionalOrganizationReference.Id = orgIds.First());
            all.Skip(1).Take(1).ToList().ForEach(x => x.OptionalOrganizationReference.Id = orgIds.Last());
            all.Skip(2).Take(1).ToList().ForEach(x => x.OptionalAccessModifier = AccessModifier.Public);

            _organizationUserContextMock.Setup(x => x.OrganizationIds).Returns(orgIds);
            _repositoryMock.Setup(x => x.AsQueryable()).Returns(all);
            ExpectCrossLevelOrganizationReadAccess(CrossOrganizationDataReadAccessLevel.None);

            //Act
            var result = _sut.QueryLifeCycleEvents();

            //Assert - first 2 by org id relation, - nothing else - not even the shared item at index 2
            Assert.Equal(all.Take(2), result);
        }

        [Fact]
        public void Can_Query_With_No_Refinements_As_RightsHolderAccess()
        {
            //Arrange
            var uniqueIds = CreateUniqueIds(13).ToList();
            var orgIds = uniqueIds.Take(2).ToList();
            var rightsHolderOrg = uniqueIds.Skip(2).First();
            var idsToSetOnFullSet = new Stack<int>(uniqueIds.Skip(3));
            var all = Many<LifeCycleTrackingEvent>(10).AsQueryable();

            all.ToList().ForEach(x =>
            {
                var id = idsToSetOnFullSet.Pop();
                x.OptionalOrganizationReference.Id = id;
                x.OptionalRightsHolderOrganization.Id = id;
            });
            all.Take(1).ToList().ForEach(x => x.OptionalOrganizationReference.Id = orgIds.First());
            all.Skip(1).Take(1).ToList().ForEach(x => x.OptionalOrganizationReference.Id = orgIds.Last());
            all.Skip(2).Take(1).ToList().ForEach(x => x.OptionalAccessModifier = AccessModifier.Public);
            all.Skip(3).Take(1).ToList().ForEach(x => x.OptionalRightsHolderOrganization.Id = rightsHolderOrg);

            _organizationUserContextMock.Setup(x => x.OrganizationIds).Returns(orgIds);
            _organizationUserContextMock.Setup(x => x.GetOrganizationIdsWhereHasRole(OrganizationRole.RightsHolderAccess)).Returns(new[] { rightsHolderOrg });
            _repositoryMock.Setup(x => x.AsQueryable()).Returns(all);
            ExpectCrossLevelOrganizationReadAccess(CrossOrganizationDataReadAccessLevel.RightsHolder);

            //Act
            var result = _sut.QueryLifeCycleEvents();

            //Assert - first 2 by org id relation, then at index 3 the rights holder org
            Assert.Equal(all.Take(2).Concat(all.Skip(3).Take(1)), result);
        }


        public static IEnumerable<object[]> GetTrackedEntityTypes() => EnumRange.All<TrackedEntityType>().Select(x => new object[] { x });

        [Theory, MemberData(nameof(GetTrackedEntityTypes))]
        public void Can_Query_With_EntityTypeFilter(TrackedEntityType filterBy)
        {
            //Arrange
            var all = EnumRange.All<TrackedEntityType>().Select(eventType =>
            {
                var lifeCycleTrackingEvent = A<LifeCycleTrackingEvent>();
                lifeCycleTrackingEvent.EntityType = eventType;
                return lifeCycleTrackingEvent;
            }).ToList().AsQueryable();
            _repositoryMock.Setup(x => x.AsQueryable()).Returns(all);
            ExpectCrossLevelOrganizationReadAccess(CrossOrganizationDataReadAccessLevel.All);

            //Act
            var result = _sut.QueryLifeCycleEvents(trackedEntityType: filterBy);

            //Assert
            var trackingEvent = Assert.Single(result);
            Assert.Equal(filterBy, trackingEvent.EntityType);
        }

        [Fact]
        public void Can_Query_With_DeletedSinceFilter()
        {
            //Arrange
            const int howMany = 10;
            var all = Many<LifeCycleTrackingEvent>(howMany).ToList();
            var referenceDate = A<DateTime>().ToUniversalTime();
            var changedDates = Enumerable.Range(0, howMany).Select(x => referenceDate.AddMilliseconds(x)).ToList().Transform(changedDates => new Stack<DateTime>(changedDates));
            all.RandomItems(howMany).ToList().ForEach(trackingEvent => trackingEvent.OccurredAtUtc = changedDates.Pop());
            var filterByDateTime = all.RandomItem().OccurredAtUtc;
            _repositoryMock.Setup(x => x.AsQueryable()).Returns(all.AsQueryable());
            ExpectCrossLevelOrganizationReadAccess(CrossOrganizationDataReadAccessLevel.All);

            //Act
            var result = _sut.QueryLifeCycleEvents(since: filterByDateTime);

            //Assert
            Assert.Equal(all.Where(x => x.OccurredAtUtc >= filterByDateTime).ToList(), result.ToList());
        }

        private IEnumerable<int> CreateUniqueIds(int toCreate)
        {
            var hashSet = new HashSet<int>();
            do
            {
                hashSet.Add(A<int>());
            } while (hashSet.Count < toCreate);

            return hashSet.ToList();
        }

        private void ExpectCrossLevelOrganizationReadAccess(CrossOrganizationDataReadAccessLevel value)
        {
            _authorizationContextMock.Setup(x => x.GetCrossOrganizationReadAccess())
                .Returns(value);
        }
    }
}
