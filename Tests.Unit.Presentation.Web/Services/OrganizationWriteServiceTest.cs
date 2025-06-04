using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Organizations;
using Core.DomainModel.Organization;
using Core.DomainServices.Repositories.Organization;
using Infrastructure.Services.DataAccess;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Organizations.Write;
using Xunit;
using Core.DomainModel.Events;
using Core.DomainServices.Generic;
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.ApplicationServices.Model.UiCustomization;
using Core.DomainModel;
using Core.DomainServices;
using Tests.Unit.Presentation.Web.Extensions;

namespace Tests.Unit.Presentation.Web.Services
{
    public class OrganizationWriteServiceTest: OrganizationServiceTestBase
    {
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IOrganizationRepository> _organizationRepository;
        private readonly Mock<IOrganizationService> _organizationService;
        private readonly Mock<IEntityIdentityResolver> _identityResolver;
        private readonly Mock<IGenericRepository<ContactPerson>> _contactPersonRepository;
        private readonly Mock<IGenericRepository<DataResponsible>> _dataResponsibleRepository;
        private readonly Mock<IGenericRepository<DataProtectionAdvisor>> _dataProtectionAdvisorRepository;
        private readonly Mock<IGenericRepository<CountryCode>> _countryCodeRepository;
        private readonly OrganizationWriteService _sut;


        public OrganizationWriteServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _transactionManager = new Mock<ITransactionManager>();
            _domainEvents = new Mock<IDomainEvents>();  
            _organizationRepository = new Mock<IOrganizationRepository>();
            _organizationService = new Mock<IOrganizationService>();
            _identityResolver = new Mock<IEntityIdentityResolver>();
            _contactPersonRepository = new Mock<IGenericRepository<ContactPerson>>();
            _dataResponsibleRepository = new Mock<IGenericRepository<DataResponsible>>();
            _dataProtectionAdvisorRepository = new Mock<IGenericRepository<DataProtectionAdvisor>>();
            _countryCodeRepository = new Mock<IGenericRepository<CountryCode>>();

            _sut = new OrganizationWriteService(_transactionManager.Object,
                _domainEvents.Object,
                _organizationService.Object,
                _authorizationContext.Object,
                _organizationRepository.Object,
                _identityResolver.Object,
                _contactPersonRepository.Object,
                _dataResponsibleRepository.Object,
                _dataProtectionAdvisorRepository.Object,
                _countryCodeRepository.Object);
        }

        [Fact]
        public void Can_Patch_UI_Root_Config()
        {
            var (org, updateParameters) = SetupPatchUIRootConfigTest();
            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _authorizationContext.Setup(_ => _.AllowModify(org)).Returns(true);

            var result = _sut.PatchUIRootConfig(org.Uuid, updateParameters);

            Assert.True(result.Ok);
            var uiRootConfig = result.Value;
            Assert.Equal(updateParameters.ShowDataProcessing.NewValue.Value, uiRootConfig.ShowDataProcessing);
            Assert.Equal(updateParameters.ShowItSystemModule.NewValue.Value, uiRootConfig.ShowItSystemModule);
            Assert.Equal(updateParameters.ShowItContractModule.NewValue.Value, uiRootConfig.ShowItContractModule);
            _organizationRepository.Verify(_ => _.Update(It.IsAny<Organization>()));
        }

        [Fact]
        public void Patch_UI_Root_Config_Returns_Not_Found_If_No_Org()
        {
            var (org, updateParameters) = SetupPatchUIRootConfigTest();

            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(new OperationError(OperationFailure.NotFound));
            _authorizationContext.Setup(_ => _.AllowModify(org)).Returns(true);

            var result = _sut.PatchUIRootConfig(org.Uuid, updateParameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Patch_UI_Root_Config_Returns_Forbidden_If_Missing_Write_Access()
        {
            var (org, updateParameters) = SetupPatchUIRootConfigTest();
            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _authorizationContext.Setup(_ => _.AllowModify(org)).Returns(false);

            var result = _sut.PatchUIRootConfig(org.Uuid, updateParameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        private (Organization, UIRootConfigUpdateParameters) SetupPatchUIRootConfigTest()
        {
            var orgUuid = A<Guid>();
            var org = new Organization() { Uuid = orgUuid, Config = Config.Default(new User()) };
            var updateParameters = new UIRootConfigUpdateParameters()
            {
                ShowItSystemModule = Maybe<bool>.Some(A<bool>()).AsChangedValue(),
                ShowDataProcessing = Maybe<bool>.Some(A<bool>()).AsChangedValue(),
                ShowItContractModule = Maybe<bool>.Some(A<bool>()).AsChangedValue()
            };
            return (org, updateParameters);
        }

        [Fact]
        public void Cannot_Update_Master_Data_If_No_Modify_Rights()
        {
            var organizationUuid = A<Guid>();
            var organization = new Mock<Organization>();
            _authorizationContext.Setup(x => x.AllowModify(It.IsAny<Organization>())).Returns(false);
            _authorizationContext.Setup(_ => _.AllowReads(It.IsAny<Organization>())).Returns(true);
            _organizationService.Setup(_ => _.GetOrganization(organizationUuid, null)).Returns(organization.Object);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            var newCvr = OptionalValueChange<Maybe<string>>.With(A<Maybe<string>>());
            var updateParameters = new OrganizationMasterDataUpdateParameters
            {
                Cvr = newCvr,
            };

            var result = _sut.PatchMasterData(organizationUuid, updateParameters);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Cannot_Update_Master_Data_If_No_Invalid_Uuid()
        {
            var invalidOrganizationUuid = A<Guid>();
            _organizationService.Setup(_ => _.GetOrganization(invalidOrganizationUuid, null)).Returns(new OperationError(OperationFailure.NotFound));
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object); _authorizationContext.Setup(x => x.AllowModify(It.IsAny<Organization>())).Returns(true);
            _authorizationContext.Setup(_ => _.AllowReads(It.IsAny<Organization>())).Returns(true);
            var newCvr = OptionalValueChange<Maybe<string>>.With(A<Maybe<string>>());
            var updateParameters = new OrganizationMasterDataUpdateParameters
            {
                Cvr = newCvr,
            };

            var result = _sut.PatchMasterData(invalidOrganizationUuid, updateParameters);
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Can_Update_Master_Data_With_Data()
        {
            var organizationUuid = A<Guid>();
            var organization = new Mock<Organization>();
            _organizationService.Setup(_ => _.GetOrganization(organizationUuid, null)).Returns(organization.Object);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object); _authorizationContext.Setup(x => x.AllowModify(It.IsAny<Organization>())).Returns(true);
            _authorizationContext.Setup(_ => _.AllowReads(It.IsAny<Organization>())).Returns(true);
            _organizationService.Setup(_ => _.CanActiveUserModifyCvr(It.IsAny<Guid>())).Returns(true);
            var newCvr = OptionalValueChange<Maybe<string>>.With(Maybe<string>.Some(A<string>()));
            var newPhone = OptionalValueChange<Maybe<string>>.With(Maybe<string>.Some(A<string>()));
            var newAddress = OptionalValueChange<Maybe<string>>.With(Maybe<string>.Some(A<string>()));
            var newEmail = OptionalValueChange<Maybe<string>>.With(Maybe<string>.Some(A<string>()));
            var updateParameters = new OrganizationMasterDataUpdateParameters
            {
                Cvr = newCvr,
                Phone = newPhone,
                Address = newAddress,
                Email = newEmail
            };

            var result = _sut.PatchMasterData(organizationUuid, updateParameters);
            Assert.True(result.Ok);

            var updatedOrganization = result.Value;
            Assert.Equal(newCvr.NewValue.Value, updatedOrganization.Cvr);
            Assert.Equal(newPhone.NewValue.Value, updatedOrganization.Phone);
            Assert.Equal(newAddress.NewValue.Value, updatedOrganization.Adress);
            Assert.Equal(newEmail.NewValue.Value, updatedOrganization.Email);
            _organizationRepository.Verify(_ => _.Update(organization.Object));
        }

        [Fact]
        public void Can_Update_Master_Data_With_Null()
        {
            var organizationUuid = A<Guid>();
            var organization = new Mock<Organization>();
            _authorizationContext.Setup(x => x.AllowModify(It.IsAny<Organization>())).Returns(true);
            _authorizationContext.Setup(_ => _.AllowReads(It.IsAny<Organization>())).Returns(true);
            _organizationService.Setup(_ => _.GetOrganization(organizationUuid, null)).Returns(organization.Object);
            _organizationService.Setup(_ => _.CanActiveUserModifyCvr(It.IsAny<Guid>())).Returns(true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            var updateParameters = new OrganizationMasterDataUpdateParameters()
            {
                Address = Maybe<string>.None.AsChangedValue(),
                Cvr = Maybe<string>.None.AsChangedValue(),
                Email = Maybe<string>.None.AsChangedValue(),
                Phone = Maybe<string>.None.AsChangedValue()
            };

            var result = _sut.PatchMasterData(organizationUuid, updateParameters);
            Assert.True(result.Ok);

            var updatedOrganization = result.Value;
            Assert.Null(updatedOrganization.Cvr);
            Assert.Null(updatedOrganization.Phone);
            Assert.Null(updatedOrganization.Adress);
            Assert.Null(updatedOrganization.Email);
            _organizationRepository.Verify(_ => _.Update(organization.Object));
        }

        [Fact]
        public void Update_Master_Data_Roles_Returns_Not_Found_If_Invalid_Uuid()
        {
            var invalidOrganizationUuid = A<Guid>();
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(invalidOrganizationUuid))
                .Returns(Maybe<int>.None);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            _organizationService.Setup(_ => _.GetOrganization(invalidOrganizationUuid, null)).Returns(new OperationError(OperationFailure.NotFound));

            var result =
                _sut.PatchOrganizationMasterDataRoles(invalidOrganizationUuid, new OrganizationMasterDataRolesUpdateParameters());

            Assert.True(result.Failed);
            var error = result.Error;
            Assert.Equal(OperationFailure.NotFound, error.FailureType);
        }

        [Fact]
        public void Update_Master_Data_Roles_Returns_Forbidden_If_Unauthorized_To_Modify_Root_Organization()
        {
            var org = GetOrgAndSetupForVerifyUnauthorized();
            var orgId = org.Id;
            var updateParameters = SetupUpdateMasterDataRoles(orgId);
            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<Organization>()))
                .Returns(false);

            var result =
                _sut.PatchOrganizationMasterDataRoles(org.Uuid, updateParameters);

            Assert.True(result.Failed);
            var error = result.Error;
            Assert.Equal(OperationFailure.Forbidden, error.FailureType);
        }

        public Organization GetOrgAndSetupForVerifyUnauthorized()
        {
            var org = CreateOrganization();
            var orgId = org.Id;
            var dataProtectionAdvisor = new DataProtectionAdvisor();
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(org.Uuid))
                .Returns(orgId);
            _organizationService.Setup(_ => _.GetDataProtectionAdvisor(orgId)).Returns(dataProtectionAdvisor);
            var dataResponsible = new DataResponsible();
            _organizationService.Setup(_ => _.GetDataResponsible(orgId)).Returns(dataResponsible);
            var contactPerson = new ContactPerson();
            _organizationService.Setup(_ => _.GetContactPerson(orgId))
                .Returns(contactPerson);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            return org;
        }

        [Fact]
        public void Can_Update_Master_Data_Roles()
        {
            var org = CreateOrganization();
            var orgId = org.Id;
            var expectedContactPerson = SetupGetMasterDataRolesContactPerson(orgId);
            var expectedDataResponsible = SetupGetMasterDataRolesDataResponsible(orgId);
            var expectedDataProtectionAdvisor = SetupGetMasterDataRolesDataProtectionAdvisor(orgId);
            var updateParameters = SetupUpdateMasterDataRoles(orgId, expectedContactPerson, expectedDataResponsible, expectedDataProtectionAdvisor);
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(org.Uuid))
                .Returns(expectedContactPerson.OrganizationId);

            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<Organization>()))
                .Returns(true);
            _organizationService.Setup(_ => _.GetContactPerson(orgId)).Returns(expectedContactPerson);
            _organizationService.Setup(_ => _.GetDataResponsible(orgId)).Returns(expectedDataResponsible);
            _organizationService.Setup(_ => _.GetDataProtectionAdvisor(orgId)).Returns(expectedDataProtectionAdvisor);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var result = _sut.PatchOrganizationMasterDataRoles(org.Uuid, updateParameters);

            Assert.True(result.Ok);
            var value = result.Value;
            AssertContactPerson(expectedContactPerson, value.ContactPerson);
            AssertDataResponsible(expectedDataResponsible, value.DataResponsible);
            AssertDataProtectionAdvisor(expectedDataProtectionAdvisor, value.DataProtectionAdvisor);
        }

        [Fact]
        public void Update_Creates_Master_Data_Roles_If_Not_Found()
        {
            var org = CreateOrganization();
            var orgId = org.Id;
            var expectedContactPerson = SetupGetMasterDataRolesContactPerson(orgId);
            var expectedDataResponsible = SetupGetMasterDataRolesDataResponsible(orgId);
            var expectedDataProtectionAdvisor = SetupGetMasterDataRolesDataProtectionAdvisor(orgId);
            var updateParameters = GetRolesUpdateParameters(expectedContactPerson, expectedDataResponsible,
                expectedDataProtectionAdvisor);

            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(org.Uuid))
                .Returns(expectedContactPerson.OrganizationId);

            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<Organization>()))
                .Returns(true);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataProtectionAdvisor>()))
                .Returns(true);
            _organizationService.Setup(_ => _.GetContactPerson(orgId)).Returns(Maybe<ContactPerson>.None);
            _organizationService.Setup(_ => _.GetDataResponsible(orgId)).Returns(Maybe<DataResponsible>.None);
            _organizationService.Setup(_ => _.GetDataProtectionAdvisor(orgId)).Returns(Maybe<DataProtectionAdvisor>.None);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            _authorizationContext.Setup(_ => _.AllowCreate<ContactPerson>(orgId)).Returns(true);
            _authorizationContext.Setup(_ => _.AllowCreate<DataProtectionAdvisor>(orgId)).Returns(true);
            _authorizationContext.Setup(_ => _.AllowCreate<DataResponsible>(orgId)).Returns(true);

            var result = _sut.PatchOrganizationMasterDataRoles(org.Uuid, updateParameters);

            Assert.True(result.Ok);
            _contactPersonRepository.Verify(_ => _.Insert(It.IsAny<ContactPerson>()));
            _dataResponsibleRepository.Verify(_ => _.Insert(It.IsAny<DataResponsible>()));
            _dataProtectionAdvisorRepository.Verify(_ => _.Insert(It.IsAny<DataProtectionAdvisor>()));
            var value = result.Value;
            AssertContactPerson(expectedContactPerson, value.ContactPerson);
            AssertDataResponsible(expectedDataResponsible, value.DataResponsible);
            AssertDataProtectionAdvisor(expectedDataProtectionAdvisor, value.DataProtectionAdvisor);
        }

        [Fact]
        public void Can_Get_Master_Data_Roles()
        {
            var org = CreateOrganization();
            var orgId = org.Id;
            var expectedContactPerson = SetupGetMasterDataRolesContactPerson(orgId);
            var expectedDataResponsible = SetupGetMasterDataRolesDataResponsible(orgId);
            var expectedDataProtectionAdvisor = SetupGetMasterDataRolesDataProtectionAdvisor(orgId);
            _organizationService.Setup(_ => _.GetContactPerson(orgId)).Returns(expectedContactPerson);
            _organizationService.Setup(_ => _.GetDataResponsible(orgId)).Returns(expectedDataResponsible);
            _organizationService.Setup(_ => _.GetDataProtectionAdvisor(orgId)).Returns(expectedDataProtectionAdvisor);
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(org.Uuid))
                .Returns(orgId);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var rolesResult = _sut.GetOrCreateOrganizationMasterDataRoles(org.Uuid);

            Assert.True(rolesResult.Ok);
            var value = rolesResult.Value;
            AssertContactPerson(expectedContactPerson, value.ContactPerson);
            AssertDataResponsible(expectedDataResponsible, value.DataResponsible);
            AssertDataProtectionAdvisor(expectedDataProtectionAdvisor, value.DataProtectionAdvisor);
            Assert.Equal(org.Uuid, rolesResult.Value.OrganizationUuid);
        }

        [Fact]
        public void Get_Or_Create_Creates_Master_Data_Roles_If_not_Found()
        {
            var org = CreateOrganization();
            var orgId = org.Id;
            _organizationService.Setup(_ => _.GetContactPerson(orgId)).Returns(Maybe<ContactPerson>.None);
            _organizationService.Setup(_ => _.GetDataResponsible(orgId)).Returns(Maybe<DataResponsible>.None);
            _organizationService.Setup(_ => _.GetDataProtectionAdvisor(orgId)).Returns(Maybe<DataProtectionAdvisor>.None);
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(org.Uuid))
                .Returns(orgId);
            _authorizationContext.Setup(_ => _.AllowCreate<ContactPerson>(orgId)).Returns(true);
            _authorizationContext.Setup(_ => _.AllowCreate<DataProtectionAdvisor>(orgId)).Returns(true);
            _authorizationContext.Setup(_ => _.AllowCreate<DataResponsible>(orgId)).Returns(true);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var rolesResult = _sut.GetOrCreateOrganizationMasterDataRoles(org.Uuid);

            Assert.True(rolesResult.Ok);
            var value = rolesResult.Value;
            _contactPersonRepository.Verify(_ => _.Insert(It.IsAny<ContactPerson>()));
            _dataResponsibleRepository.Verify(_ => _.Insert(It.IsAny<DataResponsible>()));
            _dataProtectionAdvisorRepository.Verify(_ => _.Insert(It.IsAny<DataProtectionAdvisor>()));
            Assert.Equal(org.Uuid, rolesResult.Value.OrganizationUuid);
        }

        [Fact]
        public void Get_Master_Data_Roles_Returns_Bad_Input_If_Invalid_Uuid()
        {
            var invalidOrganizationUuid = A<Guid>();
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(invalidOrganizationUuid))
                .Returns(Maybe<int>.None);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var result = _sut.GetOrCreateOrganizationMasterDataRoles(invalidOrganizationUuid);

            Assert.True(result.Failed);
            var error = result.Error;
            Assert.Equal(OperationFailure.BadInput, error.FailureType);
        }

        [Fact]
        public void Can_Update_Organization()
        {
            var org = CreateOrganization();
            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _transactionManager.Setup(_ => _.Begin()).Returns(new Mock<IDatabaseTransaction>().Object);
            _authorizationContext.Setup(_ => _.AllowModify(org)).Returns(true);
            _organizationService.Setup(_ => _.CanActiveUserModifyCvr(org.Uuid)).Returns(true);
            var updateParams = A<OrganizationBaseParameters>();
            SetupRepositoryReturnsCountryCode(updateParams.ForeignCountryCodeUuid.NewValue);

            var result = _sut.PatchOrganization(org.Uuid, updateParams);

            Assert.True(result.Ok);
            var value = result.Value;
            Assert.Equal(updateParams.Cvr.NewValue, value.Cvr);
            Assert.Equal(updateParams.Name.NewValue, value.Name);
            Assert.Equal(updateParams.ForeignCountryCodeUuid.NewValue, value.ForeignCountryCode.Uuid);
            Assert.Equal(updateParams.TypeId.NewValue, value.TypeId);
        }

        [Fact]
        public void Update_Organization_Returns_Forbidden_If_Unauthorized_To_Modify_Cvr()
        {
            var org = CreateOrganization();
            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _transactionManager.Setup(_ => _.Begin()).Returns(new Mock<IDatabaseTransaction>().Object);
            _authorizationContext.Setup(_ => _.AllowModify(org)).Returns(true);
            _organizationService.Setup(_ => _.CanActiveUserModifyCvr(org.Uuid)).Returns(false);
            SetupRepositoryReturnsCountryCode();
            var updateParams = new OrganizationBaseParameters()
            {
                Cvr = OptionalValueChange<Maybe<string>>.With(A<string>().AsCvr()),
                Name = OptionalValueChange<Maybe<string>>.With(A<string>())
            };

            var result = _sut.PatchOrganization(org.Uuid, updateParams);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Update_Organization_Returns_Forbidden_If_Unauthorized_For_Org()
        {
            var org = CreateOrganization();
            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _transactionManager.Setup(_ => _.Begin()).Returns(new Mock<IDatabaseTransaction>().Object);
            _organizationService.Setup(_ => _.CanActiveUserModifyCvr(org.Uuid)).Returns(true);
            _authorizationContext.Setup(_ => _.AllowModify(org)).Returns(false);
            SetupRepositoryReturnsCountryCode();
            var updateParams = new OrganizationBaseParameters()
            {
                Cvr = OptionalValueChange<Maybe<string>>.With(A<string>().AsCvr()),
                Name = OptionalValueChange<Maybe<string>>.With(A<string>())
            };

            var result = _sut.PatchOrganization(org.Uuid, updateParams);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        [Fact]
        public void Update_Organization_Only_Checks_Cvr_Modify_Permission_If_Cvr_Has_Change()
        {
            var org = CreateOrganization();
            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _transactionManager.Setup(_ => _.Begin()).Returns(new Mock<IDatabaseTransaction>().Object);
            _organizationService.Setup(_ => _.CanActiveUserModifyCvr(org.Uuid)).Returns(true);
            _authorizationContext.Setup(_ => _.AllowModify(org)).Returns(true);
            var updateParams = A<OrganizationBaseParameters>();
            updateParams.Cvr = OptionalValueChange<Maybe<string>>.None;
            SetupRepositoryReturnsCountryCode(updateParams.ForeignCountryCodeUuid.NewValue);

            var result = _sut.PatchOrganization(org.Uuid, updateParams);

            Assert.True(result.Ok);
            Assert.Equal(updateParams.Name.NewValue, result.Value.Name);
            Assert.Equal(org.Cvr, result.Value.Cvr);
        }

        [Fact]
        public void Update_Organization_Returns_Not_Found_If_Requested_CountryCode_Not_Found()
        {
            var org = CreateOrganization();
            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(org);
            _transactionManager.Setup(_ => _.Begin()).Returns(new Mock<IDatabaseTransaction>().Object);
            _authorizationContext.Setup(_ => _.AllowModify(org)).Returns(true);
            _organizationService.Setup(_ => _.CanActiveUserModifyCvr(org.Uuid)).Returns(true);
            var updateParams = A<OrganizationBaseParameters>();
            _countryCodeRepository.Setup(_ => _.AsQueryable()).Returns(
                new List<CountryCode>().AsQueryable());

            var result = _sut.PatchOrganization(org.Uuid, updateParams);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        [Fact]
        public void Update_Organization_Returns_Not_Found_If_No_Org()
        {
            var org = CreateOrganization();
            _organizationService.Setup(_ => _.GetOrganization(org.Uuid, null)).Returns(Result<Organization, OperationError>.Failure(OperationFailure.NotFound));
            _transactionManager.Setup(_ => _.Begin()).Returns(new Mock<IDatabaseTransaction>().Object);
            var updateParams = new OrganizationBaseParameters()
            {
                Cvr = OptionalValueChange<Maybe<string>>.With(A<string>().AsCvr()),
                Name = OptionalValueChange<Maybe<string>>.With(A<string>())
            };

            var result = _sut.PatchOrganization(org.Uuid, updateParams);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        public enum RoleType
        {
            ContactPerson,
            DataResponsible,
            DataProtectionAdvisor
        }

        [Theory]
        [InlineData(RoleType.ContactPerson)]
        [InlineData(RoleType.DataResponsible)]
        [InlineData(RoleType.DataProtectionAdvisor)]
        public void Get_Or_Create_Creates_Returns_Forbidden_If_Missing_Role_Create_Rights(RoleType roleType)
        {
            var org = CreateOrganization();
            var orgId = org.Id;
            _organizationService.Setup(_ => _.GetContactPerson(orgId)).Returns(Maybe<ContactPerson>.None);
            _organizationService.Setup(_ => _.GetDataResponsible(orgId)).Returns(Maybe<DataResponsible>.None);
            _organizationService.Setup(_ => _.GetDataProtectionAdvisor(orgId)).Returns(Maybe<DataProtectionAdvisor>.None);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(org.Uuid))
                .Returns(orgId);
            switch (roleType)
            {
                case RoleType.ContactPerson:
                    _authorizationContext.Setup(_ => _.AllowCreate<ContactPerson>(orgId)).Returns(false);
                    _authorizationContext.Setup(_ => _.AllowCreate<DataProtectionAdvisor>(orgId)).Returns(true);
                    _authorizationContext.Setup(_ => _.AllowCreate<DataResponsible>(orgId)).Returns(true);
                    break;
                case RoleType.DataResponsible:
                    _authorizationContext.Setup(_ => _.AllowCreate<DataResponsible>(orgId)).Returns(false);
                    _authorizationContext.Setup(_ => _.AllowCreate<ContactPerson>(orgId)).Returns(true);
                    _authorizationContext.Setup(_ => _.AllowCreate<DataProtectionAdvisor>(orgId)).Returns(true);

                    break;
                case RoleType.DataProtectionAdvisor:
                    _authorizationContext.Setup(_ => _.AllowCreate<DataProtectionAdvisor>(orgId)).Returns(false);
                    _authorizationContext.Setup(_ => _.AllowCreate<ContactPerson>(orgId)).Returns(true);
                    _authorizationContext.Setup(_ => _.AllowCreate<DataResponsible>(orgId)).Returns(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(roleType), roleType, null);
            }

            var rolesResult = _sut.GetOrCreateOrganizationMasterDataRoles(org.Uuid);

            Assert.False(rolesResult.Ok);
            Assert.Equal(OperationFailure.Forbidden, rolesResult.Error);
        }

        [Fact]
        public void Can_Create_Organization()
        {
            var parameters = A<OrganizationBaseParameters>();
            _organizationService
                .Setup(service => service.CreateNewOrganization(It.IsAny<Organization>()))
                .Returns(Result<Organization, OperationFailure>.Success);
            SetupRepositoryReturnsCountryCode(parameters.ForeignCountryCodeUuid.NewValue);
            var result = _sut.CreateOrganization(parameters);

            Assert.True(result.Ok);
            var organization = result.Value;
            Assert.Equal(organization.Name, parameters.Name.NewValue);
            Assert.Equal(parameters.TypeId.NewValue, organization.TypeId);
            Assert.Equal(parameters.Cvr.NewValue, organization.Cvr);
            Assert.Equal(parameters.ForeignCountryCodeUuid.NewValue, organization.ForeignCountryCode.Uuid);
        }
        private void SetupRepositoryReturnsCountryCode(Guid? uuid = null)
        {
            var expectedCountryCode = new CountryCode()
            {
                Uuid = uuid ?? new Guid()
            };
            _countryCodeRepository.Setup(_ => _.AsQueryable()).Returns(
                new List<CountryCode>() { expectedCountryCode }.AsQueryable());
        }

        private OrganizationMasterDataRolesUpdateParameters SetupUpdateMasterDataRoles(int orgId,
            ContactPerson cp = null, DataResponsible dr = null, DataProtectionAdvisor dpa = null)
        {
            var expectedContactPerson = cp ?? SetupGetMasterDataRolesContactPerson(orgId);
            var expectedDataResponsible = dr ?? SetupGetMasterDataRolesDataResponsible(orgId);
            var expectedDataProtectionAdvisor = dpa ?? SetupGetMasterDataRolesDataProtectionAdvisor(orgId);
            return GetRolesUpdateParameters(expectedContactPerson, expectedDataResponsible,
                expectedDataProtectionAdvisor);
        }

        private OrganizationMasterDataRolesUpdateParameters GetRolesUpdateParameters(ContactPerson expectedContactPerson,
            DataResponsible expectedDataResponsible, DataProtectionAdvisor expectedDataProtectionAdvisor)
        {
            return new OrganizationMasterDataRolesUpdateParameters
            {
                ContactPerson = new ContactPersonUpdateParameters()
                {
                    Email = OptionalValueChange<Maybe<string>>.With(
            expectedContactPerson.Email != null
                ? Maybe<string>.Some(expectedContactPerson.Email)
                : Maybe<string>.None
        ),
                    Name = OptionalValueChange<Maybe<string>>.With(
            expectedContactPerson.Name != null
                ? Maybe<string>.Some(expectedContactPerson.Name)
                : Maybe<string>.None
        ),
                    LastName = OptionalValueChange<Maybe<string>>.With(
            expectedContactPerson.LastName != null
                ? Maybe<string>.Some(expectedContactPerson.LastName)
                : Maybe<string>.None
        ),
                    PhoneNumber = OptionalValueChange<Maybe<string>>.With(
            expectedContactPerson.PhoneNumber != null
                ? Maybe<string>.Some(expectedContactPerson.PhoneNumber)
                : Maybe<string>.None
        ),
                },
                DataResponsible = new DataResponsibleUpdateParameters()
                {
                    Email = OptionalValueChange<Maybe<string>>.With(
            expectedDataResponsible.Email != null
                ? Maybe<string>.Some(expectedDataResponsible.Email)
                : Maybe<string>.None
        ),
                    Name = OptionalValueChange<Maybe<string>>.With(
            expectedDataResponsible.Name != null
                ? Maybe<string>.Some(expectedDataResponsible.Name)
                : Maybe<string>.None
        ),
                    Cvr = OptionalValueChange<Maybe<string>>.With(
            expectedDataResponsible.Cvr != null
                ? Maybe<string>.Some(expectedDataResponsible.Cvr)
                : Maybe<string>.None
        ),
                    Address = OptionalValueChange<Maybe<string>>.With(
            expectedDataResponsible.Adress != null
                ? Maybe<string>.Some(expectedDataResponsible.Adress)
                : Maybe<string>.None
        ),
                    Phone = OptionalValueChange<Maybe<string>>.With(
            expectedDataResponsible.Phone != null
                ? Maybe<string>.Some(expectedDataResponsible.Phone)
                : Maybe<string>.None
        )
                },
                DataProtectionAdvisor = new DataProtectionAdvisorUpdateParameters()
                {
                    Email = OptionalValueChange<Maybe<string>>.With(
            expectedDataProtectionAdvisor.Email != null
                ? Maybe<string>.Some(expectedDataProtectionAdvisor.Email)
                : Maybe<string>.None
        ),
                    Name = OptionalValueChange<Maybe<string>>.With(
            expectedDataProtectionAdvisor.Name != null
                ? Maybe<string>.Some(expectedDataProtectionAdvisor.Name)
                : Maybe<string>.None
        ),
                    Cvr = OptionalValueChange<Maybe<string>>.With(
            expectedDataProtectionAdvisor.Cvr != null
                ? Maybe<string>.Some(expectedDataProtectionAdvisor.Cvr)
                : Maybe<string>.None
        ),
                    Address = OptionalValueChange<Maybe<string>>.With(
            expectedDataProtectionAdvisor.Adress != null
                ? Maybe<string>.Some(expectedDataProtectionAdvisor.Adress)
                : Maybe<string>.None
        ),
                    Phone = OptionalValueChange<Maybe<string>>.With(
            expectedDataProtectionAdvisor.Phone != null
                ? Maybe<string>.Some(expectedDataProtectionAdvisor.Phone)
                : Maybe<string>.None
        )
                }
            };
        }
        
    }
}
