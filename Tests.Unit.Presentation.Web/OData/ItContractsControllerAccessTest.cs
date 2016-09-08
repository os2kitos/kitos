﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Http.Results;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices;
using FluentAssertions;
using NSubstitute;
using Presentation.Web.Controllers.OData;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

//https://datatellblog.wordpress.com/2015/05/05/unit-testing-asp-net-mvc-authorization/

namespace Tests.Unit.Presentation.Web.OData
{
    public class ItContractsControllerAccessTest
    {
        private ItContractsController _itContractsController;
        private IGenericRepository<User> _userRepository;
        private IGenericRepository<ItContract> _itContractRepository;
        private IGenericRepository<OrganizationUnit> _organizationUnitRepository;

        public ItContractsControllerAccessTest()
        {
            SetUp();
        }

        private void SetUp()
        {
            _itContractRepository = Substitute.For<IGenericRepository<ItContract>>();
            _organizationUnitRepository = Substitute.For<IGenericRepository<OrganizationUnit>>();
            _userRepository = Substitute.For<IGenericRepository<User>>();
            IQueryable<ItContract> contracts = new EnumerableQuery<ItContract>(new List<ItContract>
            {
                new ItContract {OrganizationId = 1,Name = "Contract belongs to org 1"}, new ItContract { OrganizationId = 2, Name = "Contract belongs to org 2" }
            });
            _itContractRepository.AsQueryable().Returns(contracts);

            IQueryable<OrganizationUnit> organizationUnits = new EnumerableQuery<OrganizationUnit>(new List<OrganizationUnit>());
            _organizationUnitRepository.AsQueryable().Returns(organizationUnits);

            _itContractsController = new ItContractsController(_itContractRepository, _organizationUnitRepository)
            {
                UserRepository = _userRepository,
                AuthenticationService = new AuthenticationService(_userRepository)

            };
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
            SetAccess(true, orgKey:1, isGlobalmin: false);

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
                    Id = 1,
                    IsGlobalAdmin = isGlobalmin,
                    DefaultOrganizationId = orgKey,
                    DefaultOrganization = new Organization {Id = orgKey,Type= new OrganizationType {Category = OrganizationCategory.Other} },
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

            _userRepository.GetByKey(Arg.Any<int>())
                .Returns(list.First());
        }

        #endregion;
    }
}