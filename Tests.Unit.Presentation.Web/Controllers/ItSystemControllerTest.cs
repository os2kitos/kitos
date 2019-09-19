using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Moq;
using Presentation.Web.Controllers.API;
using Presentation.Web.Models;
using Presentation.Web.Models.ItSystem;
using Presentation.Web.Models.Result;
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Controllers
{
    public class ItSystemControllerTest : KitosRestControllerApiTestWithAutofixture
    {
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<IGenericRepository<ItSystem>> _systemRepository;
        private readonly ItSystemController _sut;
        private readonly Mock<IItSystemService> _systemService;

        public ItSystemControllerTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _systemRepository = new Mock<IGenericRepository<ItSystem>>();
            _systemService = new Mock<IItSystemService>();
            _sut = new ItSystemController(
                _systemRepository.Object,
                Mock.Of<IGenericRepository<TaskRef>>(),
                _systemService.Object,
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
            var responseMessage = _sut.GetAccessRightsForEntity(id, true);

            //Assert
            var dto = ExpectResponseOf<EntityAccessRightsDTO>(responseMessage);

            Assert.Equal(allowRead, dto.CanView);
            Assert.Equal(allowModify, dto.CanEdit);
            Assert.Equal(allowDelete, dto.CanDelete);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_NotFound()
        {
            //Arrange
            var itSystemId = A<int>();
            ExpectGetUsingOrganizationsReturn(itSystemId, Result<OperationResult, IReadOnlyList<UsingOrganization>>.Fail(OperationResult.NotFound));

            //Act
            var responseMessage = _sut.GetUsingOrganizations(itSystemId);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_Forbidden()
        {
            //Arrange
            var itSystemId = A<int>();
            ExpectGetUsingOrganizationsReturn(itSystemId, Result<OperationResult, IReadOnlyList<UsingOrganization>>.Fail(OperationResult.Forbidden));

            //Act
            var responseMessage = _sut.GetUsingOrganizations(itSystemId);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, responseMessage.StatusCode);
        }

        [Theory]
        [InlineData(OperationResult.Conflict)]
        [InlineData(OperationResult.BadInput)]
        [InlineData(OperationResult.UnknownError)]
        public void GetUsingOrganizations_Returns_Failed_OperationResult(OperationResult operationResult)
        {
            //Arrange
            var itSystemId = A<int>();
            ExpectGetUsingOrganizationsReturn(itSystemId, Result<OperationResult, IReadOnlyList<UsingOrganization>>.Fail(operationResult));

            //Act
            var responseMessage = _sut.GetUsingOrganizations(itSystemId);

            //Assert
            Assert.Equal(HttpStatusCode.InternalServerError, responseMessage.StatusCode);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_Ok()
        {
            //Arrange
            var itSystemId = A<int>();
            var usingOrganizations = Many<UsingOrganization>().ToList();

            ExpectGetUsingOrganizationsReturn(itSystemId, Result<OperationResult, IReadOnlyList<UsingOrganization>>.Ok(usingOrganizations));

            //Act
            var responseMessage = _sut.GetUsingOrganizations(itSystemId);

            //Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            var dtos = ExpectResponseOf<IEnumerable<UsingOrganizationDTO>>(responseMessage);
            Assert.True(
                usingOrganizations.Select(x => new
                {
                    usageId = x.ItSystemUsageId,
                    org = new
                    {
                        id = x.Organization.Id,
                        name = x.Organization.Name
                    }
                })
                    .SequenceEqual(
                        dtos.Select(x => new
                        {
                            usageId = x.SystemUsageId,
                            org = new
                            {
                                id = x.Organization.Id,
                                name = x.Organization.Name
                            }
                        })));
        }

        [Theory]
        [InlineData(SystemDeleteResult.InUse, SystemDeleteConflict.InUse)]
        [InlineData(SystemDeleteResult.HasChildren, SystemDeleteConflict.HasChildren)]
        [InlineData(SystemDeleteResult.HasInterfaceExhibits, SystemDeleteConflict.HasInterfaceExhibits)]
        public void Delete_Returns_Conflict_With_SystemDeleteResults(SystemDeleteResult deleteResult, SystemDeleteConflict deleteConflict)
        {
            //Arrange
            var systemId = A<int>();
            _systemService.Setup(x => x.Delete(systemId))
                .Returns(deleteResult);

            //Act
            var responseMessage = _sut.Delete(systemId, 0); // OrgId is not used in this function.

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, responseMessage.StatusCode);
            var responseValue = ExpectResponseOf<string>(responseMessage);
            Assert.Equal(deleteConflict, Enum.Parse(typeof(SystemDeleteConflict), responseValue));
        }

        [Theory]
        [InlineData(HttpStatusCode.Forbidden, SystemDeleteResult.Forbidden)]
        [InlineData(HttpStatusCode.Conflict, SystemDeleteResult.InUse)]
        [InlineData(HttpStatusCode.Conflict, SystemDeleteResult.HasChildren)]
        [InlineData(HttpStatusCode.Conflict, SystemDeleteResult.HasInterfaceExhibits)]
        [InlineData(HttpStatusCode.InternalServerError, SystemDeleteResult.UnknownError)]
        public void Delete_Returns_Failed(HttpStatusCode code, SystemDeleteResult result)
        {
            //Arrange
            var systemId = A<int>();
            _systemService.Setup(x => x.Delete(systemId))
                .Returns(result);

            //Act
            var responseMessage = _sut.Delete(systemId, 0); // OrgId is not used in this function.

            //Assert
            Assert.Equal(code, responseMessage.StatusCode);
        }

        [Fact]
        public void Delete_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            _systemService.Setup(x => x.Delete(systemId))
                .Returns(SystemDeleteResult.Forbidden);

            //Act
            var responseMessage = _sut.Delete(systemId, 0); // OrgId is not used in this function.

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, responseMessage.StatusCode);
        }

        private void ExpectGetUsingOrganizationsReturn(
            int itSystemId,
            Result<OperationResult, IReadOnlyList<UsingOrganization>> result)
        {
            _systemService.Setup(x => x.GetUsingOrganizations(itSystemId))
                .Returns(result);
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
