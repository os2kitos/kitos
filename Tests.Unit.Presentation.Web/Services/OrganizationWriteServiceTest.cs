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
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Organizations.Write;
using Tests.Toolkit.Patterns;
using Xunit;
using Core.DomainModel.Events;
using Core.DomainServices.Generic;
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.DomainModel;
using System.Collections.Generic;
using System.Linq;
using Core.DomainServices;

namespace Tests.Unit.Presentation.Web.Services
{
    public class OrganizationWriteServiceTest: WithAutoFixture
    {
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IOrganizationRepository> _organizationrepository;
        private readonly Mock<IOrganizationService> _organizationService;
        private readonly Mock<IEntityIdentityResolver> _identityResolver;
        private readonly Mock<IGenericRepository<ContactPerson>> _contactPersonRepository;
        private readonly Mock<IGenericRepository<DataResponsible>> _dataResponsibleRepository;
        private readonly Mock<IGenericRepository<DataProtectionAdvisor>> _dataProtectionAdvisorRepository;
        private readonly OrganizationWriteService _sut;


        public OrganizationWriteServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _transactionManager = new Mock<ITransactionManager>();
            _domainEvents = new Mock<IDomainEvents>();  
            _organizationrepository = new Mock<IOrganizationRepository>();
            _organizationService = new Mock<IOrganizationService>();
            _identityResolver = new Mock<IEntityIdentityResolver>();
            _contactPersonRepository = new Mock<IGenericRepository<ContactPerson>>();
            _dataResponsibleRepository = new Mock<IGenericRepository<DataResponsible>>();
            _dataProtectionAdvisorRepository = new Mock<IGenericRepository<DataProtectionAdvisor>>();
            _sut = new OrganizationWriteService(_transactionManager.Object,
                _domainEvents.Object,
                _organizationService.Object,
                _authorizationContext.Object,
                _organizationrepository.Object,
                _identityResolver.Object,
                _contactPersonRepository.Object,
                _dataResponsibleRepository.Object,
                _dataProtectionAdvisorRepository.Object);
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

            var result = _sut.UpdateMasterData(organizationUuid, updateParameters);

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

            var result = _sut.UpdateMasterData(invalidOrganizationUuid, updateParameters);
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

            var result = _sut.UpdateMasterData(organizationUuid, updateParameters);
            Assert.True(result.Ok);

            var updatedOrganization = result.Value;
            Assert.Equal(newCvr.NewValue.Value, updatedOrganization.Cvr);
            Assert.Equal(newPhone.NewValue.Value, updatedOrganization.Phone);
            Assert.Equal(newAddress.NewValue.Value, updatedOrganization.Adress);
            Assert.Equal(newEmail.NewValue.Value, updatedOrganization.Email);
            _organizationrepository.Verify(_ => _.Update(organization.Object));
        }

        [Fact]
        public void Can_Update_Master_Data_With_Null()
        {
            var organizationUuid = A<Guid>();
            var organization = new Mock<Organization>();
            _authorizationContext.Setup(x => x.AllowModify(It.IsAny<Organization>())).Returns(true);
            _authorizationContext.Setup(_ => _.AllowReads(It.IsAny<Organization>())).Returns(true);
            _organizationService.Setup(_ => _.GetOrganization(organizationUuid, null)).Returns(organization.Object);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);
            var updateParameters = new OrganizationMasterDataUpdateParameters()
            {
                Address = Maybe<string>.None.AsChangedValue(),
                Cvr = Maybe<string>.None.AsChangedValue(),
                Email = Maybe<string>.None.AsChangedValue(),
                Phone = Maybe<string>.None.AsChangedValue()
            };

            var result = _sut.UpdateMasterData(organizationUuid, updateParameters);
            Assert.True(result.Ok);

            var updatedOrganization = result.Value;
            Assert.Null(updatedOrganization.Cvr); ;
            Assert.Null(updatedOrganization.Phone);
            Assert.Null(updatedOrganization.Adress);
            Assert.Null(updatedOrganization.Email);
            _organizationrepository.Verify(_ => _.Update(organization.Object));
        }

        [Fact]
        public void Update_Master_Data_Roles_Returns_Bad_Input_If_Invalid_Uuid()
        {
            var invalidOrganizationUuid = A<Guid>();
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(invalidOrganizationUuid))
                .Returns(Maybe<int>.None);

            var result =
                _sut.UpsertOrganizationMasterDataRoles(invalidOrganizationUuid, new OrganizationMasterDataRolesUpdateParameters());

            Assert.True(result.Failed);
            var error = result.Error;
            Assert.Equal(OperationFailure.BadInput, error.FailureType);
        }

        [Fact]
        public void Update_Master_Data_Roles_Returns_Forbidden_If_Unauthorized_To_Modify_Data_Responsible()
        {
            var org = CreateOrganization();
            var orgId = org.Id;
            var updateParameters = SetupUpdateMasterDataRoles(orgId);
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(org.Uuid))
                .Returns(orgId);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<ContactPerson>()))
                .Returns(true);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataResponsible>()))
                .Returns(false);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataProtectionAdvisor>()))
                .Returns(true);
            var dataResponsible = new DataResponsible();
            _organizationService.Setup(_ => _.GetDataResponsible(orgId)).Returns(dataResponsible);
            var contactPerson = new ContactPerson();
            _organizationService.Setup(_ => _.GetContactPerson(orgId))
                .Returns(contactPerson);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var result =
                _sut.UpsertOrganizationMasterDataRoles(org.Uuid, updateParameters);

            Assert.True(result.Failed);
            var error = result.Error;
            Assert.Equal(OperationFailure.Forbidden, error.FailureType);
        }

        [Fact]
        public void Update_Master_Data_Roles_Returns_Forbidden_If_Unauthorized_To_Modify_Contact_Person()
        {
            var org = CreateOrganization();
            var orgId = org.Id;
            var updateParameters = SetupUpdateMasterDataRoles(orgId);
            var contactPerson = new ContactPerson();
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(org.Uuid))
                .Returns(orgId);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<ContactPerson>()))
                .Returns(false);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataResponsible>()))
                .Returns(true);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataProtectionAdvisor>()))
                .Returns(true);
            _organizationService.Setup(_ => _.GetContactPerson(orgId))
                .Returns(contactPerson);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var result =
                _sut.UpsertOrganizationMasterDataRoles(org.Uuid, updateParameters);

            Assert.True(result.Failed);
            var error = result.Error;
            Assert.Equal(OperationFailure.Forbidden, error.FailureType);
        }

        [Fact]
        public void Update_Master_Data_Roles_Returns_Forbidden_If_Unauthorized_To_Modify_Data_Protection_Advisor()
        {
            var org = CreateOrganization();
            var orgId = org.Id;
            var dataProtectionAdvisor = new DataProtectionAdvisor();
            var updateParameters = SetupUpdateMasterDataRoles(orgId);
            _identityResolver.Setup(_ =>
                    _.ResolveDbId<Organization>(org.Uuid))
                .Returns(orgId);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<ContactPerson>()))
                .Returns(true);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataResponsible>()))
                .Returns(true);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataProtectionAdvisor>()))
                .Returns(false);
            _organizationService.Setup(_ => _.GetDataProtectionAdvisor(orgId)).Returns(dataProtectionAdvisor);
            var dataResponsible = new DataResponsible();
            _organizationService.Setup(_ => _.GetDataResponsible(orgId)).Returns(dataResponsible);
            var contactPerson = new ContactPerson();
            _organizationService.Setup(_ => _.GetContactPerson(orgId))
                .Returns(contactPerson);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var result =
                _sut.UpsertOrganizationMasterDataRoles(org.Uuid, updateParameters);

            Assert.True(result.Failed);
            var error = result.Error;
            Assert.Equal(OperationFailure.Forbidden, error.FailureType);
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

            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<ContactPerson>()))
                .Returns(true);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataResponsible>()))
                .Returns(true);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataProtectionAdvisor>()))
                .Returns(true);

            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var result = _sut.UpsertOrganizationMasterDataRoles(org.Uuid, updateParameters);

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

            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<ContactPerson>()))
                .Returns(true);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataResponsible>()))
                .Returns(true);
            _authorizationContext.Setup(_ =>
                    _.AllowModify(It.IsAny<DataProtectionAdvisor>()))
                .Returns(true);

            _contactPersonRepository.Setup(_ =>
                    _.AsQueryable())
                .Returns(new List<ContactPerson>().AsQueryable());
            _dataResponsibleRepository.Setup(_ =>
                    _.AsQueryable())
                .Returns(new List<DataResponsible>().AsQueryable());
            _dataProtectionAdvisorRepository.Setup(_ =>
                    _.AsQueryable())
                .Returns(new List<DataProtectionAdvisor>().AsQueryable());
            _organizationService.Setup(_ => _.GetContactPerson(orgId)).Returns(Maybe<ContactPerson>.None);
            _organizationService.Setup(_ => _.GetDataResponsible(orgId)).Returns(Maybe<DataResponsible>.None);
            _organizationService.Setup(_ => _.GetDataProtectionAdvisor(orgId)).Returns(Maybe<DataProtectionAdvisor>.None);
            var transaction = new Mock<IDatabaseTransaction>();
            _transactionManager.Setup(x => x.Begin()).Returns(transaction.Object);

            var result = _sut.UpsertOrganizationMasterDataRoles(org.Uuid, updateParameters);

            Assert.True(result.Ok);
            _contactPersonRepository.Verify(_ => _.Insert(It.IsAny<ContactPerson>()));
            _dataResponsibleRepository.Verify(_ => _.Insert(It.IsAny<DataResponsible>()));
            _dataProtectionAdvisorRepository.Verify(_ => _.Insert(It.IsAny<DataProtectionAdvisor>()));
            var value = result.Value;
            AssertContactPerson(expectedContactPerson, value.ContactPerson);
            AssertDataResponsible(expectedDataResponsible, value.DataResponsible);
            AssertDataProtectionAdvisor(expectedDataProtectionAdvisor, value.DataProtectionAdvisor);
        }

        private OrganizationMasterDataRolesUpdateParameters SetupUpdateMasterDataRoles(int orgId,
            ContactPerson cp = null, DataResponsible dr = null, DataProtectionAdvisor dpa = null)
        {
            var expectedContactPerson = cp ?? SetupGetMasterDataRolesContactPerson(orgId);
            var expectedDataResponsible = dr ?? SetupGetMasterDataRolesDataResponsible(orgId);
            var expectedDataProtectionAdvisor = dpa ?? SetupGetMasterDataRolesDataProtectionAdvisor(orgId);
            //TODO RM repo calls here? then recycle for that method which won't have repo calls too
            _contactPersonRepository.Setup(_ =>
                    _.AsQueryable())
                .Returns(new List<ContactPerson> { expectedContactPerson }.AsQueryable());
            _dataResponsibleRepository.Setup(_ =>
                    _.AsQueryable())
                .Returns(new List<DataResponsible> { expectedDataResponsible }.AsQueryable());
            _dataProtectionAdvisorRepository.Setup(_ =>
                    _.AsQueryable())
                .Returns(new List<DataProtectionAdvisor> { expectedDataProtectionAdvisor }.AsQueryable());
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
        private Organization CreateOrganization()
        {
            var organizationId = A<Guid>();
            var organization = new Organization() { Uuid = organizationId, Id = A<int>() };
            return organization;
        }

        private DataProtectionAdvisor SetupGetMasterDataRolesDataProtectionAdvisor(int orgId)
        {
            var expectedDataProtectionAdvisor = new DataProtectionAdvisor()
            {
                Email = A<string>(),
                Name = A<string>(),
                Cvr = A<string>(),
                Adress = A<string>(),
                OrganizationId = orgId,
                Phone = A<string>(),
                Id = A<int>(),
            };
            _dataProtectionAdvisorRepository.Setup(_ =>
                    _.AsQueryable())
                .Returns(new List<DataProtectionAdvisor> { expectedDataProtectionAdvisor }.AsQueryable());
            return expectedDataProtectionAdvisor;
        }

        private ContactPerson SetupGetMasterDataRolesContactPerson(int orgId)
        {
            var expectedContactPerson = new ContactPerson
            {
                Email = A<string>(),
                Name = A<string>(),
                PhoneNumber = A<string>(),
                OrganizationId = orgId,
                Id = A<int>(),
            };
            _contactPersonRepository.Setup(_ =>
                    _.AsQueryable())
                .Returns(new List<ContactPerson> { expectedContactPerson }.AsQueryable());
            return expectedContactPerson;
        }

        private DataResponsible SetupGetMasterDataRolesDataResponsible(int orgId)
        {
            var expectedDataResponsible = new DataResponsible
            {
                Email = A<string>(),
                Name = A<string>(),
                Cvr = A<string>(),
                Adress = A<string>(),
                Phone = A<string>(),
                OrganizationId = orgId,
                Id = A<int>(),
            };
            _dataResponsibleRepository.Setup(_ =>
                    _.AsQueryable())
                .Returns(new List<DataResponsible> { expectedDataResponsible }.AsQueryable());
            return expectedDataResponsible;
        }

        private void AssertContactPerson(ContactPerson expected, ContactPerson actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.PhoneNumber, actual.PhoneNumber);
            Assert.Equal(expected.OrganizationId, actual.OrganizationId);
        }

        private void AssertDataResponsible(DataResponsible expected, DataResponsible actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Cvr, actual.Cvr);
            Assert.Equal(expected.OrganizationId, actual.OrganizationId);
            Assert.Equal(expected.Adress, actual.Adress);
        }

        private void AssertDataProtectionAdvisor(DataProtectionAdvisor expected, DataProtectionAdvisor actual)
        {
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Cvr, actual.Cvr);
            Assert.Equal(expected.OrganizationId, actual.OrganizationId);
            Assert.Equal(expected.Adress, actual.Adress);
        }
    }
}
