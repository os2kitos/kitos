using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Http.Results;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;
using Core.DomainServices;
using FluentAssertions;
using NSubstitute;
using Presentation.Web.Controllers.OData;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

//https://datatellblog.wordpress.com/2015/05/05/unit-testing-asp-net-mvc-authorization/

namespace Tests.Unit.Presentation.Web.OData
{
    public class AuthenticationServiceTest
    {
        private ItContractsController _itContractsController;
        private IGenericRepository<User> _userRepository;
        private IGenericRepository<ItContract> _itContractRepository;
        private IGenericRepository<OrganizationUnit> _organizationUnitRepository;
        private AuthenticationService _authenticationService;

        public AuthenticationServiceTest()
        {
            SetUp();
        }

        private void SetUp()
        {
            _itContractRepository = Substitute.For<IGenericRepository<ItContract>>();
            _organizationUnitRepository = Substitute.For<IGenericRepository<OrganizationUnit>>();
            _userRepository = Substitute.For<IGenericRepository<User>>();
            _authenticationService = new AuthenticationService(_userRepository);
            IQueryable<ItContract> contracts = new EnumerableQuery<ItContract>(new List<ItContract>
            {
                new ItContract {OrganizationId = 1,Name = "Contract belongs to org 1"}, new ItContract { OrganizationId = 2, Name = "Contract belongs to org 2" }
            });
            _itContractRepository.AsQueryable().Returns(contracts);

            IQueryable<OrganizationUnit> organizationUnits = new EnumerableQuery<OrganizationUnit>(new List<OrganizationUnit>());
            _organizationUnitRepository.AsQueryable().Returns(organizationUnits);

            _itContractsController = new ItContractsController(_itContractRepository, _organizationUnitRepository, _authenticationService);

            var usr = new UserMock(_itContractsController, "1");
            usr.LogOn();
        }

        [Fact]
        // test AuthenticationService.HasReadAccessOutsideContext true
        public void hasReadAccessOutsideContext_returns_two_contracts()
        {
            // Arrange
            const int orgKey = 1;
            SetAccess(true, orgKey, isGlobalmin: true);



            // Act
            var result = _itContractsController.Get();

            // Assert
            Assert.IsType<OkNegotiatedContentResult<IQueryable<ItContract>>>(result);
            var okNegotiatedContentResult = result as OkNegotiatedContentResult<IQueryable<ItContract>>;
            okNegotiatedContentResult.Should().NotBeNull("List should have to items");
            Debug.Assert(okNegotiatedContentResult != null, "okNegotiatedContentResult != null");
            okNegotiatedContentResult.Content.Should().HaveCount(2);
        }

        [Fact]
        // test AuthenticationService.HasReadAccessOutsideContext false
        public void has_no_readAccessOutsideContext_returns_one_contract()
        {
            // Arrange
            const int orgKey = 1;
            SetAccess(true, orgKey, isGlobalmin: false);

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
            okNegotiatedContentResult.Should().NotBeNull("List should have one item");
            // ReSharper disable once PossibleNullReferenceException
            okNegotiatedContentResult.Content.Should().HaveCount(1);
        }

        [Fact]
        // if you are not global admin or user.DefaultOrganization.Type.Category == OrganizationCategory.Municipality
        // then you can only access the organization you are loged in to
        public void access_with_orgId_different_than_logedin_orgId_return_Forbidden()
        {
            // arrange
            var orgKey = 2;
            SetAccess(true, orgKey: 1, isGlobalmin: false);

            // act
            var result = _itContractsController.GetItContractsByOrgUnit(orgKey, 2);

            // assert
            Assert.IsType<StatusCodeResult>(result);
            var statusCode = result as StatusCodeResult;
            // ReSharper disable once PossibleNullReferenceException
            Assert.True(statusCode.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        // if you are not global admin or user.DefaultOrganization.Type.Category == OrganizationCategory.Municipality
        // then you can only access the organization you are loged in to
        public void access_with_orgId_same_as_logedin_orgId_return_OK()
        {
            // arrange
            var orgKey = 1;
            SetAccess(true, orgKey: 1, isGlobalmin: false);

            // act
            var result = _itContractsController.GetItContractsByOrgUnit(orgKey, 1);

            // assert
            Assert.IsType<OkNegotiatedContentResult<List<ItContract>>>(result);
            var okNegotiatedContentResult = result as OkNegotiatedContentResult<List<ItContract>>;
            okNegotiatedContentResult.Should().NotBeNull("List should be emty, but not null");
        }

        [Fact]
        public void test_user_global_admin()
        {
            SetAccess(true, 1, true);
            var isGlobaladmin = _authenticationService.IsGlobalAdmin(1);
            isGlobaladmin.Should().BeTrue("User should be global admin");

            SetAccess(true, 1, false);
            isGlobaladmin = _authenticationService.IsGlobalAdmin(1);
            isGlobaladmin.Should().BeFalse("User should not be global admin");
        }

        [Fact]
        public void test_user_local_admin()
        {
            SetAccess(true, 1, false, OrganizationRole.LocalAdmin);
            var isGlobaladmin = _authenticationService.IsLocalAdmin(1);
            isGlobaladmin.Should().BeTrue("User should be local admin");

            SetAccess(true, 1);
            isGlobaladmin = _authenticationService.IsLocalAdmin(1);
            isGlobaladmin.Should().BeFalse("User should not be local admin");
        }

        [Fact]
        public void test_user_has_read_access_outside_context()
        {
            // test as user
            SetAccess(true, 1);
            var hasAccess = _authenticationService.HasReadAccessOutsideContext(_userRepository.AsQueryable().First().Id);
            hasAccess.Should().BeFalse("User is standard user and should NOT have access");

            // test as global admin
            SetAccess(true, 1, true);
            hasAccess = _authenticationService.HasReadAccessOutsideContext(_userRepository.AsQueryable().First().Id);
            hasAccess.Should().BeTrue("User is global admin and should have access");

            // test as logged into an organization that allows sharing
            SetAccess(true, 1, true, organizationCategory: OrganizationCategory.Municipality);
            hasAccess = _authenticationService.HasReadAccessOutsideContext(_userRepository.AsQueryable().First().Id);
            hasAccess.Should().BeTrue("User is logged in OrganizationCategory.Municipality and should have access");
        }

        [Fact]
        public void test_has_read_access()
        {
            // test as user, that has not created the object, access == false
            var user = SetAccess(true, 1);
            var owner = CreateTestUser(2, userId: 2);
            var entity = CreateReport(owner);
            var hasAccess = _authenticationService.HasReadAccess(user.Id, entity);
            hasAccess.Should().BeFalse("user is standard user and has not created the object");

            // test as user, that has created the object, access == true
            owner = SetAccess(true, 1);
            entity = CreateReport(owner);
            hasAccess = _authenticationService.HasReadAccess(user.Id, entity);
            hasAccess.Should().BeTrue("user is standard user and has created the object");
        }

        [Fact]
        public void test_has_write_access()
        {
            // test as user, that has not created the object, access == false
            var user = SetAccess(true, 1);
            var owner = CreateTestUser(2, userId: 2);
            Entity entity = CreateReport(owner);
            var hasAccess = _authenticationService.HasWriteAccess(user.Id, entity);
            hasAccess.Should().BeFalse("user is standard user and has not created the object");

            // test as user, that has created the object, access == true
            owner = SetAccess(true, 1);
            entity = CreateReport(owner);
            hasAccess = _authenticationService.HasWriteAccess(user.Id, entity);
            hasAccess.Should().BeTrue("user is standard user and has created the object");

            // test as user, that that is entity admin, access == true, but not owner of the object
            var roles = new[]
            {
                OrganizationRole.LocalAdmin,
                OrganizationRole.ContractModuleAdmin,
                OrganizationRole.OrganizationModuleAdmin,
                OrganizationRole.ProjectModuleAdmin,
                OrganizationRole.ReportModuleAdmin,
                OrganizationRole.SystemModuleAdmin
            };

            owner = CreateTestUser(1, userId: 2);
            foreach (var role in roles)
            {
                user = SetAccess(true, 1, role: role);
                switch (role)
                {
                    case OrganizationRole.LocalAdmin:
                    case OrganizationRole.ReportModuleAdmin:
                        entity = SetOwner(new Report(), owner);
                        break;

                    case OrganizationRole.ContractModuleAdmin:
                        entity = SetOwner(new ItContract(), owner);
                        break;

                    case OrganizationRole.OrganizationModuleAdmin:
                        entity = SetOwner(new Organization(), owner);
                        break;

                    case OrganizationRole.ProjectModuleAdmin:
                        entity = SetOwner(new ItProject(), owner);
                        break;

                    case OrganizationRole.SystemModuleAdmin:
                        entity = SetOwner(new ItSystem(), owner);
                        break;
                }
                hasAccess = _authenticationService.HasWriteAccess(user.Id, entity);
                hasAccess.Should().BeTrue("user is entity admin and has access to the " + entity.GetType().Name);
                Console.WriteLine("user is entity admin and has access to the " + entity.GetType().Name);
            }
        }


        private Entity SetOwner(Entity entity, User owner)
        {
            if(entity is Organization)
                entity.Id = owner.DefaultOrganizationId.GetValueOrDefault();
            else
                ((IHasOrganization)entity).OrganizationId = owner.DefaultOrganizationId.GetValueOrDefault();

            entity.ObjectOwner = owner;
            entity.ObjectOwnerId = owner.Id;

            return entity;
        }

        #region Helpers

        private Report CreateReport(User owner)
        {
            return new Report
            {
                OrganizationId = owner.DefaultOrganizationId.GetValueOrDefault(),
                ObjectOwner = owner,
                ObjectOwnerId = owner.Id
            };
        }

        private static User CreateTestUser(int orgKey, bool isGlobalmin = false, OrganizationRole role = OrganizationRole.User, OrganizationCategory organizationCategory = OrganizationCategory.Other, int userId = 1)
        {
            return new User
            {
                Id = userId,
                IsGlobalAdmin = isGlobalmin,
                DefaultOrganizationId = orgKey,
                DefaultOrganization = new Organization
                {
                    Id = orgKey,
                    Type = new OrganizationType { Category = organizationCategory },
                    Rights = new List<OrganizationRight> { new OrganizationRight { OrganizationId = orgKey, Role = role } }
                },
                OrganizationRights = new List<OrganizationRight>
                        {
                            new OrganizationRight { OrganizationId = orgKey, Role = role }
                        }
            };
        }

        private User SetAccess(bool allow, int orgKey, bool isGlobalmin = false, OrganizationRole role = OrganizationRole.User, OrganizationCategory organizationCategory = OrganizationCategory.Other)
        {
            var list = new List<User>();

            if (allow)
            {
                var user = CreateTestUser(orgKey, isGlobalmin, role, organizationCategory);
                list.Add(user);

                _itContractsController.User = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity(user.Id.ToString()), new[] { "" });
            }

            _userRepository.Get(Arg.Any<Expression<Func<User, bool>>>())
                .Returns(list);

            _userRepository.GetByKey(Arg.Any<int>())
                .Returns(list.First());

            _userRepository.AsQueryable().Returns(list.AsQueryable());

            return list.FirstOrDefault();
        }

        #endregion;
    }
}