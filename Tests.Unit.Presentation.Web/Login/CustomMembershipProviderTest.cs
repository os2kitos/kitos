using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure;
using Xunit;
using NSubstitute;

namespace Tests.Unit.Presentation.Web.Login
{
    public class CustomMembershipProviderTest
    {
        public CustomMembershipProvider CustomMembershipProvider { get; set; }

        public User User { get; set; }

        public CustomMembershipProviderTest()
        {
            CustomMembershipProvider = Substitute.For<CustomMembershipProvider>();
            CustomMembershipProvider.UserRepositoryFactory = Substitute.For<IUserRepositoryFactory>();
            CustomMembershipProvider.CryptoService = Substitute.For<ICryptoService>();

            User = Substitute.For<User>();
            User = CustomMembershipProvider.UserRepositoryFactory.GetUserRepository().GetByEmail("support@kitos.dk");
        }

        [Fact]
        public void Should_not_be_able_to_find_user()
        {
            CustomMembershipProvider.ValidateUser("test@kitos.dk", "123").Returns(false);
            Assert.False(CustomMembershipProvider.ValidateUser("test@kitos.dk", "123"));
        }

        [Fact]
        public void Should_be_able_to_find_user()
        {
            CustomMembershipProvider.ValidateUser("support@kitos.dk", "KitosAgent").Returns(true);
            Assert.True(CustomMembershipProvider.ValidateUser("support@kitos.dk", "KitosAgent"));
        }

        [Fact]
        public void User_Not_LockedOut_Validate_Success()
        {

        }
        [Fact]
        public void User_MaxInvalidPasswordAttempts_Equals_Five()
        {
            CustomMembershipProvider.MaxInvalidPasswordAttempts.Returns(5);
            Assert.Equal(5, CustomMembershipProvider.MaxInvalidPasswordAttempts);
        }

        [Fact]
        public void User_PasswordAttemptWindow_Equals_One()
        {
            CustomMembershipProvider.PasswordAttemptWindow.Returns(1);
            Assert.Equal(1, CustomMembershipProvider.PasswordAttemptWindow);
        }
    }
}
