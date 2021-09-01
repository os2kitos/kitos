using AutoFixture;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries;
using Moq;
using System;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{

    public interface IQueryByOrganizationUuidSubject : IOwnedByOrganization { }

    public class QueryByOrganizationUuidTest : WithAutoFixture
    {
        protected override void OnFixtureCreated(Fixture fixture)
        {
            fixture.Register(() => new Organization { Id = fixture.Create<int>(), Uuid = fixture.Create<Guid>() });
            base.OnFixtureCreated(fixture);
        }

        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctOrg = A<Organization>();
            var incorrectOrg = A<Organization>();
            var matched = Mock.Of<IQueryByOrganizationUuidSubject>(x => x.Organization == correctOrg);
            var excluded = Mock.Of<IQueryByOrganizationUuidSubject>(x => x.Organization == incorrectOrg);

            var input = new[] { matched, excluded }.AsQueryable();
            var sut = new QueryByOrganizationUuid<IQueryByOrganizationUuidSubject>(correctOrg.Uuid);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Same(matched, itSystem);
        }
    }
}
