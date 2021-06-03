//using Core.DomainModel.Notification;
//using Core.DomainServices;
//using Core.DomainServices.Extensions;
//using Core.DomainServices.Repositories.Notification;
//using Moq;
//using System.Linq;
//using Tests.Toolkit.Patterns;
//using Xunit;

//namespace Tests.Unit.Core.DomainServices.Repositories
//{
//    public class UserNotificationRepositoryTest : WithAutoFixture
//    {
//        private readonly Mock<IGenericRepository<UserNotification>> _repository;
//        private readonly UserNotificationRepository _sut;

//        public UserNotificationRepositoryTest()
//        {
//            _repository = new Mock<IGenericRepository<UserNotification>>();
//            _sut = new UserNotificationRepository(_repository.Object);
//        }

//        [Fact]
//        public void Can_Add()
//        {
//            //Arrange
//            var notification = new UserNotification();
//            _repository.Setup(x => x.Insert(notification)).Returns<UserNotification>(x => x);

//            //Act
//            var userNotification = _sut.Add(notification);

//            //Assert
//            Assert.Same(notification, userNotification);
//            VerifySaved();
//        }

//        [Fact]
//        public void Delete_Returns_True_If_Found_And_Deleted()
//        {
//            //Arrange
//            var id = A<int>();
//            var notification = new UserNotification();
//            _repository.Setup(x => x.GetByKey(id)).Returns(notification);

//            //Act
//            var deleted = _sut.DeleteById(id);

//            //Assert
//            _repository.Verify(x => x.Delete(notification), Times.Once);
//            Assert.True(deleted);
//            VerifySaved();
//        }

//        [Fact]
//        public void Can_Get_By_Id()
//        {
//            //Arrange
//            var id = A<int>();
//            var notification = new UserNotification();
//            _repository.Setup(x => x.GetByKey(id)).Returns(notification);

//            //Act
//            var userNotification = _sut.GetById(id);

//            //Assert
//            Assert.True(userNotification.HasValue);
//            Assert.Same(notification, userNotification.Value);
//        }

//        private void VerifySaved()
//        {
//            _repository.Verify(x => x.Save(), Times.Once);
//        }
//    }
//}
