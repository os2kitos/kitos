﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http.Results;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using NSubstitute;
using Presentation.Web.Controllers.OData;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.OData
{
    public class ODataEconomyStreamsController
    {
        private readonly EconomyStreamsController _economyStreamsController;
        private readonly IGenericRepository<EconomyStream> _economyStreamRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly UserMock _userMock;

        public ODataEconomyStreamsController()
        {
            _economyStreamRepository = Substitute.For<IGenericRepository<EconomyStream>>();
            _userRepository = Substitute.For<IGenericRepository<User>>();

            _economyStreamsController = new EconomyStreamsController(_economyStreamRepository, _userRepository);
            _userMock = new UserMock(_economyStreamsController, "12345678");

            _userMock.LogOn();
        }

        [Fact]
        public void GetByOrganization_NoAccess_ReturnUnauthorized()
        {
            // Arrange
            const int orgKey = 1;
            SetAccess(false, orgKey);

            // Act
            var result = _economyStreamsController.GetByOrganization(orgKey);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void GetByOrganization_Access_ReturnOk()
        {
            // Arrange
            const int orgKey = 1;
            SetAccess(true, orgKey);
            IQueryable<EconomyStream> list = new EnumerableQuery<EconomyStream>(new List<EconomyStream>());
            _economyStreamRepository.AsQueryable()
                .Returns(list);

            // Act
            var result = _economyStreamsController.GetByOrganization(orgKey);

            // Assert
            Assert.IsType<OkNegotiatedContentResult<IQueryable<EconomyStream>>>(result);
            var okNegotiatedContentResult = result as OkNegotiatedContentResult<IQueryable<EconomyStream>>;
            if (okNegotiatedContentResult == null) return;

            var data = okNegotiatedContentResult.Content;
            Assert.Equal(list, data);
        }

        [Fact]
        public void GetAllExtern_NoAccess_ReturnUnauthorized()
        {
            // Arrange
            const int orgKey = 1;
            const int contractKey = 1;
            SetAccess(false, orgKey);

            // Act
            var result = _economyStreamsController.GetAllExtern(orgKey, contractKey);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void GetAllExtern_Access_ReturnOk()
        {
            // Arrange
            const int orgKey = 1;
            const int contractKey = 1;
            SetAccess(true, orgKey);
            IQueryable<EconomyStream> list = new EnumerableQuery<EconomyStream>(new List<EconomyStream>());
            _economyStreamRepository.AsQueryable()
                .Returns(list);

            // Act
            var result = _economyStreamsController.GetAllExtern(orgKey, contractKey);

            // Assert
            Assert.IsType<OkNegotiatedContentResult<IQueryable<EconomyStream>>>(result);
            var okNegotiatedContentResult = result as OkNegotiatedContentResult<IQueryable<EconomyStream>>;
            if (okNegotiatedContentResult == null) return;

            var data = okNegotiatedContentResult.Content;
            Assert.Equal(list, data);
        }

        [Fact]
        public void GetAllIntern_NoAccess_ReturnUnauthorized()
        {
            // Arrange
            const int orgKey = 1;
            const int contractKey = 1;
            SetAccess(false, orgKey);

            // Act
            var result = _economyStreamsController.GetAllIntern(orgKey, contractKey);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void GetAllIntern_Access_ReturnOk()
        {
            // Arrange
            const int orgKey = 1;
            const int contractKey = 1;
            SetAccess(true, orgKey);
            IQueryable<EconomyStream> list = new EnumerableQuery<EconomyStream>(new List<EconomyStream>());
            _economyStreamRepository.AsQueryable()
                .Returns(list);

            // Act
            var result = _economyStreamsController.GetAllIntern(orgKey, contractKey);

            // Assert
            Assert.IsType<OkNegotiatedContentResult<IQueryable<EconomyStream>>>(result);
            var okNegotiatedContentResult = result as OkNegotiatedContentResult<IQueryable<EconomyStream>>;
            if (okNegotiatedContentResult == null) return;

            var data = okNegotiatedContentResult.Content;
            Assert.Equal(list, data);
        }

        [Fact]
        public void GetSingleExtern_NoAccess_ReturnUnauthorized()
        {
            // Arrange
            const int orgKey = 1;
            const int contractKey = 1;
            const int key = 1;
            SetAccess(false, orgKey);

            // Act
            var result = _economyStreamsController.GetSingleExtern(orgKey, contractKey, key);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void GetSingleExtern_Access_ReturnOk()
        {
            // Arrange
            const int orgKey = 1;
            const int contractKey = 1;
            const int key = 1;
            SetAccess(true, orgKey);
            IQueryable<EconomyStream> list = new EnumerableQuery<EconomyStream>(new List<EconomyStream>());
            _economyStreamRepository.AsQueryable()
                .Returns(list);

            // Act
            var result = _economyStreamsController.GetSingleExtern(orgKey, contractKey, key);

            // Assert
            Assert.IsType<OkNegotiatedContentResult<IQueryable<EconomyStream>>>(result);
            var okNegotiatedContentResult = result as OkNegotiatedContentResult<IQueryable<EconomyStream>>;
            if (okNegotiatedContentResult == null) return;

            var data = okNegotiatedContentResult.Content;
            Assert.Equal(list, data);
        }

        [Fact]
        public void GetSingleIntern_NoAccess_ReturnUnauthorized()
        {
            // Arrange
            const int orgKey = 1;
            const int contractKey = 1;
            const int key = 1;
            SetAccess(false, orgKey);

            // Act
            var result = _economyStreamsController.GetSingleIntern(orgKey, contractKey, key);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void GetSingleIntern_Access_ReturnOk()
        {
            // Arrange
            const int orgKey = 1;
            const int contractKey = 1;
            const int key = 1;
            SetAccess(true, orgKey);
            IQueryable<EconomyStream> list = new EnumerableQuery<EconomyStream>(new List<EconomyStream>());
            _economyStreamRepository.AsQueryable()
                .Returns(list);

            // Act
            var result = _economyStreamsController.GetSingleIntern(orgKey, contractKey, key);

            // Assert
            Assert.IsType<OkNegotiatedContentResult<IQueryable<EconomyStream>>>(result);
            var okNegotiatedContentResult = result as OkNegotiatedContentResult<IQueryable<EconomyStream>>;
            if (okNegotiatedContentResult == null) return;

            var data = okNegotiatedContentResult.Content;
            Assert.Equal(list, data);
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
