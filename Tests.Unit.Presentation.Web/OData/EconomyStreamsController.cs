using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Results;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Moq;
using Presentation.Web.Controllers.OData;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.OData
{
    public class ODataEconomyStreamsController
    {
        private readonly EconomyStreamsController _sut;
        private readonly Mock<IGenericRepository<EconomyStream>> _economyStreamRepository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;

        public ODataEconomyStreamsController()
        {
            _economyStreamRepository = new Mock<IGenericRepository<EconomyStream>>();
            _sut = new EconomyStreamsController(_economyStreamRepository.Object);
            _authorizationContext = new Mock<IAuthorizationContext>();
            _sut.AuthorizationContext = _authorizationContext.Object;
            var userMock = new UserMock(_sut, "12345678");
            userMock.LogOn();
        }

        [Fact]
        public void GetByOrganization_NoAccess_ReturnForbidden()
        {
            // Arrange
            const int orgKey = 1;
            ExpectGetOrganizationalReadAccessReturns(orgKey, OrganizationDataReadAccessLevel.None);

            // Act
            var result = _sut.GetByOrganization(orgKey) as ResponseMessageResult;

            // Assert
            Assert.IsType<ResponseMessageResult>(result);
            Assert.Equal(HttpStatusCode.Forbidden,result.Response.StatusCode);
        }

        [Fact]
        public void GetByOrganization_Access_ReturnOk()
        {
            // Arrange
            const int orgKey = 1;
            ExpectGetOrganizationalReadAccessReturns(orgKey, OrganizationDataReadAccessLevel.All);
            IQueryable<EconomyStream> list = new EnumerableQuery<EconomyStream>(new List<EconomyStream>());
            _economyStreamRepository.Setup(x=>x.AsQueryable()).Returns(list);

            // Act
            var result = _sut.GetByOrganization(orgKey);

            // Assert
            Assert.IsType<OkNegotiatedContentResult<IQueryable<EconomyStream>>>(result);
            var okNegotiatedContentResult = result as OkNegotiatedContentResult<IQueryable<EconomyStream>>;
            if (okNegotiatedContentResult == null) return;

            var data = okNegotiatedContentResult.Content;
            Assert.Equal(list, data);
        }

        [Fact]
        public void GetByOrganizationWithPublic_Access_ReturnOk()
        {
            // Arrange
            const int orgKey = 1;
            const int contractKey = 1;
            ExpectGetOrganizationalReadAccessReturns(orgKey, OrganizationDataReadAccessLevel.Public);
            IQueryable<EconomyStream> list = new EnumerableQuery<EconomyStream>(new List<EconomyStream> { new EconomyStream { AccessModifier = AccessModifier.Public, ExternPaymentFor = new ItContract { Id = contractKey, OrganizationId = orgKey } } });
            _economyStreamRepository.Setup(x => x.AsQueryable()).Returns(list);

            // Act
            var result = _sut.GetByOrganization(orgKey);

            // Assert
            Assert.IsType<OkNegotiatedContentResult<IQueryable<EconomyStream>>>(result);
            var okNegotiatedContentResult = result as OkNegotiatedContentResult<IQueryable<EconomyStream>>;
            if (okNegotiatedContentResult == null) return;

            var data = okNegotiatedContentResult.Content;
            Assert.Equal(list, data);
        }

        [Fact]
        public void GetByOrganizationWithLocal_NoAccess_ReturnForbidden()
        {
            // Arrange
            const int orgKey = 1;
            const int contractKey = 1;
            ExpectGetOrganizationalReadAccessReturns(orgKey, OrganizationDataReadAccessLevel.None);
            IQueryable<EconomyStream> list = new EnumerableQuery<EconomyStream>(new List<EconomyStream> { new EconomyStream { AccessModifier = AccessModifier.Local, ExternPaymentFor = new ItContract { Id = contractKey, OrganizationId = orgKey } } });
            _economyStreamRepository.Setup(x => x.AsQueryable()).Returns(list);

            // Act
            var result = _sut.GetByOrganization(orgKey) as ResponseMessageResult;

            // Assert
            Assert.IsType<ResponseMessageResult>(result);
            Assert.Equal(HttpStatusCode.Forbidden, result.Response.StatusCode);
        }

        private void ExpectGetOrganizationalReadAccessReturns(int orgKey, OrganizationDataReadAccessLevel result)
        {
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgKey))
                .Returns(result);
        }
    }
}
