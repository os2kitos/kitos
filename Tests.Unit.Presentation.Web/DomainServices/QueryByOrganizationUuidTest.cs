using Core.DomainModel;
using Core.DomainServices.Queries;
using Moq;
using System;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryByOrganizationUuidTest : WithAutoFixture
    {
        public interface IQueryByOrganizationUuidSubject : IOwnedByOrganization { }

        [Fact]
        public void Apply_Returns_Items_With_Id_Match()
        {
            //Arrange
            var correctId = A<Guid>();
            var incorrectId = A<Guid>();
            var matched = Mock.Of<IQueryByOrganizationUuidSubject>(x => x.Organization.Uuid == correctId);
            var excluded = Mock.Of<IQueryByOrganizationUuidSubject>(x => x.Organization.Uuid == incorrectId);

            var input = new[] { matched, excluded }.AsQueryable();
            var sut = new QueryByOrganizationUuid<IQueryByOrganizationUuidSubject>(correctId);

            //Act
            var result = sut.Apply(input);

            //Assert
            var itSystem = Assert.Single(result);
            Assert.Same(matched, itSystem);
        }
    }
}
