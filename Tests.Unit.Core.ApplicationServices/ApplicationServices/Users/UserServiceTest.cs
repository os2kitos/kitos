using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Commands;
using Core.DomainModel.Events;
using Core.DomainModel;
using Core.DomainServices;
using Infrastructure.Services.Cryptography;
using Infrastructure.Services.DataAccess;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Organization;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Users
{
    public class UserServiceTest : WithAutoFixture
    {
        private readonly UserService _sut;
        private readonly Mock<IUserRepository> _repository;
        private readonly Mock<IOrganizationService> _organizationService;

        public UserServiceTest()
        {
            var ttl = new TimeSpan();
            var baseUrl = A<string>();
            var mailSuffix = A<string>();
            var defaultUserPassword = A<string>();
            var useDefaultUserPassword = A<bool>();
            _repository = new Mock<IUserRepository>();
            _organizationService = new Mock<IOrganizationService>();
            var transactionManager = new Mock<ITransactionManager>();
            var userRepository = new Mock<IGenericRepository<User>>();
            var orgRepository = new Mock<IGenericRepository<Organization>>();
            var passwordRepository = new Mock<IGenericRepository<PasswordResetRequest>>();
            var mailClient = new Mock<IMailClient>();
            var cryptoService = new Mock<ICryptoService>();
            var authorizationContext = new Mock<IAuthorizationContext>();
            var domainService = new Mock<IDomainEvents>();
            var orgUserContext = new Mock<IOrganizationalUserContext>();
            var commandBus = new Mock<ICommandBus>();

            _sut = new UserService(ttl,
                baseUrl,
                mailSuffix,
                defaultUserPassword,
                useDefaultUserPassword,
                userRepository.Object,
                orgRepository.Object,
                passwordRepository.Object,
                mailClient.Object,
                cryptoService.Object,
                authorizationContext.Object,
                domainService.Object,
                _repository.Object,
                _organizationService.Object,
                transactionManager.Object,
                orgUserContext.Object,
                commandBus.Object);
        }

        [Fact]
        public void Can_Find_Users_By_Email()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var permissions = new OrganizationPermissionsResult(true, true, true, true);
            var userEmail = A<string>();
            var users = new List<User>
            {
                new()
                {
                    Email = A<string>()
                },
                new()
                {
                    Email = userEmail
                }
            };

            _organizationService.Setup(x => x.GetPermissions(orgUuid)).Returns(permissions);
            _repository.Setup(x => x.AsQueryable()).Returns(users.AsQueryable());

            //Act
            var result = _sut.GetUserByEmail(orgUuid, userEmail);

            //Assert
            Assert.True(result.Ok);
            Assert.Equal(userEmail, result.Value.Email);
        }

        [Fact]
        public void Find_Users_By_Email_Returns_Forbidden_If_User_Not_Allowed_To_Read()
        {
            //Arrange
            var orgUuid = A<Guid>();
            var permissions = new OrganizationPermissionsResult(false, false, false, false);

            _organizationService.Setup(x => x.GetPermissions(orgUuid)).Returns(permissions);

            //Act
            var result = _sut.GetUserByEmail(orgUuid, A<string>());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }


    }
}
