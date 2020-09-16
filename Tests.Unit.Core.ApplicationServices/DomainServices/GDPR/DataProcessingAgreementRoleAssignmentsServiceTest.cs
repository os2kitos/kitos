using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.GDPR;
using Core.DomainServices.Options;
using Infrastructure.Services.Types;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.GDPR
{
    public class DataProcessingAgreementRoleAssignmentsServiceTest : WithAutoFixture
    {
        private readonly DataProcessingAgreementRoleAssignmentsService _sut;
        private readonly Mock<IOptionsService<DataProcessingAgreementRight, DataProcessingAgreementRole>> _optionsServiceMock;
        private readonly Mock<IUserRepository> _userRepository;

        public DataProcessingAgreementRoleAssignmentsServiceTest()
        {
            _optionsServiceMock = new Mock<IOptionsService<DataProcessingAgreementRight, DataProcessingAgreementRole>>();
            _userRepository = new Mock<IUserRepository>();
            _sut = new DataProcessingAgreementRoleAssignmentsService(
                _optionsServiceMock.Object,
                _userRepository.Object);
        }

        [Fact]
        public void Can_GetApplicableRoles()
        {
            //Arrange
            var dataProcessingAgreement = CreateDpa();
            var availableRoles = new[] { new DataProcessingAgreementRole(), new DataProcessingAgreementRole() };
            ExpectAvailableRoles(dataProcessingAgreement, availableRoles);

            //Act
            var roles = _sut.GetApplicableRoles(dataProcessingAgreement);

            //Assert
            Assert.Equal(availableRoles, roles);
        }

        [Fact]
        public void GetUsersWhichCanBeAssignedToRole_Returns_OrganizationUsers_Which_DoesNotAlreadyHaveTheRoleOnTheDpa()
        {
            //Arrange
            var roleId = A<int>();
            var excludedUserId = A<int>();
            var dataProcessingAgreement = CreateDpa((roleId, excludedUserId));
            Maybe<string> emailQuery = A<string>();
            var users = new[] { new User { Id = A<int>() }, new User { Id = A<int>() }, new User { Id = excludedUserId } };
            var agreementRole = new DataProcessingAgreementRole { Id = roleId };
            ExpectAvailableOption(dataProcessingAgreement, roleId, agreementRole);
            ExpectOrganizationUsers(dataProcessingAgreement, emailQuery, users);

            //Act
            var result = _sut.GetUsersWhichCanBeAssignedToRole(dataProcessingAgreement, roleId, emailQuery);

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
            var dataProcessingAgreement = CreateDpa();
            ExpectAvailableOption(dataProcessingAgreement, roleId, Maybe<DataProcessingAgreementRole>.None);

            //Act
            var result = _sut.GetUsersWhichCanBeAssignedToRole(dataProcessingAgreement, roleId, A<string>());

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
            var dataProcessingAgreement = CreateDpa((A<int>(), userId));
            var users = new[] { new User { Id = A<int>() }, new User { Id = userId } };
            var agreementRole = new DataProcessingAgreementRole { Id = roleId };
            ExpectAvailableOption(dataProcessingAgreement, roleId, agreementRole);
            ExpectOrganizationUsers(dataProcessingAgreement, Maybe<string>.None, users);

            //Act
            var result = _sut.AssignRole(dataProcessingAgreement, roleId, userId);

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
            var dataProcessingAgreement = CreateDpa();
            ExpectAvailableOption(dataProcessingAgreement, roleId, Maybe<DataProcessingAgreementRole>.None);

            //Act
            var result = _sut.AssignRole(dataProcessingAgreement, roleId, userId);

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
            var dataProcessingAgreement = CreateDpa();
            var users = new[] { new User { Id = A<int>() } }; //Target user is not included
            var agreementRole = new DataProcessingAgreementRole { Id = roleId };
            ExpectAvailableOption(dataProcessingAgreement, roleId, agreementRole);
            ExpectOrganizationUsers(dataProcessingAgreement, Maybe<string>.None, users);

            //Act
            var result = _sut.AssignRole(dataProcessingAgreement, roleId, userId);

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
            var dataProcessingAgreement = CreateDpa((roleId, userId));
            var users = new[] { new User { Id = A<int>() }, new User { Id = userId } };
            var agreementRole = new DataProcessingAgreementRole { Id = roleId };
            ExpectAvailableOption(dataProcessingAgreement, roleId, agreementRole);
            ExpectOrganizationUsers(dataProcessingAgreement, Maybe<string>.None, users);

            //Act
            var result = _sut.AssignRole(dataProcessingAgreement, roleId, userId);

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
            var dataProcessingAgreement = CreateDpa((roleId, userId));
            var user = new User { Id = userId };
            var agreementRole = new DataProcessingAgreementRole { Id = roleId };
            ExpectOption(dataProcessingAgreement, roleId, agreementRole);
            ExpectGetUser(userId, user);

            //Act
            var result = _sut.RemoveRole(dataProcessingAgreement, roleId, userId);

            //Assert
            Assert.True(result.Ok);
            var removedRight = result.Value;
            Assert.Equal(roleId, removedRight.RoleId);
            Assert.Equal(userId, removedRight.UserId);

        }

        [Fact]
        public void Cannot_RemoveRole_If_RoleId_Is_Invalid()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var dataProcessingAgreement = CreateDpa((roleId, userId));
            ExpectOption(dataProcessingAgreement, roleId, Maybe<DataProcessingAgreementRole>.None);

            //Act
            var result = _sut.RemoveRole(dataProcessingAgreement, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);

        }

        [Fact]
        public void Cannot_RemoveRole_If_UserId_Is_Invalid()
        {
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var dataProcessingAgreement = CreateDpa((roleId, userId));
            var agreementRole = new DataProcessingAgreementRole { Id = roleId };
            ExpectOption(dataProcessingAgreement, roleId, agreementRole);
            ExpectGetUser(userId, null);

            //Act
            var result = _sut.RemoveRole(dataProcessingAgreement, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);

        }

        [Fact]
        public void Cannot_RemoveRole_If_No_Existing_Assignment()
        {  
            //Arrange
            var roleId = A<int>();
            var userId = A<int>();
            var dataProcessingAgreement = CreateDpa((A<int>(), userId)); //Existing user role but different role
            var user = new User { Id = userId };
            var agreementRole = new DataProcessingAgreementRole { Id = roleId };
            ExpectOption(dataProcessingAgreement, roleId, agreementRole);
            ExpectGetUser(userId, user);

            //Act
            var result = _sut.RemoveRole(dataProcessingAgreement, roleId, userId);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadInput, result.Error.FailureType);

        }

        private void ExpectAvailableRoles(DataProcessingAgreement dataProcessingAgreement,
            DataProcessingAgreementRole[] availableRoles)
        {
            _optionsServiceMock.Setup(x => x.GetAvailableOptions(dataProcessingAgreement.OrganizationId))
                .Returns(availableRoles);
        }

        private void ExpectAvailableOption(DataProcessingAgreement dataProcessingAgreement, int roleId, Maybe<DataProcessingAgreementRole> result)
        {
            _optionsServiceMock.Setup(x => x.GetAvailableOption(dataProcessingAgreement.OrganizationId, roleId))
                .Returns(result);
        }

        private void ExpectOption(DataProcessingAgreement dataProcessingAgreement, int roleId, Maybe<DataProcessingAgreementRole> agreementRole)
        {
            _optionsServiceMock.Setup(x => x.GetOption(dataProcessingAgreement.OrganizationId, roleId))
                .Returns(agreementRole.Select(role => (role, true)));
        }

        private void ExpectOrganizationUsers(DataProcessingAgreement dataProcessingAgreement, Maybe<string> emailQuery, User[] users)
        {
            _userRepository.Setup(x => x.SearchOrganizationUsers(dataProcessingAgreement.OrganizationId, emailQuery))
                .Returns(users.AsQueryable());
        }

        private DataProcessingAgreement CreateDpa((int righRole, int rightUserId)? right = null)
        {
            var dataProcessingAgreement = new DataProcessingAgreement
            {
                OrganizationId = A<int>(),
                Rights = new List<DataProcessingAgreementRight>
                {
                    new DataProcessingAgreementRight
                    {
                        RoleId = right?.righRole ?? A<int>(),
                        UserId = right?.rightUserId ?? A<int>()
                    }
                }
            };
            return dataProcessingAgreement;
        }

        private void ExpectGetUser(int userId, User user)
        {
            _userRepository.Setup(x => x.GetById(userId)).Returns(user);
        }
    }
}
