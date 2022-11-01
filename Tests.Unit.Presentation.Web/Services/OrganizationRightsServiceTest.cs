using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class OrganizationRightsServiceTest : WithAutoFixture
    {
        private readonly OrganizationRightsService _sut;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IGenericRepository<OrganizationRight>> _organizationRightRepository;
        private readonly Mock<IGenericRepository<OrganizationUnitRight>> _organizationUnitRightRepository;
        private readonly Mock<IOrganizationalUserContext> _organizationUserContext;
        private readonly Mock<ITransactionManager> _transactionManager;

        public OrganizationRightsServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _organizationRightRepository = new Mock<IGenericRepository<OrganizationRight>>();
            _organizationUserContext = new Mock<IOrganizationalUserContext>();
            _organizationUnitRightRepository = new Mock<IGenericRepository<OrganizationUnitRight>>();
            _transactionManager = new Mock<ITransactionManager>(); 
            _transactionManager = new Mock<ITransactionManager>();
            var orgRepository = new Mock<IGenericRepository<Organization>>();
            var identityResolver = new Mock<IEntityIdentityResolver>();

            _sut = new OrganizationRightsService(_authorizationContext.Object,
                _organizationRightRepository.Object,
                _organizationUserContext.Object,
                Mock.Of<IDomainEvents>(), 
                Mock.Of<ILogger>(),
                _organizationUnitRightRepository.Object,
                _transactionManager.Object,
                orgRepository.Object,
                identityResolver.Object);
        }

        [Fact]
        public void RemoveRole_Returns_NotFound()
        {
            //Arrange
            var id = A<int>();
            _organizationRightRepository.Setup(x => x.GetByKey(id)).Returns(default(OrganizationRight));

            //Act
            var result = _sut.RemoveRole(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.NotFound, result.Error);
        }

        [Fact]
        public void RemoveRole_Returns_Forbidden()
        {
            //Arrange
            var id = A<int>();
            var organizationRight = new OrganizationRight();
            _organizationRightRepository.Setup(x => x.GetByKey(id)).Returns(organizationRight);
            _authorizationContext.Setup(x => x.AllowDelete(organizationRight)).Returns(false);

            //Act
            var result = _sut.RemoveRole(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void RemoveRole_Returns_Conflict_If_Global_Admin_Tries_To_Remove_Self()
        {
            //Arrange
            var id = A<int>();
            int usrId = A<int>();
            var organizationRight = new OrganizationRight()
            {
                UserId = usrId,
                Role = OrganizationRole.GlobalAdmin
            };
            _organizationRightRepository.Setup(x => x.GetByKey(id)).Returns(organizationRight);
            _authorizationContext.Setup(x => x.AllowDelete(organizationRight)).Returns(true);
            _organizationUserContext.Setup(x => x.UserId).Returns(usrId);

            //Act
            var result = _sut.RemoveRole(id);

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Conflict, result.Error);
        }

        [Fact]
        public void RemoveRole_Returns_Ok()
        {
            //Arrange
            var id = A<int>();
            var organizationRight = new OrganizationRight()
            {
                UserId = A<int>(),
                Id = id
            };
            _organizationRightRepository.Setup(x => x.GetByKey(id)).Returns(organizationRight);
            _authorizationContext.Setup(x => x.AllowDelete(organizationRight)).Returns(true);
            _organizationUserContext.Setup(x => x.UserId).Returns(A<int>());

            //Act
            var result = _sut.RemoveRole(id);

            //Assert
            Assert.True(result.Ok);
            _organizationRightRepository.Verify(x => x.DeleteByKey(id), Times.Once);
            _organizationRightRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void AddRightToUser_Returns_Forbidden()
        {
            //Arrange
            var organizationId = A<int>();
            _authorizationContext.Setup(x => x.AllowCreate<OrganizationRight>(organizationId, It.IsAny<OrganizationRight>())).Returns(false);

            //Act
            var result = _sut.AssignRole(organizationId, A<int>(), A<OrganizationRole>());

            //Assert
            Assert.False(result.Ok);
            Assert.Equal(OperationFailure.Forbidden, result.Error);
        }

        [Fact]
        public void AddRightToUser_Returns_Ok()
        {
            //Arrange
            var organizationId = A<int>();
            var userId = A<int>();
            var organizationRole = A<OrganizationRole>();
            _organizationUserContext.Setup(x => x.UserId).Returns(userId);
            _authorizationContext.Setup(x => x.AllowCreate<OrganizationRight>(organizationId, It.IsAny<OrganizationRight>())).Returns(true);
            _organizationRightRepository.Setup(x => x.Insert(It.IsAny<OrganizationRight>())).Returns<OrganizationRight>(right => right);

            //Act
            var result = _sut.AssignRole(organizationId, userId, organizationRole);

            //Assert
            Assert.True(result.Ok);
            var resultValue = result.Value;
            Assert.Equal(organizationId, resultValue.OrganizationId);
            Assert.Equal(userId, resultValue.UserId);
            Assert.Equal(organizationRole, resultValue.Role);
            _organizationRightRepository.Verify(x => x.Insert(It.IsAny<OrganizationRight>()), Times.Once);
            _organizationRightRepository.Verify(x => x.Save(), Times.Once);
        }
    }
}
