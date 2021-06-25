using Core.DomainModel;
using Core.DomainServices.Queries.UserQueries;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices.Users
{
    public class QueryByApiOrStakeHolderAccessTest : WithAutoFixture
    {

        [Fact]
        public void Apply_Returns_Users_With_StakeHolder_Or_Api_Access()
        {
            //Arrange
            var userWithStakeHolder = new User() { Id = A<int>(), HasStakeHolderAccess = true };
            var userWithApi = new User() { Id = A<int>(), HasApiAccess = true };
            var userWithStakeHolderAndApi = new User() { Id = A<int>(), HasStakeHolderAccess = true, HasApiAccess = true };
            var userWithNothing = new User() { Id = A<int>() };

            var input = new[] { userWithStakeHolder, userWithApi, userWithNothing, userWithStakeHolderAndApi }.AsQueryable();
            var sut = new QueryByApiOrStakeHolderAccess();

            //Act
            var result = sut.Apply(input);

            //Assert
            Assert.Equal(3, result.Count());

            var userWithStakeHolderResult = result.First(x => x.Id == userWithStakeHolder.Id);
            Assert.True(userWithStakeHolderResult.HasStakeHolderAccess);
            Assert.Null(userWithStakeHolderResult.HasApiAccess); // HasApiAccess is nullable

            var userWithApiResult = result.First(x => x.Id == userWithApi.Id);
            Assert.False(userWithApiResult.HasStakeHolderAccess);
            Assert.True(userWithApiResult.HasApiAccess);

            var userWithStakeHolderAndApiResult = result.First(x => x.Id == userWithStakeHolderAndApi.Id);
            Assert.True(userWithStakeHolderAndApiResult.HasStakeHolderAccess);
            Assert.True(userWithStakeHolderAndApiResult.HasApiAccess);
        }
    }
}
