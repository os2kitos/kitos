using Core.Abstractions.Types;
using Core.ApplicationServices.Users;
using Core.DomainModel;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using Moq;
using System;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Users
{
    public class UserLoggedInServiceTest: WithAutoFixture
    {
        private Mock<IUserRepository> _userRepository;
        private Mock<ITransactionManager> _transactionManager;
        private UserLoggedInService _sut;

        public UserLoggedInServiceTest()
        {
            _userRepository = new Mock<IUserRepository>();
            _transactionManager = new Mock<ITransactionManager>();
            _sut = new UserLoggedInService(_userRepository.Object, _transactionManager.Object);

        }

        [Fact]
        public void UpdateLatestLoginWithoutAuthenticating_Saves_Timestamp_On_User()
        {
            var now = DateTime.UtcNow;
            var uuid = A<Guid>();
            var user = new User();
            _userRepository.Setup(x => x.GetByUuid(uuid)).Returns(user);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var errorMaybe = _sut.UpdateLatestLoginWithoutAuthenticating(uuid, now);

            Assert.Equal(now, user.LastLogin);
            Assert.True(errorMaybe.IsNone);
            _userRepository.Verify(x => x.Update(user));
            _userRepository.Verify(x => x.Save());
            transaction.Verify(x => x.Commit());
        }

        [Fact]
        public void UpdateLatestLoginWithoutAuthenticating_Propagates_Error_If_User_Not_Found()
        {
            var now = DateTime.UtcNow;
            var uuid = A<Guid>();
            var user = new User();
            _userRepository.Setup(x => x.GetByUuid(uuid)).Returns(Maybe<User>.None);

            var errorMaybe = _sut.UpdateLatestLoginWithoutAuthenticating(uuid, now);

            Assert.True(errorMaybe.HasValue);
            Assert.Equal(OperationFailure.NotFound, errorMaybe.Value.FailureType);
            Assert.Null(user.LastLogin);
        }
    }
}
