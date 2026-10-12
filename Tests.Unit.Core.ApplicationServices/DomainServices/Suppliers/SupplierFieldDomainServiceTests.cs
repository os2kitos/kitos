using Core.Abstractions.Helpers;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.SupplierAssociatedFields;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Suppliers;
using Moq;
using System;
using System.Collections.Generic;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Suppliers
{
    public class SupplierFieldDomainServiceTests : WithAutoFixture
    {
        private SupplierFieldDomainService _sut;

        private Mock<IOrganizationRepository> _organizationRepository;

        private readonly string _isOversightCompleted =
            ObjectHelper.GetPropertyPath<DataProcessingRegistration>(x => x.IsOversightCompleted);

        private readonly string _oversightDate =
            ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightDate);

        private readonly string _remark =
            ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightRemark);

        private readonly string _oversightReportLink =
            ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightReportLink);

        private readonly string _oversightOptionId =
            ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightOptionId);

        private readonly string _containsAITechnology =
            ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.ContainsAITechnology);

        private readonly string _systemUsageCriticalityLevel =
            ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.SystemUsageCriticalityLevel);

        private readonly string _preriskAssessment =
            ObjectHelper.GetPropertyPath<ItSystemUsage>(x => x.preriskAssessment);

        private readonly string _oversightReportLinkName =
            ObjectHelper.GetPropertyPath<DataProcessingRegistrationOversightDate>(x => x.OversightReportLinkName);

        public SupplierFieldDomainServiceTests()
        {
            _organizationRepository = new Mock<IOrganizationRepository>();
            _sut = new SupplierFieldDomainService(_organizationRepository.Object);
        }

        [Fact]
        public void IsSupplierControlled_ShouldReturnTrue_ForDefaultSupplierControlledField_WhenNoOrganizationConfiguration()
        {
            var orgUuid = SetupExpectNoConfigurations();   
            // Act
            var result = _sut.IsSupplierControlled(_isOversightCompleted, orgUuid);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetSupplierAssociatedFieldConfigurations_ShouldReturnNone_WhenOrganizationHasNoSupplierFieldConfigurations()
        {
            var orgUuid = A<Guid>();
            var organization = CreateOrganization();
            organization.Uuid = orgUuid;
            organization.SupplierAssociatedFieldConfigurations = [];
            ExpectOrganization(organization);

            var result = _sut.IsSupplierControlled(_isOversightCompleted, orgUuid);

            Assert.True(result);
        }

        [Fact]
        public void GetSupplierAssociatedFieldConfigurations_ShouldReturnConfigurations_WhenOrganizationHasSupplierFieldConfigurations()
        {
            var orgUuid = A<Guid>();
            var expected = new SupplierAssociatedFieldConfiguration
            {
                FieldKey = _oversightReportLinkName,
                ControlState = FieldControlState.Supplier
            };
            var organization = CreateOrganization();
            organization.Uuid = orgUuid;
            organization.SupplierAssociatedFieldConfigurations = [expected];
            ExpectOrganization(organization);

            var result = _sut.IsSupplierControlled(_oversightReportLinkName, orgUuid);

            Assert.True(result);
        }

        [Fact]
        public void IsSupplierControlled_ShouldReturnFalse_ForDefaultNonSupplierControlledField_WhenNoOrganizationConfiguration()
        {
            var orgUuid = SetupExpectNoConfigurations();

            // Arrange
            var nonSupplierControlledProperty = A<string>();
            // Act
            var result = _sut.IsSupplierControlled(nonSupplierControlledProperty, orgUuid);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSupplierControlled_ShouldOverrideSharedDefault_WhenOrganizationConfigurationExists()
        {
            var orgUuid = A<Guid>();
            var organization = CreateOrganization();
            organization.Uuid = orgUuid;
            organization.SupplierAssociatedFieldConfigurations = [new SupplierAssociatedFieldConfiguration { FieldKey = _oversightReportLinkName, ControlState = FieldControlState.Supplier }];
            ExpectOrganization(organization);
            
            var result = _sut.IsSupplierControlled(_oversightReportLinkName, orgUuid);
            
            Assert.True(result);
        }

        [Fact]
        public void IsSupplierControlled_ShouldOverrideSupplierDefault_WhenOrganizationConfigurationExists()
        {
            var expected = new SupplierAssociatedFieldConfiguration { FieldKey = _isOversightCompleted, ControlState = FieldControlState.Organization };
            var orgUuid = SetupExpectConfiguration(expected);
            var result = _sut.IsSupplierControlled(_isOversightCompleted, orgUuid);
            
            Assert.False(result);
        }

        [Fact]
        public void IsSupplierControlled_ShouldReturnTrue_ForDefaultSupplierControlledField_WhenOrganizationConfigurationExistsOnlyForOtherFields()
        {
            var expected = new SupplierAssociatedFieldConfiguration { FieldKey = _remark, ControlState = FieldControlState.Organization };
            var orgUuid = SetupExpectConfiguration(expected);

            var result = _sut.IsSupplierControlled(_isOversightCompleted, orgUuid);

            Assert.True(result);
        }

        [Fact]
        public void OnlySupplierFieldChanges_ShouldReturnTrue_WhenAllPropertiesAreSupplierControlled()
        {
            // Arrange
            var orgUuid = SetupExpectNoConfigurations();
            var properties = new[]
            {
                _isOversightCompleted, _remark
            };

            // Act
            var result = _sut.ContainsOnlySupplierControlledAndSharedFields(properties, orgUuid);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void OnlySupplierFieldChanges_ShouldReturnFalse_WhenAnyPropertyIsNotSupplierControlled()
        {
            // Arrange
            var orgUuid = SetupExpectNoConfigurations();
            var nonSupplierControlledProperty = A<string>();
            var properties = new[]
            {
                _isOversightCompleted, nonSupplierControlledProperty
            };
            // Act
            var result = _sut.ContainsOnlySupplierControlledAndSharedFields(properties, orgUuid);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsAnySupplierControlledFields_ShouldReturnTrue_WhenAnyPropertyIsDefaultSupplierControlled_AndNoOrganizationConfig()
        {
            // Arrange
            var orgUuid = SetupExpectNoConfigurations();
            var nonSupplierControlledProperty = A<string>();
            var properties = new[]
            {
                nonSupplierControlledProperty, _remark
            };
            // Act
            var result = _sut.ContainsAnySupplierControlledFields(properties, orgUuid);
            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ContainsAnySupplierControlledFields_ShouldReturnFalse_WhenAllSupplierDefaultsAreOverriddenToNonSupplier(bool useShared)
        {
            var state = useShared ? FieldControlState.Shared : FieldControlState.Organization;            var allSupplierDefaults = new[]
            {
                new SupplierAssociatedFieldConfiguration { FieldKey = _isOversightCompleted, ControlState = state },
                new SupplierAssociatedFieldConfiguration { FieldKey = _oversightDate, ControlState = state },
                new SupplierAssociatedFieldConfiguration { FieldKey = _remark, ControlState = state },
                new SupplierAssociatedFieldConfiguration { FieldKey = _oversightReportLink, ControlState = state },
                new SupplierAssociatedFieldConfiguration { FieldKey = _oversightOptionId, ControlState = state },
                new SupplierAssociatedFieldConfiguration { FieldKey = _containsAITechnology, ControlState = state },
                new SupplierAssociatedFieldConfiguration { FieldKey = _systemUsageCriticalityLevel, ControlState = state },
                new SupplierAssociatedFieldConfiguration { FieldKey = _preriskAssessment, ControlState = state }
            };
            var orgUuid = SetupExpectConfiguration(allSupplierDefaults);
            var properties = new[] { _isOversightCompleted, _oversightDate, _remark, _oversightReportLink, _oversightOptionId, _containsAITechnology, _systemUsageCriticalityLevel, _preriskAssessment };
            // Act
            var result = _sut.ContainsAnySupplierControlledFields(properties, orgUuid);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsAnySupplierControlledFields_ShouldReturnFalse_WhenNoPropertyIsDefaultSupplierControlled_AndNoOrganizationConfig()
        {
            // Arrange
            var orgUuid = SetupExpectNoConfigurations();
            var nonSupplierControlledProperty1 = A<string>();
            var nonSupplierControlledProperty2 = A<string>();
            var properties = new[]
            {
                nonSupplierControlledProperty1, nonSupplierControlledProperty2
            };
            // Act
            var result = _sut.ContainsAnySupplierControlledFields(properties, orgUuid);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsAnySupplierControlledFields_ShouldOverrideDefaultWithTrue_WhenAnyPropertyIsSupplierControlledInOrganizationConfig()
        {
            // Arrange
            var expected = new SupplierAssociatedFieldConfiguration { FieldKey = _oversightReportLinkName, ControlState = FieldControlState.Supplier };
            var orgUuid = SetupExpectConfiguration(expected);
            var nonSupplierControlledProperty = A<string>();
            var properties = new[]
            {
                nonSupplierControlledProperty, _oversightReportLinkName
            };
            // Act
            var result = _sut.ContainsAnySupplierControlledFields(properties, orgUuid);
            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(FieldControlState.Supplier, FieldControlState.Supplier)]
        [InlineData(FieldControlState.Shared, FieldControlState.Shared)]
        [InlineData(FieldControlState.Supplier, FieldControlState.Shared)]
        public void ContainsOnlySupplierControlledAndSharedFields_ShouldReturnTrue_WhenAllPropsInOrgConfigAreSupplierOrShared(
            FieldControlState state1, FieldControlState state2)
        {
            // Arrange
            var config1 = new SupplierAssociatedFieldConfiguration { FieldKey = _isOversightCompleted, ControlState = state1 };
            var config2 = new SupplierAssociatedFieldConfiguration { FieldKey = _oversightDate, ControlState = state2 };
            var orgUuid = SetupExpectConfiguration(config1, config2);
            var properties = new[] { _isOversightCompleted, _oversightDate };
            // Act
            var result = _sut.ContainsOnlySupplierControlledAndSharedFields(properties, orgUuid);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsOnlySupplierControlledAndSharedFields_ShouldReturnFalse_WhenAnyPropInOrgConfigIsOrganizationControlState()
        {
            // Arrange
            var sharedConfig = new SupplierAssociatedFieldConfiguration { FieldKey = _isOversightCompleted, ControlState = FieldControlState.Shared };
            var supplierConfig = new SupplierAssociatedFieldConfiguration { FieldKey = _oversightDate, ControlState = FieldControlState.Supplier };
            var organizationConfig = new SupplierAssociatedFieldConfiguration { FieldKey = _remark, ControlState = FieldControlState.Organization };
            var orgUuid = SetupExpectConfiguration(sharedConfig, supplierConfig, organizationConfig);
            var properties = new[] { _isOversightCompleted, _oversightDate, _remark };
            // Act
            var result = _sut.ContainsOnlySupplierControlledAndSharedFields(properties, orgUuid);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsOnlySupplierControlledAndSharedFields_ShouldReturnTrue_WhenNoOrgConfig_AndAllInDefaultMapAreSharedOrSupplier()
        {
            // Arrange
            var orgUuid = SetupExpectNoConfigurations();
            var properties = new[] { _isOversightCompleted, _oversightDate, _remark };
            // Act
            var result = _sut.ContainsOnlySupplierControlledAndSharedFields(properties, orgUuid);
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsOnlySupplierControlledAndSharedFields_ShouldReturnFalse_WhenOnePropertyMissingFromBothOrgConfigAndDefaultMap()
        {
            // Arrange
            var config1 = new SupplierAssociatedFieldConfiguration { FieldKey = _isOversightCompleted, ControlState = FieldControlState.Supplier };
            var config2 = new SupplierAssociatedFieldConfiguration { FieldKey = _oversightDate, ControlState = FieldControlState.Shared };
            var orgUuid = SetupExpectConfiguration(config1, config2);
            var unknownProperty = A<string>();
            var properties = new[] { _isOversightCompleted, _oversightDate, unknownProperty };
            // Act
            var result = _sut.ContainsOnlySupplierControlledAndSharedFields(properties, orgUuid);
            // Assert
            Assert.False(result);
        }

        private Guid SetupExpectConfiguration(params SupplierAssociatedFieldConfiguration[] configurations) {
            var orgUuid = A<Guid>();
            var organization = CreateOrganization();
            organization.Uuid = orgUuid;
            organization.SupplierAssociatedFieldConfigurations = new List<SupplierAssociatedFieldConfiguration>(configurations);
            ExpectOrganization(organization);
            return orgUuid;
        }

        private Guid SetupExpectNoConfigurations()
        {
            var orgUuid = A<Guid>();
            var organization = CreateOrganization();
            organization.Uuid = orgUuid;
            organization.SupplierAssociatedFieldConfigurations = [];
            ExpectOrganization(organization);
            return orgUuid;
        }

        private void ExpectOrganization(Organization organization)
        {
            _organizationRepository.Setup(x => x.GetByUuid(organization.Uuid)).Returns(organization);
        }

        private static Organization CreateOrganization()
        {
            return new Organization();
        }
    }
}
