using Core.Abstractions.Helpers;
using Core.Abstractions.Types;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Organizations.Write;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Core.DomainModel.SupplierAssociatedFields;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Suppliers;
using Infrastructure.Services.DataAccess;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class OrganizationSupplierServiceTest : WithAutoFixture
    {
        private readonly Mock<IGenericRepository<OrganizationSupplier>> _organizationSupplierRepository;
        private readonly Mock<IOrganizationService> _organizationService;
        private readonly Mock<IEntityIdentityResolver> _entityIdentityResolver;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<ISupplierFieldDomainService> _supplierFieldDomainService;
        private readonly Mock<IOrganizationRepository> _organizationRepository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly OrganizationSupplierService _sut;

        private readonly string _fieldWithDefaultSupplierControl =
            ObjectHelper.GetPropertyPath<DataProcessingRegistration>(x => x.IsOversightCompleted);
        private readonly string _fieldWithDefaultSharedControl =
            ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightReportLinkName);

        public OrganizationSupplierServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _organizationSupplierRepository = new Mock<IGenericRepository<OrganizationSupplier>>();
            _organizationService = new Mock<IOrganizationService>();
            _entityIdentityResolver = new Mock<IEntityIdentityResolver>();
            _transactionManager = new Mock<ITransactionManager>();
            _supplierFieldDomainService = new Mock<ISupplierFieldDomainService>();
            _organizationRepository = new Mock<IOrganizationRepository>();
            _sut = new OrganizationSupplierService(_organizationSupplierRepository.Object,
                _organizationService.Object, 
                _entityIdentityResolver.Object, 
                _transactionManager.Object,
                _supplierFieldDomainService.Object,
                _organizationRepository.Object,
                _authorizationContext.Object);
        }

        [Fact]
        public void GetSupplierFieldConfigurations_Returns_DefaultConfigurations_If_No_Available()
        {
            var expected = SupplierAssociatedFields.DefaultConfiguration;
            var orgUuid = A<Guid>();
            _organizationService.Setup(x => x.GetOrganization(orgUuid, null))
                .Returns(new Organization { Uuid = orgUuid });
            _supplierFieldDomainService.Setup(x => x.GetSupplierAssociatedFieldConfigurations(orgUuid))
                .Returns(Maybe<IEnumerable<SupplierAssociatedFieldConfiguration>>.None);

            var result = _sut.GetSupplierFieldConfigurations(orgUuid);
            Assert.True(result.Ok);
            var config = result.Value;
            Assert.Equal(expected.Count, config.Count);
            foreach (var expectedConfig in expected)
            {
                var actualConfig = config.SingleOrDefault(x => x.FieldKey == expectedConfig.FieldKey);
                Assert.NotNull(actualConfig);
                Assert.Equal(expectedConfig.ControlState, actualConfig.ControlState);
            }
        }

        [Fact]
        public void GetSupplierFieldConfigurations_OverridesDefaultsWithOrganizationalConfiguration()
        {
            var expectedControlState = FieldControlState.Organization;
            var organizationalConfig = new HashSet<SupplierAssociatedFieldConfiguration>
            {
                new SupplierAssociatedFieldConfiguration
                {
                    FieldKey = _fieldWithDefaultSupplierControl,
                    ControlState = expectedControlState
                },
                new SupplierAssociatedFieldConfiguration
                {
                    FieldKey = _fieldWithDefaultSharedControl,
                    ControlState = expectedControlState
                }
            };
            var orgUuid = A<Guid>();
            _organizationService.Setup(x => x.GetOrganization(orgUuid, null))
                .Returns(new Organization { Uuid = orgUuid });
            _supplierFieldDomainService.Setup(_ => _.GetSupplierAssociatedFieldConfigurations(orgUuid))
                .Returns(organizationalConfig);

            var getResult = _sut.GetSupplierFieldConfigurations(orgUuid);

            Assert.True(getResult.Ok);
            var result = getResult.Value;

            var overriddenFieldsFromResult = result.Where(x => x.FieldKey == _fieldWithDefaultSupplierControl || x.FieldKey == _fieldWithDefaultSharedControl).ToList();
            Assert.Equal(organizationalConfig.Count, overriddenFieldsFromResult.Count);
            Assert.Contains(overriddenFieldsFromResult, x => x.FieldKey == _fieldWithDefaultSupplierControl && x.ControlState == expectedControlState);
            Assert.Contains(overriddenFieldsFromResult, x => x.FieldKey == _fieldWithDefaultSharedControl && x.ControlState == expectedControlState);
            
            var nonOverriddenFieldsFromResult = result.Where(x => x.FieldKey != _fieldWithDefaultSupplierControl && x.FieldKey != _fieldWithDefaultSharedControl).ToList();
            var defaultFields = SupplierAssociatedFields.DefaultConfiguration.Where(x => x.FieldKey != _fieldWithDefaultSupplierControl && x.FieldKey != _fieldWithDefaultSharedControl).ToList();
            Assert.Equal(defaultFields.Count, nonOverriddenFieldsFromResult.Count);
            foreach (var expectedConfig in defaultFields)
            {
                var actualConfig = nonOverriddenFieldsFromResult.SingleOrDefault(x => x.FieldKey == expectedConfig.FieldKey);
                Assert.NotNull(actualConfig);
                Assert.Equal(expectedConfig.ControlState, actualConfig.ControlState);
            }
        }

        [Fact]
        public void Can_Upsert_Supplier_Field_Configurations()
        {
            var organizationUuid = A<Guid>();
            var current = new HashSet<SupplierAssociatedFieldConfiguration>
            {
                new SupplierAssociatedFieldConfiguration
                {
                    FieldKey = _fieldWithDefaultSupplierControl,
                    ControlState = FieldControlState.Organization
                }
            };
            _organizationService.Setup(x => x.GetOrganization(organizationUuid, null))
                .Returns(new Organization
                {
                    Uuid = organizationUuid,
                    SupplierAssociatedFieldConfigurations = new List<SupplierAssociatedFieldConfiguration>(current)
                });
            _authorizationContext.Setup(x => x.AllowModify(It.IsAny<Organization>())).Returns(true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            var result = _sut.UpsertSupplierFieldConfigurations(
                organizationUuid,
                new SupplierAssociatedFieldConfigurationUpdateParameters
                {
                    Configurations = new[]
                    {
                        new SupplierAssociatedFieldConfiguration
                        {
                            FieldKey = _fieldWithDefaultSupplierControl,
                            ControlState = FieldControlState.Supplier
                        }
                    }
                });

            Assert.True(result.Ok);
            var updated = Assert.Single(result.Value);
            Assert.Equal(FieldControlState.Supplier, updated.ControlState);
            _organizationRepository.Verify(x => x.Update(It.Is<Organization>(o =>
                o.Uuid == organizationUuid &&
                o.SupplierAssociatedFieldConfigurations != null &&
                o.SupplierAssociatedFieldConfigurations.Any(c =>
                    c.FieldKey == _fieldWithDefaultSupplierControl &&
                    c.ControlState == FieldControlState.Supplier))), Times.Once);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Get_Using_Organizations_Returns_NotFound_If_Supplier_Not_Found()
        {
            var supplierUuid = A<Guid>();
            var supplierId = A<int>();
            var usingOrganizationUuid = A<Guid>();
            var usingOrganization = new Organization
            {
                Uuid = usingOrganizationUuid,
                Id = A<int>()
            };
            _organizationSupplierRepository.Setup(x => x.AsQueryable()).Returns(new List<OrganizationSupplier>
            {
                new OrganizationSupplier
                {
                    Supplier = new Organization { Uuid = supplierUuid },
                    SupplierId = supplierId,
                    Organization = usingOrganization,
                    OrganizationId = usingOrganization.Id,
                }
            }.AsQueryable());
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(supplierUuid)).Returns(Maybe<int>.None);

            var result = _sut.GetUsingOrganizations(supplierUuid);

            Assert.True(result.Failed);
            var error = result.Error;
            Assert.Equal(OperationFailure.NotFound, error.FailureType);
        }

        [Fact]
        public void Can_Get_Using_Organizations()
        {
            var supplierUuid = A<Guid>();
            var supplierId = A<int>();
            var usingOrganizationUuid = A<Guid>();
            var usingOrganization = new Organization
            {
                Uuid = usingOrganizationUuid,
                Id = A<int>()
            };
            _organizationSupplierRepository.Setup(x => x.AsQueryable()).Returns(new List<OrganizationSupplier>
            {
                new OrganizationSupplier
                {
                    Supplier = new Organization { Uuid = supplierUuid },
                    SupplierId = supplierId,
                    Organization = usingOrganization,
                    OrganizationId = usingOrganization.Id,
                }
            }.AsQueryable());
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(supplierUuid)).Returns(supplierId);

            var result = _sut.GetUsingOrganizations(supplierUuid);

            Assert.True(result.Ok);
            var value = Assert.Single(result.Value);
            Assert.Equal(usingOrganizationUuid, value.Uuid);
        }

        [Fact]
        public void Can_Get_Suppliers()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var supplierUuid = A<Guid>();
            var supplier = new Organization
            {
                Uuid = supplierUuid
            };
            var organization = new Organization
            {
                Uuid = organizationUuid,
                Suppliers = new List<OrganizationSupplier> { new OrganizationSupplier { Supplier = supplier } }
            };
            var orgSupplier = new OrganizationSupplier
            {
                Organization = organization,
                Supplier = supplier
            };

            _organizationSupplierRepository.Setup(x => x.GetWithReferencePreload(x => x.Supplier))
                .Returns(new List<OrganizationSupplier> { orgSupplier }.AsQueryable());

            //Act
            var result = _sut.GetSuppliersForOrganization(organizationUuid);

            //Assert
            Assert.True(result.Ok);
            var supplierResult = Assert.Single(result.Value);
            Assert.Equal(supplierUuid, supplierResult.Supplier.Uuid);
        }

        [Fact]
        public void Can_Get_Available_Suppliers()
        {
            //Arrange
            var organizationUuid = A<Guid>();

            var supplierUuid = A<Guid>();


            var suppliers = new List<Organization>
            {
                new Organization
                {
                    Uuid = supplierUuid
                }
            };

            _organizationService
                .Setup(x => x.SearchAccessibleOrganizations(false, It.IsAny<IDomainQuery<Organization>[]>()))
                .Returns(suppliers.AsQueryable());

            //Act
            var result = _sut.GetAvailableSuppliers(organizationUuid);

            //Assert
            Assert.True(result.Ok);
            var supplierResult = Assert.Single(result.Value);
            Assert.Equal(supplierUuid, supplierResult.Uuid);
        }

        [Fact]
        public void Can_Add_Supplier()
        {
            //Arrange
            var transaction = new Mock<IDatabaseTransaction>();
            var organizationUuid = A<Guid>();
            var supplierUuid = A<Guid>();
            var organizationId = A<int>();
            var supplierId = A<int>();
            var organization = new Organization
            {
                Id = organizationId,
                Uuid = organizationUuid
            };
            var supplier = new Organization
            {
                Id = supplierId,
                Uuid = supplierUuid
            };
            var organizationSupplier = new OrganizationSupplier
            {
                Organization = organization,
                Supplier = supplier
            };

            _organizationService.Setup(x => x.GetOrganization(organizationUuid, null))
                .Returns(organization);
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(organizationUuid))
                .Returns(organizationId);
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(supplierUuid))
                .Returns(supplierId);
            _organizationSupplierRepository.Setup(x => x.Insert(It.IsAny<OrganizationSupplier>()))
                .Returns(organizationSupplier);
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var result = _sut.AddSupplierToOrganization(organizationUuid, supplierUuid);
            //Assert
            Assert.True(result.Ok);
            var supplierResult = result.Value;
            Assert.Equal(organizationId, supplierResult.Organization.Id);
            Assert.Equal(supplierId, supplierResult.Supplier.Id);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Add_Supplier_Fails_If_Supplier_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var supplierUuid = A<Guid>();
            var organizationId = A<int>();
            var organization = new Organization
            {
                Id = organizationId,
                Uuid = organizationUuid
            };

            _organizationService.Setup(x => x.GetOrganization(organizationUuid, null))
                .Returns(organization);
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(organizationUuid))
                .Returns(organizationId);
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(supplierUuid))
                .Returns(Maybe<int>.None);

            //Act
            var result = _sut.AddSupplierToOrganization(organizationUuid, supplierUuid);
            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Add_Supplier_Fails_If_Organization_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var supplierUuid = A<Guid>();
            var organization = new Organization
            {
                Uuid = organizationUuid
            };

            _organizationService.Setup(x => x.GetOrganization(organizationUuid, null))
                .Returns(organization);
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(organizationUuid))
                .Returns(Maybe<int>.None);

            //Act
            var result = _sut.AddSupplierToOrganization(organizationUuid, supplierUuid);
            //Assert
            Assert.True(result.Failed);

        }

        [Fact]
        public void Add_Supplier_Fails_If_Organization_Already_Has_Supplier()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var supplierUuid = A<Guid>();
            var organization = new Organization
            {
                Uuid = organizationUuid,
                Suppliers = new List<OrganizationSupplier>{ new OrganizationSupplier{ Supplier = new Organization {Uuid = supplierUuid}} }
            };

            _organizationService.Setup(x => x.GetOrganization(organizationUuid, null))
                .Returns(organization);

            //Act
            var result = _sut.AddSupplierToOrganization(organizationUuid, supplierUuid);
            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.BadState, result.Error.FailureType);
        }

        [Fact]
        public void Can_Delete_Supplier()
        {
            //Arrange
            var transaction = new Mock<IDatabaseTransaction>();
            var organizationUuid = A<Guid>();
            var supplierUuid = A<Guid>();
            var organizationId = A<int>();
            var supplierId = A<int>();

            var organizationSupplier = new OrganizationSupplier
            {
                OrganizationId = organizationId,
                SupplierId = supplierId
            };

            _organizationSupplierRepository.Setup(x => x.AsQueryable())
                .Returns(new List<OrganizationSupplier>{ organizationSupplier }.AsQueryable());
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(organizationUuid))
                .Returns(organizationId);
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(supplierUuid))
                .Returns(supplierId);
            _organizationSupplierRepository.Setup(x => x.Delete(It.IsAny<OrganizationSupplier>()));
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            //Act
            var result = _sut.RemoveSupplierFromOrganization(organizationUuid, supplierUuid);

            //Assert
            Assert.True(result.IsNone);
            transaction.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public void Delete_Supplier_If_OrganizationUuid_Is_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var supplierUuid = A<Guid>();
            var organizationId = A<int>();
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(organizationUuid))
                .Returns(organizationId);
            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(supplierUuid))
                .Returns(Maybe<int>.None);

            //Act
            var result = _sut.RemoveSupplierFromOrganization(organizationUuid, supplierUuid);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }

        [Fact]
        public void Delete_Supplier_If_SupplierUuid_Is_NotFound()
        {
            //Arrange
            var organizationUuid = A<Guid>();
            var supplierUuid = A<Guid>();

            _entityIdentityResolver.Setup(x => x.ResolveDbId<Organization>(organizationUuid))
                .Returns(Maybe<int>.None);

            //Act
            var result = _sut.RemoveSupplierFromOrganization(organizationUuid, supplierUuid);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(OperationFailure.NotFound, result.Value.FailureType);
        }
    }
}
