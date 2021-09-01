using System;
using Core.DomainModel;
using Core.DomainModel.Result;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model
{
    internal class ModelStub : HasRightsEntity<ModelStub, RightStub, RoleStub>
    {
        public override RightStub CreateNewRight(RoleStub role, User user)
        {
            return new RightStub()
            {
                Role = role,
                RoleId = role.Id,
                User = user,
                UserId = user.Id,
                Object = this
            };
        }
    }

    internal class RightStub : IRight<ModelStub, RightStub, RoleStub>
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int RoleId { get; set; }
        public RoleStub Role { get; set; }
        public int ObjectId { get; set; }
        public ModelStub Object { get; set; }
    }

    internal class RoleStub : IRoleEntity, IHasId, IHasUuid
    {
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
        public int Id { get; set; }
        public Guid Uuid { get; set; }
    }


    public class HasRightsEntityTest : WithAutoFixture
    {
        [Fact]
        public void Can_AssignRole()
        {
            //Arrange 
            var role = CreateRole();
            var user = CreateUser();
            var sut = new ModelStub();

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
            //Arrange 
            var role = CreateRole();
            var user = CreateUser();
            var sut = new ModelStub();
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
            //Arrange 
            var role = CreateRole();
            var user = CreateUser();
            var sut = new ModelStub();
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
            //Arrange 
            var role = CreateRole();
            var user = CreateUser();
            var sut = new ModelStub();

            //Act
            var assignResult = sut.RemoveRole(role, user);

            //Assert
            Assert.True(assignResult.Failed);
            Assert.Equal(OperationFailure.BadInput, assignResult.Error.FailureType);
        }

        [Fact]
        public void Can_GetRights()
        {
            //Arrange 
            var role = CreateRole();
            var user = CreateUser();
            var sut = new ModelStub();
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
            //Arrange 
            var role = CreateRole();
            var sut = new ModelStub();

            //Act
            var rights = sut.GetRights(role.Id);

            //Assert
            Assert.Empty(rights);
        }

        private RoleStub CreateRole()
        {
            return new RoleStub
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
        }

        private User CreateUser()
        {
            return new User()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
        }
    }
}
