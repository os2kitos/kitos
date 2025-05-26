using System.Linq;
using AutoFixture;
using Core.DomainModel;
using Core.DomainServices.Queries.User;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.DomainServices
{
    public class QueryUserByNameOrEmailTest : WithAutoFixture
    {
        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            var outer = new Fixture();
            fixture.Register(() => new User()
            {
                Name = outer.Create<string>(),
                LastName = outer.Create<string>(),
                Email = outer.Create<string>()
            });
        }

        [Theory]
        [InlineData("fullFirstName", "fullFirstName", true)]
        [InlineData("fullFirstName", "fullFirst", true)]
        [InlineData("fullFirstName", "full", true)]
        [InlineData("fullFirstName", "fuld", false)]
        public void Will_Match_On_FirstName(string correctFirstName, string query, bool expectMatch)
        {
            //Arrange
            var matchUser = A<User>();
            matchUser.Name = correctFirstName;
            var inputQuery = Many<User>().Concat(new[] { matchUser }).AsQueryable();
            var sut = new QueryUserByNameOrEmail(query);

            //Act
            var result = sut.Apply(inputQuery).ToList();

            //Assert
            Assert.Equal(expectMatch, result.Any());
            if (expectMatch)
                Assert.Same(matchUser, result.FirstOrDefault());
        }

        [Theory]
        [InlineData("fullLastName", "fullLastName", true)]
        [InlineData("fullLastName", "fullLast", true)]
        [InlineData("fullLastName", "full", true)]
        [InlineData("fullLastName", "fuld", false)]
        public void Will_Match_On_LastName(string correctLastName, string query, bool expectMatch)
        {
            //Arrange
            var matchUser = A<User>();
            matchUser.LastName = correctLastName;
            var inputQuery = Many<User>().Concat(new[] { matchUser }).AsQueryable();
            var sut = new QueryUserByNameOrEmail(query);

            //Act
            var result = sut.Apply(inputQuery).ToList();

            //Assert
            Assert.Equal(expectMatch, result.Any());
            if (expectMatch)
                Assert.Same(matchUser, result.FirstOrDefault());
        }

        [Theory]
        [InlineData("fullEmail@email.com", "fullEmail@email.com", true)]
        [InlineData("fullEmail@email.com", "fullEmail@", true)]
        [InlineData("fullEmail@email.com", "full", true)]
        [InlineData("fullEmail@email.com", "fuld", false)]
        public void Will_Match_On_Email(string email, string query, bool expectMatch)
        {
            //Arrange
            var matchUser = A<User>();
            matchUser.Email = email;
            var inputQuery = Many<User>().Concat(new[] { matchUser }).AsQueryable();
            var sut = new QueryUserByNameOrEmail(query);

            //Act
            var result = sut.Apply(inputQuery).ToList();

            //Assert
            Assert.Equal(expectMatch, result.Any());
            if (expectMatch)
                Assert.Same(matchUser, result.FirstOrDefault());
        }

        [Theory]
        [InlineData("email@kitos.dk", "os2", "kitos", "email@kitos.dk os2 kitos", true)]
        [InlineData("email@kitos.dk", "os2", "kitos", "email@kito os kit", true)]
        [InlineData("email@kitos.dk", "os2", "kitos", "os kit", true)]
        [InlineData("email@kitos.dk", "os2", "kitos", "kit", true)]
        [InlineData("email@kitos.dk", "os2", "kitos", "indberetning", false)]
        [InlineData("email@kitos.dk", "os2", "kitos", "email@kitos.dk os2 kitt", false)]
        [InlineData("email@kitos.dk", "os2", "kitos", "email@kitos.dk os3 kitos", false)]
        [InlineData("email@kitos.dk", "os2", "kitos", "email@kitos.com os2 kitos", false)]
        public void Will_Match_On_First_Last_And_Email(string email, string firstName, string lastName, string query, bool expectMatch)
        {
            //Arrange
            var matchUser = A<User>();
            matchUser.Email = email;
            matchUser.Name = firstName;
            matchUser.LastName = lastName;
            var inputQuery = Many<User>().Concat(new[] { matchUser }).AsQueryable();
            var sut = new QueryUserByNameOrEmail(query);

            //Act
            var result = sut.Apply(inputQuery).ToList();

            //Assert
            Assert.Equal(expectMatch, result.Any());
            if (expectMatch)
                Assert.Same(matchUser, result.FirstOrDefault());
        }
    }
}
