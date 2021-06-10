using System;
using System.Linq;
using AutoFixture;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public interface IQueryByUuidSubject : IHasUuid {}

    public class QueryByUuidTest : WithAutoFixture
    {
        protected override void OnFixtureCreated(Fixture fixture)
        {
            fixture.Register(() => new ItSystem { Id = fixture.Create<int>() });
            base.OnFixtureCreated(fixture);
        }

        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = Mock.Of<IQueryByUuidSubject>(x=>x.Uuid == correctId);
            var excluded = Mock.Of<IQueryByUuidSubject>(x => x.Uuid == incorrectId);

            var input = new[] { matched, excluded }.AsQueryable();
            var sut = new QueryByUuid<IQueryByUuidSubject>(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Same(matched, itSystem);
        }
    }
}
