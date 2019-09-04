using System.Net;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Moq;
using Presentation.Web.Controllers.API;
using Presentation.Web.Models;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Controllers
{
    public class ItSystemControllerTest : KitosRestControllerApiTestWithAutofixture
    {
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IGenericRepository<ItSystem>> _systemRepository;
        private readonly ItSystemController _sut;

        public ItSystemControllerTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IGenericRepository<ItSystem>>();
            _sut = new ItSystemController(
                _systemRepository.Object,
                Mock.Of<IGenericRepository<TaskRef>>(),
                Mock.Of<IItSystemService>(),
                new ReferenceService(),
                _authorizationContext.Object
                );

            SetupControllerFrorTest(_sut);
        }

        [Fact]
        public void GetAccessRights_Returns_Forbidden_If_No_Access_In_Organization()
        {
            //Arrange
            ExpectAllowReadInOrganization(false);

            //Act
            var accessRights = _sut.GetAccessRights(true);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, accessRights.StatusCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetAccessRights_With_Organization_Access_Returns_Based_On_AccessRights(bool allowCreate)
        {
            //Arrange
            ExpectAllowReadInOrganization(true);
            ExpectAllowCreateReturns(allowCreate);

            //Act
            var responseMessage = _sut.GetAccessRights(true);

            //Assert
            var dto = ExpectResponseOf<EntitiesAccessRightsDTO>(responseMessage);

            Assert.True(dto.CanView);
            Assert.Equal(allowCreate, dto.CanCreate);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, true)]
        [InlineData(true, false, true)]
        public void GetAccessRightsForEntity_Returns_Based_On_AccessRights(bool allowRead, bool allowModify, bool allowDelete)
        {
            //Arrange
            var id = A<int>();
            var itSystem = new ItSystem();
            _systemRepository.Setup(x => x.GetByKey(id)).Returns(itSystem);
            ExpectAllowReadReturns(allowRead, itSystem);
            ExpectAllowModifyReturns(allowModify, itSystem);
            ExpectAllowDeleteReturns(allowDelete, itSystem);

            //Act
            var responseMessage = _sut.GetAccessRightsForEntity(id,true);

            //Assert
            var dto = ExpectResponseOf<EntityAccessRightsDTO>(responseMessage);

            Assert.Equal(allowRead, dto.CanView);
            Assert.Equal(allowModify, dto.CanEdit);
            Assert.Equal(allowDelete, dto.CanDelete);
        }

        private void ExpectAllowDeleteReturns(bool allowDelete, ItSystem itSystem)
        {
            _authorizationContext.Setup(x => x.AllowDelete(itSystem)).Returns(allowDelete);
        }

        private void ExpectAllowModifyReturns(bool allowModify, ItSystem itSystem)
        {
            _authorizationContext.Setup(x => x.AllowModify(itSystem)).Returns(allowModify);
        }

        private void ExpectAllowReadReturns(bool allowRead, ItSystem itSystem)
        {
            _authorizationContext.Setup(x => x.AllowReads(itSystem)).Returns(allowRead);
        }

        private void ExpectAllowCreateReturns(bool allowWrite)
        {
            _authorizationContext.Setup(x => x.AllowCreate<ItSystem>()).Returns(allowWrite);
        }

        private void ExpectAllowReadInOrganization(bool success)
        {
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(It.IsAny<int>()))
                .Returns(success ? OrganizationDataReadAccessLevel.All : OrganizationDataReadAccessLevel.None);
        }
    }
}
