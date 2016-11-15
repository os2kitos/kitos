using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using NSubstitute;
using Presentation.Web.Controllers.OData.OptionControllers;
using Xunit;

namespace Tests.Unit.Presentation.Web.OData
{
    public class RolesAndTypesPriorityTest
    {
        private readonly IGenericRepository<ItProjectType> _itProjectTypeMockRepository;
        private readonly IAuthenticationService _iAuthenticationServiceMock;
        private readonly ItProjectTypesController _itProjectTypesMockController;
        private ItProjectType MockProjectType { get; set; } = new ItProjectType();

        public RolesAndTypesPriorityTest()
        {
            _itProjectTypeMockRepository = Substitute.For<IGenericRepository<ItProjectType>>();
            _iAuthenticationServiceMock = Substitute.For<IAuthenticationService>();
            _itProjectTypesMockController = new ItProjectTypesController(_itProjectTypeMockRepository, _iAuthenticationServiceMock);
        }

        [Fact]
        public void Priority_should_increment_by_one()
        {
            //Arrange
            // Get project type
            //_itProjectTypesMockController.GetByOrganizationKey(1).Returns(MockProjectType);
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
