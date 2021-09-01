using System;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Options;
using Core.DomainServices.Role;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Role
{
    public abstract class RoleAssignmentsServiceTest<TModel, TRight, TRole> : WithAutoFixture
        where TRight : Entity, IRight<TModel, TRight, TRole>
        where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
        where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
    {
        private readonly RoleAssignmentService<TRight, TRole, TModel> _sut;
        private readonly Mock<IOptionsService<TRight, TRole>> _optionsServiceMock;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IGenericRepository<TRight>> _rightRepositoryMock;

        public RoleAssignmentsServiceTest()
        {
            _optionsServiceMock = new Mock<IOptionsService<TRight, TRole>>();
            _userRepository = new Mock<IUserRepository>();
            _rightRepositoryMock = new Mock<IGenericRepository<TRight>>();
            _sut = new RoleAssignmentService<TRight, TRole, TModel>(
                _optionsServiceMock.Object,
                _userRepository.Object,
                _rightRepositoryMock.Object);
        }

        [Fact]
        public void Can_GetApplicableRoles()
        {
            //Arrange
            var model = CreateModel();
            var availableRoles = new[] { CreateRole(), CreateRole() };
            ExpectAvailableRoles(model, availableRoles);

            //Act
            var roles = _sut.GetApplicableRoles(model);

            //Assert
            Assert.Equal(availableRoles, roles);
        }

        [Fact]
        public void GetUsersWhichCanBeAssignedToRole_Returns_OrganizationUsers_Which_DoesNotAlreadyHaveTheRoleOnTheDpa()
        {
            //Arrange
            var roleId = A<int>();
            var excludedUserId = A<int>();
            var model = CreateModel((roleId, excludedUserId));
            Maybe<string> emailQuery = A<string>();
            var users = new[] { new User { Id = A<int>() }, new User { Id = A<int>() }, new User { Id = excludedUserId } };
            var agreementRole = CreateRole(roleId);
            ExpectAvailableOption(model, roleId, agreementRole);
            ExpectOrganizationUsers(model, emailQuery, users);

            //Act
            var result = _sut.GetUsersWhichCanBeAssignedToRole(model, roleId, emailQuery);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(2, result.Value.Count());
            Assert.Equal(users.Where(x => x.Id != excludedUserId), result.Value.ToList());
        }

        [Fact]
        public void Cannot_GetUsersWhichCanBeAssignedToRole_If_RoleId_Is_Invalid()
        {
            //Arrange
            var roleId = A<int>();
            var model = CreateModel();
            ExpectAvailableOption(model, roleId, Maybe<TRole>.None);

            //Act
            var result = _sut.GetUsersWhichCanBeAssignedToRole(model, roleId, A<string>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Can_AssignRole()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var model = CreateModel((A<int>(), userId));
            var users = new[] { new User { Id = A<int>() }, new User { Id = userId } };
            var agreementRole = CreateRole(roleId);
            ExpectAvailableOption(model, roleId, agreementRole);
            ExpectOrganizationUsers(model, Maybe<string>.None, users);

            //Act
            var result = _sut.AssignRole(model, roleId, userId);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(roleId, result.Value.Role.Id);
            Assert.Equal(userId, result.Value.User.Id);
        }

        [Fact]
        public void Cannot_AssignRole_If_RoleIs_Is_Invalid()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var model = CreateModel();
            ExpectAvailableOption(model, roleId, Maybe<TRole>.None);

            //Act
            var result = _sut.AssignRole(model, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignRole_If_User_Id_Is_Invalid()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var model = CreateModel();
            var users = new[] { new User { Id = A<int>() } }; //Target user is not included
            var agreementRole = CreateRole(roleId);
            ExpectAvailableOption(model, roleId, agreementRole);
            ExpectOrganizationUsers(model, Maybe<string>.None, users);

            //Act
            var result = _sut.AssignRole(model, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignRole_If_User_Already_Has_Role_On_Dpa()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var model = CreateModel((roleId, userId));
            var users = new[] { new User { Id = A<int>() }, new User { Id = userId } };
            var agreementRole = CreateRole(roleId);
            ExpectAvailableOption(model, roleId, agreementRole);
            ExpectOrganizationUsers(model, Maybe<string>.None, users);

            //Act
            var result = _sut.AssignRole(model, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Conflict, result.Error.FailureType);
        }

        [Fact]
        public void Can_RemoveRole()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var model = CreateModel((roleId, userId));
            var user = new User { Id = userId };
            var agreementRole = CreateRole(roleId);
            ExpectOption(model, roleId, agreementRole);
            ExpectGetUser(userId, user);

            //Act
            var result = _sut.RemoveRole(model, roleId, userId);

            //Assert
            Assert.True(result.Ok);
            var removedRight = result.Value;
            Assert.Equal(roleId, removedRight.RoleId);
            Assert.Equal(userId, removedRight.UserId);
            _rightRepositoryMock.Verify(x => x.Delete(removedRight), Times.Once);
        }

        [Fact]
        public void Cannot_RemoveRole_If_RoleId_Is_Invalid()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var model = CreateModel((roleId, userId));
            ExpectOption(model, roleId, Maybe<TRole>.None);

            //Act
            var result = _sut.RemoveRole(model, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            _rightRepositoryMock.Verify(x => x.Delete(It.IsAny<TRight>()), Times.Never);
        }

        [Fact]
        public void Cannot_RemoveRole_If_UserId_Is_Invalid()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var model = CreateModel((roleId, userId));
            var agreementRole = CreateRole(roleId);
            ExpectOption(model, roleId, agreementRole);
            ExpectGetUser(userId, null);

            //Act
            var result = _sut.RemoveRole(model, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            _rightRepositoryMock.Verify(x => x.Delete(It.IsAny<TRight>()), Times.Never);
        }

        [Fact]
        public void Cannot_RemoveRole_If_No_Existing_Assignment()
        {  
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var model = CreateModel((A<int>(), userId)); //Existing user role but different role
            var user = new User { Id = userId };
            var agreementRole = CreateRole(roleId);
            ExpectOption(model, roleId, agreementRole);
            ExpectGetUser(userId, user);

            //Act
            var result = _sut.RemoveRole(model, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            _rightRepositoryMock.Verify(x => x.Delete(It.IsAny<TRight>()), Times.Never);
        }

        [Fact]
        public void Can_AssignRole_Using_Uuid()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();
            var model = CreateModel((A<int>(), userId));
            var users = new[] { new User { Id = A<int>(), Uuid = A<Guid>()}, new User { Id = userId, Uuid = userUuid } };
            var agreementRole = CreateRole(roleId, roleUuid);
            ExpectAvailableOption(model, roleId, agreementRole);
            ExpectOrganizationUsers(model, Maybe<string>.None, users);
            ExpectGetUserByUuid(userUuid, users.First(x => x.Uuid == userUuid));
            ExpectGetRoleByUuid(model.OrganizationId, roleUuid, agreementRole);

            //Act
            var result = _sut.AssignRole(model, roleUuid, userUuid);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(roleId, result.Value.Role.Id);
            Assert.Equal(userId, result.Value.User.Id);
        }

        [Fact]
        public void Cannot_AssignRole_Using_Uuid_If_User_Does_Not_Exist()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();
            var model = CreateModel((A<int>(), userId));
            var users = new[] { new User { Id = A<int>(), Uuid = A<Guid>() }, new User { Id = userId, Uuid = userUuid } };
            var agreementRole = CreateRole(roleId, roleUuid);
            ExpectAvailableOption(model, roleId, agreementRole);
            ExpectOrganizationUsers(model, Maybe<string>.None, users);
            ExpectGetUserByUuid(userUuid, Maybe<User>.None);
            ExpectGetRoleByUuid(model.OrganizationId, roleUuid, agreementRole);

            //Act
            var result = _sut.AssignRole(model, roleUuid, userUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_AssignRole_Using_Uuid_If_Role_Does_Not_Exist()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();
            var model = CreateModel((A<int>(), userId));
            var users = new[] { new User { Id = A<int>(), Uuid = A<Guid>() }, new User { Id = userId, Uuid = userUuid } };
            var agreementRole = CreateRole(roleId, roleUuid);
            ExpectAvailableOption(model, roleId, agreementRole);
            ExpectOrganizationUsers(model, Maybe<string>.None, users);
            ExpectGetUserByUuid(userUuid, users.First(x => x.Uuid == userUuid));
            ExpectGetRoleByUuid(model.OrganizationId, roleUuid, Maybe<TRole>.None);

            //Act
            var result = _sut.AssignRole(model, roleUuid, userUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Can_RemoveRole_Using_Uuid()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();
            var model = CreateModel((roleId, userId));
            var user = new User { Id = userId, Uuid = userUuid };
            var agreementRole = CreateRole(roleId, roleUuid);
            ExpectOption(model, roleId, agreementRole);
            ExpectGetUser(userId, user);
            ExpectGetUserByUuid(userUuid, user);
            ExpectGetRoleByUuid(model.OrganizationId, roleUuid, agreementRole);

            //Act
            var result = _sut.RemoveRole(model, roleUuid, userUuid);

            //Assert
            Assert.True(result.Ok);
            var removedRight = result.Value;
            Assert.Equal(roleId, removedRight.RoleId);
            Assert.Equal(userId, removedRight.UserId);
            _rightRepositoryMock.Verify(x => x.Delete(removedRight), Times.Once);
        }

        [Fact]
        public void Can_RemoveRole_Using_Uuid_If_User_Does_Not_Exist()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();
            var model = CreateModel((roleId, userId));
            var user = new User { Id = userId, Uuid = userUuid };
            var agreementRole = CreateRole(roleId, roleUuid);
            ExpectOption(model, roleId, agreementRole);
            ExpectGetUser(userId, user);
            ExpectGetUserByUuid(userUuid, Maybe<User>.None);
            ExpectGetRoleByUuid(model.OrganizationId, roleUuid, agreementRole);

            //Act
            var result = _sut.RemoveRole(model, roleUuid, userUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        [Fact]
        public void Can_RemoveRole_Using_Uuid_If_Role_Does_Not_Exist()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();
            var model = CreateModel((roleId, userId));
            var user = new User { Id = userId, Uuid = userUuid };
            var agreementRole = CreateRole(roleId, roleUuid);
            ExpectOption(model, roleId, agreementRole);
            ExpectGetUser(userId, user);
            ExpectGetUserByUuid(userUuid, user);
            ExpectGetRoleByUuid(model.OrganizationId, roleUuid, Maybe<TRole>.None);

            //Act
            var result = _sut.RemoveRole(model, roleUuid, userUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        private void ExpectAvailableRoles(TModel model, TRole[] availableRoles)
        {
            _optionsServiceMock.Setup(x => x.GetAvailableOptions(model.OrganizationId))
                .Returns(availableRoles);
        }

        private void ExpectAvailableOption(TModel model, int roleId, Maybe<TRole> result)
        {
            _optionsServiceMock.Setup(x => x.GetAvailableOption(model.OrganizationId, roleId))
                .Returns(result);
        }

        private void ExpectOption(TModel model, int roleId, Maybe<TRole> agreementRole)
        {
            _optionsServiceMock.Setup(x => x.GetOption(model.OrganizationId, roleId))
                .Returns(agreementRole.Select(role => (role, true)));
        }

        private void ExpectOrganizationUsers(TModel model, Maybe<string> emailQuery, User[] users)
        {
            _userRepository.Setup(x => x.SearchOrganizationUsers(model.OrganizationId, emailQuery))
                .Returns(users.AsQueryable());
        }

        public abstract TModel CreateModel((int righRole, int rightUserId)? right = null);
        public abstract TRole CreateRole(int? roleId = null, Guid? roleUuid = null);

        private void ExpectGetUser(int userId, User user)
        {
            _userRepository.Setup(x => x.GetById(userId)).Returns(user);
        }

        private void ExpectGetUserByUuid(Guid userUuid, Maybe<User> user)
        {
            if (user.HasValue)
            {
                _userRepository.Setup(x => x.GetByUuid(userUuid)).Returns(user);
            }
            else
            {
                _userRepository.Setup(x => x.GetByUuid(userUuid)).Returns(Maybe<User>.None);
            }
        }

        private void ExpectGetRoleByUuid(int orgId, Guid roleUuid, Maybe<TRole> role)
        {
            if (role.HasValue)
            {
                _optionsServiceMock.Setup(x => x.GetOptionByUuid(orgId, roleUuid)).Returns(Maybe<(TRole option, bool available)>.Some((role.Value, true)));
            }
            else
            {
                _optionsServiceMock.Setup(x => x.GetOptionByUuid(orgId, roleUuid)).Returns(Maybe<(TRole option, bool available)>.None);
            }
            
        }
    }
}
