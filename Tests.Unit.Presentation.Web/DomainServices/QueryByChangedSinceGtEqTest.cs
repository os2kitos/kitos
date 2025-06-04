using System;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices.Queries;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByChangedSinceGtEqTest : WithAutoFixture
    {
        [Fact]
        public void Apply_Includes_Results_With_LastChanged_EqualToOrGreaterThan()
        {
            //Arrange
            var referenceTime = A<DateTime>().ToUniversalTime();
            var sut = new QueryByChangedSinceGtEq<Entity>(referenceTime);

            var tooOld = CreateEntity(referenceTime.AddTicks(-1));
            var equalTo = CreateEntity(referenceTime);
            var greaterThan = CreateEntity(referenceTime.AddTicks(1));

            //Act
            var result = sut.Apply(new[] { tooOld, equalTo, greaterThan }.AsQueryable());

            //Assert
            Assert.Equal(new[] { equalTo, greaterThan }, result);
        }

        private static Entity CreateEntity(DateTime lastModified)
        {
            var entity = Mock.Of<Entity>();
            entity.LastChanged = lastModified;
            return entity;
        }
    }
}
