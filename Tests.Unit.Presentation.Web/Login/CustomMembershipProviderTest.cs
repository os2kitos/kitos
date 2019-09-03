using System;
using Core.DomainModel;
using Core.DomainServices;
using Infrastructure.Services.Cryptography;
using Ninject.Extensions.Logging;
using Presentation.Web.Infrastructure;
using Xunit;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Unit.Presentation.Web.Login
{
    public class CustomMembershipProviderTest
    {
        public CustomMembershipProvider CustomMembershipProviderMock { get; set; }

        public User MockUser { get; set; } = new User();

        public CustomMembershipProviderTest()
        {
            // Setting up the necessary mocks
            CustomMembershipProviderMock = new CustomMembershipProvider()
            {
                UserRepositoryFactory = Substitute.For<IUserRepositoryFactory>(),
                CryptoService = Substitute.For<ICryptoService>(),
                Logger = Substitute.For<ILogger>()
            };
        }

        [Fact]
        public void Should_Validate_User()
        {
            // Arrange
            // Get an existing user
            CustomMembershipProviderMock.UserRepositoryFactory.GetUserRepository().GetByEmail("existingUser@kitos.dk").Returns(MockUser);
            // Set user password
            MockUser.Password = "thePassword";
            // Helper method CheckPassord in ValidateUser -> Validate returns true if the password passed to ValidateUser is equal to the user objects password property
            CustomMembershipProviderMock.CryptoService.Encrypt("").ReturnsForAnyArgs("thePassword");

            // Act
            // Input existing user with valid password
            var success = CustomMembershipProviderMock.ValidateUser("existingUser@kitos.dk", "thePassword");

            // Assert
            Assert.True(success);
        }

        [Fact]
        public void Should_Not_Validate_User()
        {
            // Arrange
            // Get a non-existing user
            // The user repository returns null if no user is found
            CustomMembershipProviderMock.UserRepositoryFactory.GetUserRepository()
                .GetByEmail("nonExistingUser@kitos.dk")
                .ReturnsNull();

            // Act
            // Input non-existing user and some password
            var success = CustomMembershipProviderMock.ValidateUser("existingUser@kitos.dk", "somePassword");

            // Assert
            Assert.False(success);
        }

        [Fact]
        public void Should_Increment_FailedAttempts()
        {
            // Arrange
            // Failed attempts are zero a the outset
            var previousFailedAttempts = MockUser.FailedAttempts;
            // Make sure the password validation returns false. The password passed to ValidateUser must not match "notTheUsersPassword".
            CustomMembershipProviderMock.CryptoService.Encrypt("").ReturnsForAnyArgs("notTheUsersPassword");

            // Act
            // Trigger increment with incorrect login information
            CustomMembershipProviderMock.ValidateUser("support@kitos.dk", "incorrectPassword");

            // Assert
            // PreviousFailedAttempts < MockUser.FailedAttempts < previousFailedAttempts + 2
            Assert.InRange(MockUser.FailedAttempts, previousFailedAttempts, previousFailedAttempts + 2);
        }

        [Fact]
        public void Should_Lockout_User()
        {
            // Arrange
            // Get an existing user
            CustomMembershipProviderMock.UserRepositoryFactory.GetUserRepository().GetByEmail("existingUser@kitos.dk").Returns(MockUser);
            // Next login attempt should trigger a lockout when failed attempts >= 5.
            MockUser.FailedAttempts = 4;
            // The user was previously not locked and LockedOutDate is therefore null
            MockUser.LockedOutDate = null;
            // Make sure the password validation returns false
            CustomMembershipProviderMock.CryptoService.Encrypt("").ReturnsForAnyArgs("notTheUsersPassword");

            // Act
            // Trigger lockout by passing incorrect login information
            CustomMembershipProviderMock.ValidateUser("existingUser@kitos.dk", "incorrectPassword");

            // Assert
            // MockUser is locked out if it has an LockedOutDate
            Assert.True(MockUser.LockedOutDate != null);
        }

        [Fact]
        public void Should_Not_Allow_Login_Attempts()
        {
            // Arrange
            // Get an existing user
            CustomMembershipProviderMock.UserRepositoryFactory.GetUserRepository().GetByEmail("existingUser@kitos.dk").Returns(MockUser);

            // Act
            // Input existing user with valid password
            CustomMembershipProviderMock.CryptoService.Encrypt("").ReturnsForAnyArgs("thePassword");
            // User is lock out if User.LockedOutDate != null
            MockUser.LockedOutDate = new DateTime(2016, 08, 22, 9, 00, 00);
            var success = CustomMembershipProviderMock.ValidateUser("existingUser@kitos.dk", "thePassword");

            // Assert
            Assert.False(success);
        }
    }
}
