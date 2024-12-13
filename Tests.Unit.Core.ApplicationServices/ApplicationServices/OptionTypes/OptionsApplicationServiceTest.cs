using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.Organization;

using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainServices.Model.Options;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.OptionTypes
{
    public class OptionsApplicationServiceTest : WithAutoFixture
    {
        private readonly OptionsApplicationService<ItSystem, BusinessType> _sut;

        private readonly Mock<IOptionsService<ItSystem, BusinessType>> _businessTypeOptionService;
        private readonly Mock<IOrganizationRepository> _organizationRepository;
        private readonly Mock<IAuthorizationContext> _authorizationContext;

        public OptionsApplicationServiceTest()
        {
            _businessTypeOptionService = new Mock<IOptionsService<ItSystem, BusinessType>>();
            _organizationRepository = new Mock<IOrganizationRepository>();
            _authorizationContext = new Mock<IAuthorizationContext>();

            _sut = new OptionsApplicationService<ItSystem, BusinessType>(_businessTypeOptionService.Object, _authorizationContext.Object, _organizationRepository.Object);
        }

        [Fact]
        public void GetAvailableOptions_Returns_Options_If_Options_Exists()
        {
            //Arrange
            var uuid = Guid.NewGuid();
            var orgId = A<int>();
            var numberOfBusinessTypes = A<int>() % 10;
            var optionDescriptors = new List<OptionDescriptor<BusinessType>>();
            for(int i = 0; i<numberOfBusinessTypes; i++)
            {
                optionDescriptors.Add( new OptionDescriptor<BusinessType>(CreateBusinessType(),A<string>()));
            }

            _organizationRepository.Setup(x => x.GetByUuid(uuid)).Returns(new Organization() { Id = orgId });
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgId)).Returns(OrganizationDataReadAccessLevel.All);
            _businessTypeOptionService.Setup(x => x.GetAvailableOptionsDetails(orgId)).Returns(optionDescriptors);

            //Act
            var businessTypes = _sut.GetOptionTypes(uuid);

            //Assert
            Assert.True(businessTypes.Ok);
            Assert.Equal(numberOfBusinessTypes, businessTypes.Value.Count());

            AssertSortedByPriority(businessTypes.Value);
            VerifyOptionsMatch(optionDescriptors, businessTypes.Value);

        }

        [Fact]
        public void GetAvailableOptions_Returns_Empty_List_If_No_Options_Exists()
        {
            //Arrange
            var uuid = Guid.NewGuid();
            var orgId = A<int>();

            _organizationRepository.Setup(x => x.GetByUuid(uuid)).Returns(new Organization() { Id = orgId });
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgId)).Returns(OrganizationDataReadAccessLevel.All);
            _businessTypeOptionService.Setup(x => x.GetAvailableOptions(orgId)).Returns(new List<BusinessType>() { });

            //Act
            var businessTypes = _sut.GetOptionTypes(uuid);

            //Assert
            Assert.True(businessTypes.Ok);
            Assert.Empty(businessTypes.Value);
        }

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.None)]
        [InlineData(OrganizationDataReadAccessLevel.RightsHolder)]
        public void GetAvailableOptions_Returns_Forbidden_If_Not_OrganizationDataReadAccessLevel_Is_All(OrganizationDataReadAccessLevel authContextReturn)
        {
            //Arrange
            var uuid = Guid.NewGuid();
            var orgId = A<int>();

            _organizationRepository.Setup(x => x.GetByUuid(uuid)).Returns(new Organization() { Id = orgId });
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgId)).Returns(authContextReturn);

            //Act
            var businessTypes = _sut.GetOptionTypes(uuid);

            //Assert
            Assert.True(businessTypes.Failed);
            Assert.Equal(OperationFailure.Forbidden, businessTypes.Error.FailureType);
        }

        [Fact]
        public void GetAvailableOptions_Returns_NotFound_If_No_Organization_With_Uuid_Is_Found()
        {
            //Arrange
            var uuid = Guid.NewGuid();
            var orgId = A<int>();

            _organizationRepository.Setup(x => x.GetByUuid(uuid)).Returns(Maybe<Organization>.None);

            //Act
            var businessTypes = _sut.GetOptionTypes(uuid);

            //Assert
            Assert.True(businessTypes.Failed);
            Assert.Equal(OperationFailure.NotFound, businessTypes.Error.FailureType);
        }

        [Fact]
        public void GetBusinessType_Returns_BusinessType_With_IsAvailable_Property_Set()
        {
            //Arrange
            var orgUuid = Guid.NewGuid();
            var orgId = A<int>();
            var typeUuid = Guid.NewGuid();
            var businessType = new OptionDescriptor<BusinessType>(CreateBusinessType(typeUuid),A<string>());
            var isAvailable = A<bool>();

            _organizationRepository.Setup(x => x.GetByUuid(orgUuid)).Returns(new Organization() { Id = orgId });
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgId)).Returns(OrganizationDataReadAccessLevel.All);
            _businessTypeOptionService.Setup(x => x.GetAllOptionsDetails(orgId)).Returns((businessType, isAvailable).WrapAsEnumerable());

            //Act
            var businessTypeResult = _sut.GetOptionType(orgUuid, typeUuid);

            //Assert
            Assert.True(businessTypeResult.Ok);
            Assert.Equal(isAvailable, businessTypeResult.Value.available);
            Assert.Equal(businessType, businessTypeResult.Value.option);
        }

        [Fact]
        public void GetBusinessType_Returns_NotFound_If_No_Organization_With_Uuid_Is_Found()
        {
            //Arrange
            var orgUuid = Guid.NewGuid();
            var orgId = A<int>();
            var typeUuid = Guid.NewGuid();
            var businessType = CreateBusinessType();

            _organizationRepository.Setup(x => x.GetByUuid(orgUuid)).Returns(Maybe<Organization>.None);

            //Act
            var businessTypeResult = _sut.GetOptionType(orgUuid, typeUuid);

            //Assert
            Assert.True(businessTypeResult.Failed);
            Assert.Equal(OperationFailure.NotFound, businessTypeResult.Error.FailureType);
        }

        [Theory]
        [InlineData(OrganizationDataReadAccessLevel.Public)]
        [InlineData(OrganizationDataReadAccessLevel.None)]
        [InlineData(OrganizationDataReadAccessLevel.RightsHolder)]
        public void GetBusinessType_Returns_Forbidden_If_Not_OrganizationDataReadAccessLevel_Is_All(OrganizationDataReadAccessLevel authContextReturn)
        {
            //Arrange
            var orgUuid = Guid.NewGuid();
            var orgId = A<int>();
            var typeUuid = Guid.NewGuid();
            var businessType = CreateBusinessType();

            _organizationRepository.Setup(x => x.GetByUuid(orgUuid)).Returns(new Organization() { Id = orgId });
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgId)).Returns(authContextReturn);

            //Act
            var businessTypeResult = _sut.GetOptionType(orgUuid, typeUuid);

            //Assert
            Assert.True(businessTypeResult.Failed);
            Assert.Equal(OperationFailure.Forbidden, businessTypeResult.Error.FailureType);
        }

        [Fact]
        public void GetBusinessType_Returns_NotFound_If_No_BusinessType_With_Uuid_Is_Found()
        {
            //Arrange
            var orgUuid = Guid.NewGuid();
            var orgId = A<int>();
            var typeUuid = Guid.NewGuid();
            var wrongUuid = Guid.NewGuid();
            var businessType = new OptionDescriptor<BusinessType>(CreateBusinessType(typeUuid), A<string>());
            var isAvailable = A<bool>();

            _organizationRepository.Setup(x => x.GetByUuid(orgUuid)).Returns(new Organization() { Id = orgId });
            _authorizationContext.Setup(x => x.GetOrganizationReadAccessLevel(orgId)).Returns(OrganizationDataReadAccessLevel.All);
            _businessTypeOptionService.Setup(x => x.GetAllOptionsDetails(orgId)).Returns((businessType, isAvailable).WrapAsEnumerable());

            //Act
            var businessTypeResult = _sut.GetOptionType(orgUuid, wrongUuid);

            //Assert
            Assert.True(businessTypeResult.Failed);
            Assert.Equal(OperationFailure.NotFound, businessTypeResult.Error.FailureType);
        }

        private BusinessType CreateBusinessType(Guid? uuid = null)
        {
            return new BusinessType
            {
                Id = A<int>(),
                Uuid = uuid ?? Guid.NewGuid(),
                Priority = A<int>()
            };
        }

        private static void AssertSortedByPriority(IEnumerable<OptionDescriptor<BusinessType>> sortedOptions)
        {
            var list = sortedOptions.ToList();
            for (int i = 0; i < list.Count - 1; i++)
            {
                var currentPriority = list[i].Option.Priority;
                var nextPriority = list[i + 1].Option.Priority;
                Assert.True(currentPriority >= nextPriority);
            }
        }

        private static void VerifyOptionsMatch(List<OptionDescriptor<BusinessType>> expectedOptions, IEnumerable<OptionDescriptor<BusinessType>> actualOptions)
        {
            foreach (var expected in expectedOptions)
            {
                var actual = actualOptions.SingleOrDefault(x => x.Option.Uuid == expected.Option.Uuid);
                Assert.NotNull(actual);
                Assert.Equal(expected.Description, actual.Description);
            }
        }

    }
}
