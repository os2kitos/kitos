using System;
using System.Collections.Generic;
using AutoFixture;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Organizations.Write;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Infrastructure.Services.DataAccess;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Organizations
{
    public class OrganizationUnitWriteServiceTest : WithAutoFixture
    {
        private readonly OrganizationUnitWriteService _sut;
        private readonly Mock<ITransactionManager> _transactionManagerMock;
        private readonly Mock<IDomainEvents> _domainEventsMock;
        private readonly Mock<IDatabaseControl> _databaseControlMock;
        private readonly Mock<IOrganizationUnitService> _organizationServiceMock;
        private readonly Mock<IAuthorizationContext> _authorizationContextMock;

        public OrganizationUnitWriteServiceTest()
        {
            _transactionManagerMock = new Mock<ITransactionManager>();
            _domainEventsMock = new Mock<IDomainEvents>();
            _databaseControlMock = new Mock<IDatabaseControl>();
            _organizationServiceMock = new Mock<IOrganizationUnitService>();
            _authorizationContextMock = new Mock<IAuthorizationContext>();
            _sut = new OrganizationUnitWriteService(_transactionManagerMock.Object,
                _domainEventsMock.Object,
                _organizationServiceMock.Object,
                _authorizationContextMock.Object,
                _databaseControlMock.Object);
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            fixture.Register(() => new OrganizationUnitUpdateParameters
            {
                Name = A<string>().AsChangedValue(),
                Origin = A<OrganizationUnitOrigin>().AsChangedValue(),
                ParentUuid = A<Guid>().FromNullable().AsChangedValue(),
                Ean = A<long>().FromNullable().AsChangedValue(),
                LocalId = A<string>().FromNullable().AsChangedValue()

            });
        }

        [Fact]
        public void CreateNewUnit_Returns_Ok()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<OrganizationUnitUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var name = inputParameters.Name.NewValue;
            var origin = inputParameters.Origin.NewValue;
            var parentUnit = new OrganizationUnit { Uuid = inputParameters.ParentUuid.NewValue.Value };
            var organization = new Organization { Id = orgDbId, Uuid = organizationUuid, OrgUnits = new List<OrganizationUnit> { parentUnit } };
            var ean = inputParameters.Ean.NewValue;
            var localId = inputParameters.LocalId.NewValue;

            parentUnit.Organization = organization;
            var unit = new OrganizationUnit { Name = name, Origin = origin, Parent = parentUnit, Organization = organization, Ean = ean.HasValue ? ean.Value : null, LocalId = localId.HasValue ? localId.Value : null };

            ExpectCreateUnitReturns(organizationUuid, parentUnit.Uuid, name, origin, unit);
            ExpectWithWriteAccessReturns(unit, true);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Ok);
            transactionMock.Verify(x => x.Commit(), Times.AtLeastOnce);
        }

        [Fact]
        public void CreateNewUnit_Returns_Forbidden()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<OrganizationUnitUpdateParameters>();
            var orgDbId = A<int>();
            var name = inputParameters.Name.NewValue;
            var origin = inputParameters.Origin.NewValue;
            var parentUnit = new OrganizationUnit { Uuid = inputParameters.ParentUuid.NewValue.Value };
            var organization = new Organization { Id = orgDbId, Uuid = organizationUuid, OrgUnits = new List<OrganizationUnit> { parentUnit } };
            parentUnit.Organization = organization;
            var unit = new OrganizationUnit { Name = name, Origin = origin, Parent = parentUnit, Organization = organization };

            ExpectTransactionBegins();
            ExpectCreateUnitReturns(organizationUuid, parentUnit.Uuid, name, origin, unit);
            ExpectWithWriteAccessReturns(unit, false);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void CreateNewUnit_Returns_Error_When_Failed_Create()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<OrganizationUnitUpdateParameters>();
            var orgDbId = A<int>();
            var name = inputParameters.Name.NewValue;
            var origin = inputParameters.Origin.NewValue;
            var parentUnit = new OrganizationUnit { Uuid = inputParameters.ParentUuid.NewValue.Value };
            var organization = new Organization { Id = orgDbId, Uuid = organizationUuid, OrgUnits = new List<OrganizationUnit> { parentUnit } };
            parentUnit.Organization = organization;

            var operationError = A<OperationError>();

            ExpectTransactionBegins();
            ExpectCreateUnitReturns(organizationUuid, parentUnit.Uuid, name, origin, operationError);

            //Act
            var result = _sut.Create(organizationUuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(operationError.FailureType, result.Error.FailureType);
        }

        [Fact]
        public void PatchUnit_Returns_Ok()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<OrganizationUnitUpdateParameters>();
            var transactionMock = ExpectTransactionBegins();
            var orgDbId = A<int>();
            var name = inputParameters.Name.NewValue;
            var origin = inputParameters.Origin.NewValue;
            var ean = inputParameters.Ean.NewValue;
            var localId = inputParameters.LocalId.NewValue;

            var parentUnit = new OrganizationUnit { Uuid = inputParameters.ParentUuid.NewValue.Value };
            var unit = new OrganizationUnit { Uuid = A<Guid>(), Name = name, Origin = origin, Parent = parentUnit, Ean = ean.HasValue ? ean.Value : null, LocalId = localId.HasValue ? localId.Value : null };
            var organization = new Organization { Id = orgDbId, Uuid = organizationUuid, OrgUnits = new List<OrganizationUnit> { parentUnit, unit } };

            parentUnit.Organization = organization;
            unit.Organization = organization;

            ExpectGetOrganizationAndAuthorizeModificationReturns(organizationUuid, organization);
            ExpectWithWriteAccessReturns(unit, true);

            //Act
            var result = _sut.Patch(organizationUuid, unit.Uuid, inputParameters);

            //Assert
            Assert.True(result.Ok);
            var updatedUnit = result.Value;
            transactionMock.Verify(x => x.Commit(), Times.AtLeastOnce);
            Assert.Equal(inputParameters.Origin.NewValue, updatedUnit.Origin);
            Assert.Equal(inputParameters.Name.NewValue, updatedUnit.Name);
            Assert.Equal(inputParameters.ParentUuid.NewValue, updatedUnit.Parent.Uuid);
        }

        [Fact]
        public void PatchUnit_Returns_Error_When_Patch_Fails()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var inputParameters = A<OrganizationUnitUpdateParameters>();
            ExpectTransactionBegins();
            var name = inputParameters.Name.NewValue;
            var origin = inputParameters.Origin.NewValue;
            var unit = new OrganizationUnit { Uuid = A<Guid>(), Name = name, Origin = origin };

            var error = CreateOperationError();

            ExpectGetOrganizationAndAuthorizeModificationReturns(organizationUuid, error);
            ExpectWithWriteAccessReturns(unit, true);

            //Act
            var result = _sut.Patch(organizationUuid, unit.Uuid, inputParameters);

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(error.FailureType, result.Error.FailureType);
        }

        private Mock<IDatabaseTransaction> ExpectTransactionBegins()
        {
            var transactionMock = new Mock<IDatabaseTransaction>();
            _transactionManagerMock.Setup(x => x.Begin()).Returns(transactionMock.Object);
            return transactionMock;
        }

        private void ExpectGetOrganizationAndAuthorizeModificationReturns(Guid orgUuid,
            Result<Organization, OperationError> result)
        {
            _organizationServiceMock.Setup(service => service.GetOrganizationAndAuthorizeModification(orgUuid))
                .Returns(result);
        }

        private void ExpectCreateUnitReturns(Guid organizationUuid, Guid parentUuid, string name,
            OrganizationUnitOrigin origin, Result<OrganizationUnit, OperationError> result)
        {
            _organizationServiceMock.Setup(service =>
                service.Create(organizationUuid, parentUuid, name, origin)).Returns(result);
        }

        private void ExpectDeleteUnitReturns(Guid organizationUuid, Guid organizationUnitUuid, Maybe<OperationError> result)
        {
            _organizationServiceMock.Setup(service => service.Delete(organizationUuid, organizationUnitUuid))
                .Returns(result);
        }

        private void ExpectWithWriteAccessReturns(IEntity entity,
            bool result)
        {
            _authorizationContextMock.Setup(mock => mock.AllowModify(entity)).Returns(result);
        }

        private OperationError CreateOperationError()
        {
            return new OperationError(A<OperationFailure>());
        }
    }
}
