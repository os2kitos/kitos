using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Http.Results;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices;
using NSubstitute;
using Presentation.Web.Controllers.OData;
using Xunit;

namespace Tests.Unit.Presentation.Web.OData
{
    public class ItContractsControllerAccessTest
    {
        private readonly ItContractsController _itContractsController;
        private readonly IGenericRepository<User> _userRepository;
        private IGenericRepository<ItContract> _itContractRepository;
        private IGenericRepository<OrganizationUnit> _organizationUnitRepository;

        public ItContractsControllerAccessTest()
        {
            _itContractRepository = Substitute.For<IGenericRepository<ItContract>>();
            _organizationUnitRepository = Substitute.For<IGenericRepository<OrganizationUnit>>();
            _userRepository = Substitute.For<IGenericRepository<User>>();

            _itContractsController = new ItContractsController(_itContractRepository, _organizationUnitRepository)
            {
                UserRepository = _userRepository
            };
            //_itContractsController.AuthenticationService = new AuthenticationService(aUser);
            //_itContractsController.UserService = new UserService();

        }

        [Fact]
        public void Get_NoAccess_ReturnUnauthorized()
        {
            // Arrange
            const int orgKey = 1;
            SetAccess(false, orgKey);

            // Act
            var result = _itContractsController.Get();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        #region Helpers

        /// <summary>
        /// Set user repository mock to return given access rights.
        /// </summary>
        /// <param name="allow">The access right to grant.</param>
        /// <param name="orgKey">The orgKey to grant access for.</param>
        private void SetAccess(bool allow, int orgKey)
        {
            var list = new List<User>();
            if (allow)
            {
                list.Add(new User
                {
                    OrganizationRights = new List<OrganizationRight>
                        {
                            new OrganizationRight
                            {
                                OrganizationId = orgKey
                            }
                        }
                });
            }

            _userRepository.Get(Arg.Any<Expression<Func<User, bool>>>())
                .Returns(list);
        }

        #endregion;
    }
}