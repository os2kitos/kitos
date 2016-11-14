using System;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Ninject.Extensions.Logging;
using Presentation.Web.Infrastructure;
using Xunit;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Presentation.Web.Controllers.OData.OptionControllers;

namespace Tests.Unit.Presentation.Web.RolesAndTypesPriorityTest
{
    public class RolesAndTypesPriorityTest
    {
        private readonly IGenericRepository<ItProjectRole> _itProjectRoleRepository;
        private readonly IAuthenticationService _iAuthenticationService;
        private readonly ItProjectRolesController _itProjectRolesController;

        public RolesAndTypesPriorityTest()
        {
            _itProjectRoleRepository = Substitute.For<IGenericRepository<ItProjectRole>>();
            _iAuthenticationService = Substitute.For<IAuthenticationService>();
            _itProjectRolesController = new ItProjectRolesController(_itProjectRoleRepository, _iAuthenticationService);
        }

        [Fact]
        public void Priority_should_increment_by_one()
        {
            //Arrange
            //Act
            //Assert
        }

        [Fact]
        public void Priority_should_decrement_by_one()
        {
            //Arrange
            //Act
            //Assert
        }

        //public CustomMembershipProvider CustomMembershipProviderMock { get; set; }
        //public User MockUser { get; set; } = new User();
        //public RolesAndTypesPriorityTest()
        //{
        //    // Setting up the necessary mocks
        //    CustomMembershipProviderMock = new CustomMembershipProvider()
        //    {
        //        UserRepositoryFactory = Substitute.For<IUserRepositoryFactory>(),
        //        CryptoService = Substitute.For<ICryptoService>(),
        //        Logger = Substitute.For<ILogger>()
        //    };
        //}

        //[Fact]
        //public void Should_Validate_User()
        //{
        //    // Arrange
        //    // Get an existing user
        //    CustomMembershipProviderMock.UserRepositoryFactory.GetUserRepository().GetByEmail("existingUser@kitos.dk").Returns(MockUser);
        //    // Set user password
        //    MockUser.Password = "thePassword";
        //    // Helper method CheckPassord in ValidateUser -> Validate returns true if the password passed to ValidateUser is equal to the user objects password property
        //    CustomMembershipProviderMock.CryptoService.Encrypt("").ReturnsForAnyArgs("thePassword");

        //    // Act
        //    // Input existing user with valid password
        //    var success = CustomMembershipProviderMock.ValidateUser("existingUser@kitos.dk", "thePassword");

        //    // Assert
        //    Assert.True(success);
        //}
    }
}
