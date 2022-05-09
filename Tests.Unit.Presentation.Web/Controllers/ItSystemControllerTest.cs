﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Time;
using Moq;
using Presentation.Web.Controllers.API.V1;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.ItSystem;
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
                Mock.Of<IOperationClock>(x => x.Now == DateTime.Now)
                );

            SetupControllerFrorTest(_sut);
            _sut.AuthorizationContext = _authorizationContext.Object;
        }

        [Fact]
        public void GetAccessRights_Returns_Forbidden_If_No_Access_In_Organization()
        {
            //Arrange
            var organizationId = A<int>();
            ExpectAllowReadInOrganization(organizationId, false);

            //Act
            var accessRights = _sut.GetAccessRights(true, organizationId);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, accessRights.StatusCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetAccessRights_With_Organization_Access_Returns_Based_On_AccessRights(bool allowCreate)
        {
            //Arrange
            var organizationId = A<int>();
            ExpectAllowReadInOrganization(organizationId, true);
            ExpectAllowCreateReturns(organizationId, allowCreate);

            //Act
            var responseMessage = _sut.GetAccessRights(true, organizationId);

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
            ExpectGetUsingOrganizationsReturn(itSystemId, Result<IReadOnlyList<UsingOrganization>, OperationFailure>.Failure(OperationFailure.NotFound));

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
            ExpectGetUsingOrganizationsReturn(itSystemId, Result<IReadOnlyList<UsingOrganization>, OperationFailure>.Failure(OperationFailure.Forbidden));

            //Act
            var responseMessage = _sut.GetUsingOrganizations(itSystemId);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, responseMessage.StatusCode);
        }

        [Theory]
        [InlineData(OperationFailure.Conflict, HttpStatusCode.Conflict)]
        [InlineData(OperationFailure.BadInput, HttpStatusCode.BadRequest)]
        [InlineData(OperationFailure.UnknownError, HttpStatusCode.InternalServerError)]
        public void GetUsingOrganizations_Returns_Failed_OperationResult(OperationFailure operationResult, HttpStatusCode expectedStatusCode)
        {
            //Arrange
            var itSystemId = A<int>();
            ExpectGetUsingOrganizationsReturn(itSystemId, Result<IReadOnlyList<UsingOrganization>, OperationFailure>.Failure(operationResult));

            //Act
            var responseMessage = _sut.GetUsingOrganizations(itSystemId);

            //Assert
            Assert.Equal(expectedStatusCode, responseMessage.StatusCode);
        }

        [Fact]
        public void GetUsingOrganizations_Returns_Ok()
        {
            //Arrange
            var itSystemId = A<int>();
            var usingOrganizations = Many<UsingOrganization>().ToList();

            ExpectGetUsingOrganizationsReturn(itSystemId, Result<IReadOnlyList<UsingOrganization>, OperationFailure>.Success(usingOrganizations));

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
            ExpectDeleteSystemReturn(systemId, deleteResult);

            //Act
            var responseMessage = DeleteSystem(systemId);

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
        [InlineData(HttpStatusCode.NotFound, SystemDeleteResult.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError, SystemDeleteResult.UnknownError)]
        public void Delete_Returns_Failed(HttpStatusCode code, SystemDeleteResult result)
        {
            //Arrange
            var systemId = A<int>();
            ExpectDeleteSystemReturn(systemId, result);

            //Act
            var responseMessage = DeleteSystem(systemId);

            //Assert
            Assert.Equal(code, responseMessage.StatusCode);
        }

        [Fact]
        public void Delete_Returns_Ok()
        {
            //Arrange
            var systemId = A<int>();
            ExpectDeleteSystemReturn(systemId, SystemDeleteResult.Ok);

            //Act
            var responseMessage = DeleteSystem(systemId);

            //Assert
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
        }

        private HttpResponseMessage DeleteSystem(int systemId)
        {
            return _sut.Delete(systemId, 0); // OrgId is only a route qualifier and is therefore not used.
        }

        private void ExpectGetUsingOrganizationsReturn(
            int itSystemId,
            Result<IReadOnlyList<UsingOrganization>, OperationFailure> result)
        {
            _systemService.Setup(x => x.GetUsingOrganizations(itSystemId))
                .Returns(result);
        }

        private void ExpectDeleteSystemReturn(int systemId, SystemDeleteResult deleteResult)
        {
            _systemService.Setup(x => x.Delete(systemId,false))
                .Returns(deleteResult);
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

        private void ExpectAllowCreateReturns(int organizationId, bool allowWrite)
        {
            _authorizationContext.Setup(x => x.AllowCreate<ItSystem>(organizationId)).Returns(allowWrite);
        }

        private void ExpectAllowReadInOrganization(int organizationId, bool success)
        {
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(organizationId))
                .Returns(success ? OrganizationDataReadAccessLevel.All : OrganizationDataReadAccessLevel.None);
        }
    }
}
