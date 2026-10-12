using System;
using System.Collections.Generic;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Authorization
{
    public class FieldAuthorizationModelTest: WithAutoFixture
    {
        private readonly Mock<IOrganizationalUserContext> _activeUserContext;
        private readonly FieldAuthorizationModel _sut;
        private readonly Mock<ISupplierAssociatedFieldsService> _supplierAssociatedFieldsService;
        private readonly Mock<IAuthorizationContext> _authorizationContext;

        public FieldAuthorizationModelTest()
        {
            _activeUserContext = new Mock<IOrganizationalUserContext>();
            _supplierAssociatedFieldsService = new Mock<ISupplierAssociatedFieldsService>();
            _authorizationContext = new Mock<IAuthorizationContext>();
            _sut = new FieldAuthorizationModel(_activeUserContext.Object, _supplierAssociatedFieldsService.Object, _authorizationContext.Object);
        }

        [Fact]
        public void GivenEntityOrParametersIsNull_AuthorizeUpdate_Returns_False()
        {
            IsUserGlobalAdmin(false);
            var entity = SetupEntityWithIdAndSuppliers();
            var parameters = new Mock<ISupplierAssociatedEntityUpdateParameters>();

            var withNullEntity = _sut.AuthorizeUpdate(null, parameters.Object);
            var withNullParameters = _sut.AuthorizeUpdate(entity.Object, null);

            Assert.True(withNullEntity.Ok && !withNullEntity.Value);
            Assert.True(withNullParameters.Ok && !withNullParameters.Value);
        }

        [Fact]
        public void GivenUserIsGlobalAdmin_AuthorizeUpdate_Returns_True()
        {
            IsUserGlobalAdmin(true);
            var entity = SetupEntityWithIdAndSuppliers();
            var parameters = new Mock<ISupplierAssociatedEntityUpdateParameters>();
            var result = _sut.AuthorizeUpdate(entity.Object, parameters.Object);

            Assert.True(result.Ok && result.Value);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenOrgHasNoSuppliers_AuthorizeUpdate_Returns_AllowModifyResult(bool allowModifyResult)
        {
            IsUserGlobalAdmin(false);
            var entity = SetupEntityWithIdAndSuppliers(false);
            var parameters = new Mock<ISupplierAssociatedEntityUpdateParameters>();

            SetupAllowModifyReturns(allowModifyResult, entity.Object);
            var result = _sut.AuthorizeUpdate(entity.Object, parameters.Object);

            Assert.Equal(allowModifyResult, result.Ok && result.Value);
            VerifyAllowModify(entity.Object);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenSupplierApiUser_Has_Non_Supplier_Changes_AuthorizeUpdate_Returns_AllowModifyResult(bool allowModifyResult)
        {
            IsUserGlobalAdmin(false);
            var parameters = new Mock<ISupplierAssociatedEntityUpdateParameters>();
            var entity = SetupEntityWithIdAndSuppliers();

            ExpectHasOnlySupplierChangesReturns(false, parameters, entity.Object);
            SetupDoesUserHaveSupplierApiAccess(true, entity);
            SetupAllowModifyReturns(allowModifyResult, entity.Object);

            var result = _sut.AuthorizeUpdate(entity.Object, parameters.Object);

            Assert.Equal(allowModifyResult, result.Ok && result.Value);
            VerifyAllowModify(entity.Object);
        }

        [Fact]
        public void GivenSupplierApiUser_Has_Only_Supplier_Changes_AuthorizeUpdate_Returns_True()
        {
            IsUserGlobalAdmin(false);
            var parameters = new Mock<ISupplierAssociatedEntityUpdateParameters>();
            var entity = SetupEntityWithIdAndSuppliers();

            ExpectHasOnlySupplierChangesReturns(true, parameters, entity.Object);
            SetupDoesUserHaveSupplierApiAccess(true, entity);

            var result = _sut.AuthorizeUpdate(entity.Object, parameters.Object);

            Assert.True(result.Ok && result.Value);
        }

        [Fact]
        public void GivenNonSupplierApiUser_Has_Supplier_Changes_AuthorizeUpdate_Returns_False()
        {
            IsUserGlobalAdmin(false);
            var parameters = new Mock<ISupplierAssociatedEntityUpdateParameters>();
            var entity = SetupEntityWithIdAndSuppliers();

            ExpectHasAnySupplierChangesReturns(true, parameters, entity.Object);
            SetupDoesUserHaveSupplierApiAccess(false, entity);

            var result = _sut.AuthorizeUpdate(entity.Object, parameters.Object);

            Assert.True(result.Ok && !result.Value);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GivenNonSupplierApiUser_Has_No_Supplier_Changes_AuthorizeUpdate_Returns_AllowModifyResult(bool allowModifyResult)
        {
            IsUserGlobalAdmin(false);
            var parameters = new Mock<ISupplierAssociatedEntityUpdateParameters>();
            var entity = SetupEntityWithIdAndSuppliers();

            ExpectHasAnySupplierChangesReturns(false, parameters, entity.Object);
            SetupDoesUserHaveSupplierApiAccess(false, entity);
            SetupAllowModifyReturns(allowModifyResult, entity.Object);

            var result = _sut.AuthorizeUpdate(entity.Object, parameters.Object);

            Assert.Equal(allowModifyResult, result.Ok && result.Value);
            VerifyAllowModify(entity.Object);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetFieldPermissions_Returns_Expected_Result(bool expected)
        {
            var key = A<string>();
            var orgUuid = A<Guid>();

            IsUserGlobalAdmin(false);
            var entity = SetupEntityWithIdAndSuppliers();
            ExpectIsSupplierControlledFieldReturns(key, expected);

            var result = _sut.GetFieldPermissions(entity.Object, key, orgUuid);

        }

        private void VerifyAllowModify(IEntityOwnedByOrganization entity)
        {
            _authorizationContext.Verify(_ => _.AllowModify(entity), Times.Once);
        }

        private Mock<IEntityOwnedByOrganization> SetupEntityWithIdAndSuppliers(bool hasSuppliers = true)
        {
            var entityId = A<int>();
            var entity = new Mock<IEntityOwnedByOrganization>();
            entity.Setup(_ => _.Id).Returns(entityId);
            
            var suppliers = new List<OrganizationSupplier>();
            if (hasSuppliers)
            {
                suppliers.Add(new OrganizationSupplier());
            }
            entity.Setup(e => e.Organization).Returns(new Organization
            {
                Suppliers = suppliers
            });
            
            return entity;
        }

        private void SetupAllowModifyReturns(bool result, IEntityOwnedByOrganization entity)
        {
            _authorizationContext.Setup(_ => _.AllowModify(entity)).Returns(result);
        }

        private void IsUserGlobalAdmin(bool value)
        {
            _activeUserContext.Setup(x => x.IsGlobalAdmin()).Returns(value);
        }

        private void ExpectHasAnySupplierChangesReturns(bool expectedResult, Mock<ISupplierAssociatedEntityUpdateParameters> parameters, IEntity entity)
        {
            _supplierAssociatedFieldsService.Setup(_ => _.HasAnySupplierChanges(parameters.Object, entity, It.IsAny<Guid>())).Returns(expectedResult);
        }

        private void ExpectHasOnlySupplierChangesReturns(bool expectedResult, Mock<ISupplierAssociatedEntityUpdateParameters> parameters, IEntity entity)
        {
            _supplierAssociatedFieldsService.Setup(_ => _.HasOnlySupplierChanges(parameters.Object, entity, It.IsAny<Guid>())).Returns(expectedResult);
        }

        private void ExpectIsSupplierControlledFieldReturns(string key, bool result)
        {
            _supplierAssociatedFieldsService.Setup(x => x.IsFieldSupplierControlled(key, It.IsAny<Guid>())).Returns(result);
        }
        private void SetupDoesUserHaveSupplierApiAccess(bool value, Mock<IEntityOwnedByOrganization> entity)
        {
            var supplierId = A<int>();
            entity.SetupGet(e => e.Organization).Returns(new Organization
            {
                Suppliers = new List<OrganizationSupplier>
                {
                    new() { SupplierId = supplierId }
                }
            });
            _activeUserContext.Setup(_ => _.OrganizationIds).Returns(new List<int>(){A<int>()});
            _activeUserContext.Setup(_ => _.IsSupplierApiUserForOrganizationWithSuppliers(It.IsAny<IEnumerable<int>>()))
                .Returns(value);
       }
    }
}
