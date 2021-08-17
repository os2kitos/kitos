using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
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
    public class RoleAssignmentsServiceTest : WithAutoFixture
    {
        private readonly RoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration> _sut;
        private readonly Mock<IOptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole>> _optionsServiceMock;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IGenericRepository<DataProcessingRegistrationRight>> _rightRepositoryMock;

        public RoleAssignmentsServiceTest()
        {
            _optionsServiceMock = new Mock<IOptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole>>();
            _userRepository = new Mock<IUserRepository>();
            _rightRepositoryMock = new Mock<IGenericRepository<DataProcessingRegistrationRight>>();
            _sut = new RoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration>(
                _optionsServiceMock.Object,
                _userRepository.Object,
                _rightRepositoryMock.Object);
        }

        [Fact]
        public void Can_GetApplicableRoles()
        {
            //Arrange
            var registration = CreateDpa();
            var availableRoles = new[] { new DataProcessingRegistrationRole(), new DataProcessingRegistrationRole() };
            ExpectAvailableRoles(registration, availableRoles);

            //Act
            var roles = _sut.GetApplicableRoles(registration);

            //Assert
            Assert.Equal(availableRoles, roles);
        }

        [Fact]
        public void GetUsersWhichCanBeAssignedToRole_Returns_OrganizationUsers_Which_DoesNotAlreadyHaveTheRoleOnTheDpa()
        {
            //Arrange
            var roleId = A<int>();
            var excludedUserId = A<int>();
            var registration = CreateDpa((roleId, excludedUserId));
            Maybe<string> emailQuery = A<string>();
            var users = new[] { new User { Id = A<int>() }, new User { Id = A<int>() }, new User { Id = excludedUserId } };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId };
            ExpectAvailableOption(registration, roleId, agreementRole);
            ExpectOrganizationUsers(registration, emailQuery, users);

            //Act
            var result = _sut.GetUsersWhichCanBeAssignedToRole(registration, roleId, emailQuery);

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
            var registration = CreateDpa();
            ExpectAvailableOption(registration, roleId, Maybe<DataProcessingRegistrationRole>.None);

            //Act
            var result = _sut.GetUsersWhichCanBeAssignedToRole(registration, roleId, A<string>());

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
            var registration = CreateDpa((A<int>(), userId));
            var users = new[] { new User { Id = A<int>() }, new User { Id = userId } };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId };
            ExpectAvailableOption(registration, roleId, agreementRole);
            ExpectOrganizationUsers(registration, Maybe<string>.None, users);

            //Act
            var result = _sut.AssignRole(registration, roleId, userId);

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
            var registration = CreateDpa();
            ExpectAvailableOption(registration, roleId, Maybe<DataProcessingRegistrationRole>.None);

            //Act
            var result = _sut.AssignRole(registration, roleId, userId);

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
            var registration = CreateDpa();
            var users = new[] { new User { Id = A<int>() } }; //Target user is not included
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId };
            ExpectAvailableOption(registration, roleId, agreementRole);
            ExpectOrganizationUsers(registration, Maybe<string>.None, users);

            //Act
            var result = _sut.AssignRole(registration, roleId, userId);

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
            var registration = CreateDpa((roleId, userId));
            var users = new[] { new User { Id = A<int>() }, new User { Id = userId } };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId };
            ExpectAvailableOption(registration, roleId, agreementRole);
            ExpectOrganizationUsers(registration, Maybe<string>.None, users);

            //Act
            var result = _sut.AssignRole(registration, roleId, userId);

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
            var registration = CreateDpa((roleId, userId));
            var user = new User { Id = userId };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId };
            ExpectOption(registration, roleId, agreementRole);
            ExpectGetUser(userId, user);

            //Act
            var result = _sut.RemoveRole(registration, roleId, userId);

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
            var registration = CreateDpa((roleId, userId));
            ExpectOption(registration, roleId, Maybe<DataProcessingRegistrationRole>.None);

            //Act
            var result = _sut.RemoveRole(registration, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            _rightRepositoryMock.Verify(x => x.Delete(It.IsAny<DataProcessingRegistrationRight>()), Times.Never);
        }

        [Fact]
        public void Cannot_RemoveRole_If_UserId_Is_Invalid()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var registration = CreateDpa((roleId, userId));
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId };
            ExpectOption(registration, roleId, agreementRole);
            ExpectGetUser(userId, null);

            //Act
            var result = _sut.RemoveRole(registration, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            _rightRepositoryMock.Verify(x => x.Delete(It.IsAny<DataProcessingRegistrationRight>()), Times.Never);
        }

        [Fact]
        public void Cannot_RemoveRole_If_No_Existing_Assignment()
        {  
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var registration = CreateDpa((A<int>(), userId)); //Existing user role but different role
            var user = new User { Id = userId };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId };
            ExpectOption(registration, roleId, agreementRole);
            ExpectGetUser(userId, user);

            //Act
            var result = _sut.RemoveRole(registration, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
            _rightRepositoryMock.Verify(x => x.Delete(It.IsAny<DataProcessingRegistrationRight>()), Times.Never);
        }

        [Fact]
        public void Can_AssignRole_Using_Uuid()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var roleUuid = A<Guid>();
            var userUuid = A<Guid>();
            var registration = CreateDpa((A<int>(), userId));
            var users = new[] { new User { Id = A<int>(), Uuid = A<Guid>()}, new User { Id = userId, Uuid = userUuid } };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId, Uuid = roleUuid };
            ExpectAvailableOption(registration, roleId, agreementRole);
            ExpectOrganizationUsers(registration, Maybe<string>.None, users);
            ExpectGetUserByUuid(userUuid, users.First(x => x.Uuid == userUuid));
            ExpectGetRoleByUuid(registration.OrganizationId, roleUuid, agreementRole);

            //Act
            var result = _sut.AssignRole(registration, roleUuid, userUuid);

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
            var registration = CreateDpa((A<int>(), userId));
            var users = new[] { new User { Id = A<int>(), Uuid = A<Guid>() }, new User { Id = userId, Uuid = userUuid } };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId, Uuid = roleUuid };
            ExpectAvailableOption(registration, roleId, agreementRole);
            ExpectOrganizationUsers(registration, Maybe<string>.None, users);
            ExpectGetUserByUuid(userUuid, Maybe<User>.None);
            ExpectGetRoleByUuid(registration.OrganizationId, roleUuid, agreementRole);

            //Act
            var result = _sut.AssignRole(registration, roleUuid, userUuid);

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
            var registration = CreateDpa((A<int>(), userId));
            var users = new[] { new User { Id = A<int>(), Uuid = A<Guid>() }, new User { Id = userId, Uuid = userUuid } };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId, Uuid = roleUuid };
            ExpectAvailableOption(registration, roleId, agreementRole);
            ExpectOrganizationUsers(registration, Maybe<string>.None, users);
            ExpectGetUserByUuid(userUuid, users.First(x => x.Uuid == userUuid));
            ExpectGetRoleByUuid(registration.OrganizationId, roleUuid, Maybe<DataProcessingRegistrationRole>.None);

            //Act
            var result = _sut.AssignRole(registration, roleUuid, userUuid);

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
            var registration = CreateDpa((roleId, userId));
            var user = new User { Id = userId, Uuid = userUuid };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId, Uuid = roleUuid };
            ExpectOption(registration, roleId, agreementRole);
            ExpectGetUser(userId, user);
            ExpectGetUserByUuid(userUuid, user);
            ExpectGetRoleByUuid(registration.OrganizationId, roleUuid, agreementRole);

            //Act
            var result = _sut.RemoveRole(registration, roleUuid, userUuid);

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
            var registration = CreateDpa((roleId, userId));
            var user = new User { Id = userId, Uuid = userUuid };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId, Uuid = roleUuid };
            ExpectOption(registration, roleId, agreementRole);
            ExpectGetUser(userId, user);
            ExpectGetUserByUuid(userUuid, Maybe<User>.None);
            ExpectGetRoleByUuid(registration.OrganizationId, roleUuid, agreementRole);

            //Act
            var result = _sut.RemoveRole(registration, roleUuid, userUuid);

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
            var registration = CreateDpa((roleId, userId));
            var user = new User { Id = userId, Uuid = userUuid };
            var agreementRole = new DataProcessingRegistrationRole { Id = roleId, Uuid = roleUuid };
            ExpectOption(registration, roleId, agreementRole);
            ExpectGetUser(userId, user);
            ExpectGetUserByUuid(userUuid, user);
            ExpectGetRoleByUuid(registration.OrganizationId, roleUuid, Maybe<DataProcessingRegistrationRole>.None);

            //Act
            var result = _sut.RemoveRole(registration, roleUuid, userUuid);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);
        }

        private void ExpectAvailableRoles(DataProcessingRegistration dataProcessingRegistration,
            DataProcessingRegistrationRole[] availableRoles)
        {
            _optionsServiceMock.Setup(x => x.GetAvailableOptions(dataProcessingRegistration.OrganizationId))
                .Returns(availableRoles);
        }

        private void ExpectAvailableOption(DataProcessingRegistration dataProcessingRegistration, int roleId, Maybe<DataProcessingRegistrationRole> result)
        {
            _optionsServiceMock.Setup(x => x.GetAvailableOption(dataProcessingRegistration.OrganizationId, roleId))
                .Returns(result);
        }

        private void ExpectOption(DataProcessingRegistration dataProcessingRegistration, int roleId, Maybe<DataProcessingRegistrationRole> agreementRole)
        {
            _optionsServiceMock.Setup(x => x.GetOption(dataProcessingRegistration.OrganizationId, roleId))
                .Returns(agreementRole.Select(role => (role, true)));
        }

        private void ExpectOrganizationUsers(DataProcessingRegistration dataProcessingRegistration, Maybe<string> emailQuery, User[] users)
        {
            _userRepository.Setup(x => x.SearchOrganizationUsers(dataProcessingRegistration.OrganizationId, emailQuery))
                .Returns(users.AsQueryable());
        }

        private DataProcessingRegistration CreateDpa((int righRole, int rightUserId)? right = null)
        {
            var registration = new DataProcessingRegistration
            {
                OrganizationId = A<int>(),
                Rights = new List<DataProcessingRegistrationRight>
                {
                    new DataProcessingRegistrationRight
                    {
                        RoleId = right?.righRole ?? A<int>(),
                        UserId = right?.rightUserId ?? A<int>()
                    }
                }
            };
            return registration;
        }

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

        private void ExpectGetRoleByUuid(int orgId, Guid roleUuid, Maybe<DataProcessingRegistrationRole> role)
        {
            if (role.HasValue)
            {
                _optionsServiceMock.Setup(x => x.GetOptionByUuid(orgId, roleUuid)).Returns(Maybe<(DataProcessingRegistrationRole option, bool available)>.Some((role.Value, true)));
            }
            else
            {
                _optionsServiceMock.Setup(x => x.GetOptionByUuid(orgId, roleUuid)).Returns(Maybe<(DataProcessingRegistrationRole option, bool available)>.None);
            }
            
        }
    }
}
