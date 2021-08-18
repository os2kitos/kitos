using System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model
{
    public class HasRightsEntityTest : WithAutoFixture
    {
        [Fact]
        public void Can_AssignRole()
        {
            //Arrange - ItSystemUsage extends HasRightsEntity
            var role = new ItSystemRole()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var user = new User()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var sut = new ItSystemUsage();

            //Act
            var assignResult = sut.AssignRole(role, user);

            //Assert
            Assert.True(assignResult.Ok);
            var right = Assert.Single(sut.Rights);
            Assert.Equal(role.Uuid, right.Role.Uuid);
            Assert.Equal(role.Id, right.Role.Id);
            Assert.Equal(user.Uuid, right.User.Uuid);
            Assert.Equal(user.Id, right.User.Id);
        }

        [Fact]
        public void Cannot_AssignRole_If_Already_Assigned()
        {
            //Arrange - ItSystemUsage extends HasRightsEntity
            var role = new ItSystemRole()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var user = new User()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var sut = new ItSystemUsage();
            var initialAssignResult = sut.AssignRole(role, user);
            Assert.True(initialAssignResult.Ok);

            //Act
            var assignResult = sut.AssignRole(role, user);

            //Assert
            Assert.True(assignResult.Failed);
            Assert.Equal(OperationFailure.Conflict, assignResult.Error.FailureType);
        }

        [Fact]
        public void Can_RemoveRole()
        {
            //Arrange - ItSystemUsage extends HasRightsEntity
            var role = new ItSystemRole()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var user = new User()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var sut = new ItSystemUsage();
            var initialAssignResult = sut.AssignRole(role, user);
            Assert.True(initialAssignResult.Ok);
            Assert.Single(sut.Rights);

            //Act
            var assignResult = sut.RemoveRole(role, user);

            //Assert
            Assert.True(assignResult.Ok);
            Assert.Empty(sut.Rights);
        }

        [Fact]
        public void Cannot_RemoveRole_If_Not_Assigned()
        {
            //Arrange - ItSystemUsage extends HasRightsEntity
            var role = new ItSystemRole()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var user = new User()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var sut = new ItSystemUsage();

            //Act
            var assignResult = sut.RemoveRole(role, user);

            //Assert
            Assert.True(assignResult.Failed);
            Assert.Equal(OperationFailure.BadInput, assignResult.Error.FailureType);
        }

        [Fact]
        public void Can_GetRights()
        {
            //Arrange - ItSystemUsage extends HasRightsEntity
            var role = new ItSystemRole()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var user = new User()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var sut = new ItSystemUsage();
            var initialAssignResult = sut.AssignRole(role, user);
            Assert.True(initialAssignResult.Ok);

            //Act
            var rights = sut.GetRights(role.Id);

            //Assert
            var right = Assert.Single(rights);
            Assert.Equal(role.Uuid, right.Role.Uuid);
            Assert.Equal(role.Id, right.Role.Id);
            Assert.Equal(user.Uuid, right.User.Uuid);
            Assert.Equal(user.Id, right.User.Id);
        }

        [Fact]
        public void Can_GetRights_Returns_Empty_If_No_Rights_For_Role()
        {
            //Arrange - ItSystemUsage extends HasRightsEntity
            var role = new ItSystemRole()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
            var sut = new ItSystemUsage();

            //Act
            var rights = sut.GetRights(role.Id);

            //Assert
            Assert.Empty(rights);
        }
    }
}
