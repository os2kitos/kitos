using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Options;
using Core.DomainServices.Role;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Role
{
    public abstract class RoleAssignmentsServiceTest<TModel, TRight, TRole> : WithAutoFixture
        where TRight : Entity, IRight<TModel, TRight, TRole>, new()
        where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
        where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
    {
        private readonly RoleAssignmentService<TRight, TRole, TModel> _sut;
        private readonly Mock<IOptionsService<TRight, TRole>> _optionsServiceMock;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IGenericRepository<TRight>> _rightRepositoryMock;

        protected RoleAssignmentsServiceTest()
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
            var newRight = result.Value;
            Assert.Equal(roleId, newRight.Role.Id);
            Assert.Equal(userId, newRight.User.Id);
            Assert.Contains(model.Rights, x => x == newRight);
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
            Assert.DoesNotContain(model.Rights, x => x == removedRight);
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
            var users = new[] { new User { Id = A<int>(), Uuid = A<Guid>() }, new User { Id = userId, Uuid = userUuid } };
            var agreementRole = CreateRole(roleId, roleUuid);
            ExpectAvailableOption(model, roleId, agreementRole);
            ExpectOrganizationUsers(model, Maybe<string>.None, users);
            ExpectGetUserByUuid(userUuid, users.First(x => x.Uuid == userUuid));
            ExpectGetRoleByUuid(model.OrganizationId, roleUuid, agreementRole);

            //Act
            var result = _sut.AssignRole(model, roleUuid, userUuid);

            //Assert
            Assert.True(result.Ok);
            var newRight = result.Value;
            Assert.Equal(roleId, newRight.Role.Id);
            Assert.Equal(userId, newRight.User.Id);
            Assert.Contains(model.Rights, x => x == newRight);
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
            ExpectGetUserByUuid(userUuid, user);
            ExpectGetRoleByUuid(model.OrganizationId, roleUuid, agreementRole);

            //Act
            var result = _sut.RemoveRole(model, roleUuid, userUuid);

            //Assert
            Assert.True(result.Ok);
            var removedRight = result.Value;
            Assert.Equal(roleId, removedRight.RoleId);
            Assert.Equal(userId, removedRight.UserId);
            Assert.DoesNotContain(model.Rights, x => x == removedRight);
            _rightRepositoryMock.Verify(x => x.Delete(removedRight), Times.Once);
        }

        [Fact]
        public void Cannot_RemoveRole_Using_Uuid_If_User_Does_Not_Exist()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();
            var model = CreateModel((roleId, userId));
            var agreementRole = CreateRole(roleId, roleUuid);
            ExpectOption(model, roleId, agreementRole);
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

        [Fact]
        public void BatchUpdateRoles_Mirrors_Requested_Role_Assignments_To_Target()
        {
            //Arrange
            var model = CreateModel();
            var rightThatIsKept = CreateRight();
            var rightThatIsRemoved = CreateRight();
            model.Rights.Clear();
            model.Rights.Add(rightThatIsKept);
            model.Rights.Add(rightThatIsRemoved);

            var expectedAddition1 = CreateRight();
            var expectedAddition2 = CreateRight(rightThatIsRemoved.User);

            var requestedNewState = new List<(Guid roleUuid, Guid userUuid)>()
            {
                (rightThatIsKept.Role.Uuid, rightThatIsKept.User.Uuid),
                (expectedAddition1.Role.Uuid,expectedAddition1.User.Uuid),

                //Same user as the removed right but different role
                (expectedAddition2.Role.Uuid,expectedAddition2.User.Uuid)
            };

            SetupAdditionPrerequisites(model, expectedAddition1.Role, expectedAddition1.User);
            SetupAdditionPrerequisites(model, expectedAddition2.Role, expectedAddition2.User);

            ExpectOrganizationUsers(model, Maybe<string>.None, expectedAddition1.User, expectedAddition2.User);

            //Act
            var error = _sut.BatchUpdateRoles(model, requestedNewState);

            //Assert
            Assert.True(error.IsNone);
            var actualState = model.Rights.Select(r => (r.Role.Uuid, r.User.Uuid)).ToList();
            Assert.Equal(requestedNewState, actualState);
        }

        [Fact]
        public void Cannot_BatchUpdateRoles_If_New_User_Is_Not_In_Organization()
        {
            //Arrange
            var model = CreateModel();
            model.Rights.Clear();

            var expectedAddition1 = CreateRight();
            var expectedAddition2 = CreateRight();

            var requestedNewState = new List<(Guid roleUuid, Guid userUuid)>()
            {
                (expectedAddition1.Role.Uuid,expectedAddition1.User.Uuid),
                (expectedAddition2.Role.Uuid,expectedAddition2.User.Uuid)
            };

            SetupAdditionPrerequisites(model, expectedAddition1.Role, expectedAddition1.User);
            SetupAdditionPrerequisites(model, expectedAddition2.Role, expectedAddition2.User);

            //user 2 is not in organization
            ExpectOrganizationUsers(model, Maybe<string>.None, expectedAddition1.User);

            //Act
            var error = _sut.BatchUpdateRoles(model, requestedNewState);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal($"Failed to assign role with Uuid: {expectedAddition2.Role.Uuid} from user with Uuid: {expectedAddition2.User.Uuid}, with following error message: User Id {expectedAddition2.User.Id} is invalid in the context of assign role {expectedAddition2.Role.Id} to {typeof(TModel)} with id {model.Id} in organization with id '{model.OrganizationId}'", error.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        [Fact]
        public void Cannot_BatchUpdateRoles_If_Role_Is_Not_Available()
        {
            //Arrange
            var model = CreateModel();
            model.Rights.Clear();

            var addition = CreateRight();

            var requestedNewState = new List<(Guid roleUuid, Guid userUuid)>()
            {
                (addition.Role.Uuid,addition.User.Uuid),
            };

            SetupAdditionPrerequisites(model, addition.Role, addition.User);
            ExpectAvailableOption(model, addition.RoleId, Maybe<TRole>.None);

            //Act
            var error = _sut.BatchUpdateRoles(model, requestedNewState);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal($"Failed to assign role with Uuid: {addition.Role.Uuid} from user with Uuid: {addition.User.Uuid}, with following error message: Invalid role id", error.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        [Fact]
        public void Cannot_BatchUpdateRoles_If_Role_Cannot_Be_Resolved()
        {
            //Arrange
            var model = CreateModel();
            model.Rights.Clear();

            var addition = CreateRight();

            var requestedNewState = new List<(Guid roleUuid, Guid userUuid)>()
            {
                (addition.Role.Uuid,addition.User.Uuid),
            };

            SetupAdditionPrerequisites(model, addition.Role, addition.User);
            ExpectGetRoleByUuid(model.OrganizationId, addition.Role.Uuid, Maybe<TRole>.None);

            //Act
            var error = _sut.BatchUpdateRoles(model, requestedNewState);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal($"Failed to assign role with Uuid: {addition.Role.Uuid} from user with Uuid: {addition.User.Uuid}, with following error message: Could not find role with Uuid: {addition.Role.Uuid}", error.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        [Fact]
        public void Cannot_BatchUpdateRoles_If_User_Cannot_Be_Resolved()
        {
            //Arrange
            var model = CreateModel();
            model.Rights.Clear();

            var addition = CreateRight();

            var requestedNewState = new List<(Guid roleUuid, Guid userUuid)>()
            {
                (addition.Role.Uuid,addition.User.Uuid),
            };

            SetupAdditionPrerequisites(model, addition.Role, addition.User);
            ExpectGetUserByUuid(addition.User.Uuid, Maybe<User>.None);

            //Act
            var error = _sut.BatchUpdateRoles(model, requestedNewState);

            //Assert
            Assert.True(error.HasValue);
            Assert.Equal($"Failed to assign role with Uuid: {addition.Role.Uuid} from user with Uuid: {addition.User.Uuid}, with following error message: Could not find user with Uuid: {addition.User.Uuid}", error.Value.Message.GetValueOrEmptyString());
            Assert.Equal(OperationFailure.BadInput, error.Value.FailureType);
        }

        private void SetupAdditionPrerequisites(TModel model, TRole role, User user)
        {
            ExpectGetRoleByUuid(model.OrganizationId, role.Uuid, role);
            ExpectGetUserByUuid(user.Uuid, user);
            ExpectAvailableOption(model, role.Id, role);
        }

        private TRight CreateRight(User predefinedUser = null)
        {
            var role = CreateRole();
            var user = predefinedUser ?? CreateUser();
            return new TRight
            {
                Role = role,
                User = user,
                RoleId = role.Id,
                UserId = user.Id
            };
        }

        private User CreateUser()
        {
            return new User()
            {
                Uuid = A<Guid>(),
                Id = A<int>()
            };
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

        private void ExpectOrganizationUsers(TModel model, Maybe<string> emailQuery, params User[] users)
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
