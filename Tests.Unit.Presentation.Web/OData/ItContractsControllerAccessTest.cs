using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.Results;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices;
using FluentAssertions;
using NSubstitute;
using Presentation.Web.Controllers.OData;
using Xunit;

namespace Tests.Unit.Presentation.Web.OData
{
    public class ItContractsControllerAccessTest
    {
        private readonly ItContractsController _itContractsController;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<ItContract> _itContractRepository;
        private IGenericRepository<OrganizationUnit> _organizationUnitRepository;

        public ItContractsControllerAccessTest()
        {
            _itContractRepository = Substitute.For<IGenericRepository<ItContract>>();
            _organizationUnitRepository = Substitute.For<IGenericRepository<OrganizationUnit>>();
            _userRepository = Substitute.For<IGenericRepository<User>>();

            _itContractsController = new ItContractsController(_itContractRepository, _organizationUnitRepository)
            {
                UserRepository = _userRepository,
                AuthenticationService = new AuthenticationService(_userRepository)

            };
            //_itContractsController.UserService = new UserService();

        }

        [Fact]
        public void get_hasReadAccessOutsideContext_returns_two_contracts()
        {
            // Arrange
            const int orgKey = 1;
            SetAccess(true, orgKey,isGlobalmin:true);

            IQueryable<ItContract> list = new EnumerableQuery<ItContract>(new List<ItContract>
            {
                new ItContract {OrganizationId = 1,Name = "Contract belongs to org 1"}, new ItContract { OrganizationId = 2, Name = "Contract belongs to org 2" }
            });
            _itContractRepository.AsQueryable()
                .Returns(list);

            // Act
            var result = _itContractsController.Get();

            // Assert
            Assert.IsType<OkNegotiatedContentResult<IQueryable<ItContract>>>(result);
            var okNegotiatedContentResult = result as OkNegotiatedContentResult<IQueryable<ItContract>>;
            okNegotiatedContentResult.Should().NotBeNull("List should have to items");
            okNegotiatedContentResult.Content.Should().HaveCount(2);
        }

        #region Helpers

        /// <summary>
        /// Set user repository mock to return given access rights.
        /// </summary>
        /// <param name="allow">The access right to grant.</param>
        /// <param name="orgKey">The orgKey to grant access for.</param>
        /// <param name="isGlobalmin"></param>
        private void SetAccess(bool allow, int orgKey, bool isGlobalmin = false)
        {
            var list = new List<User>();
            if (allow)
            {
                list.Add(new User
                {
                    Id = 0,
                    IsGlobalAdmin = isGlobalmin,
                    DefaultOrganizationId = orgKey,
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