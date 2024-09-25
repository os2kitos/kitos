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

namespace Tests.Unit.Presentation.Web.Services
{
    public class OrganizationWriteServiceTest: WithAutoFixture
    {
        private readonly Mock<IAuthorizationContext> _authorizationContext;
        private readonly Mock<ITransactionManager> _transactionManager;
        private readonly Mock<IDomainEvents> _domainEvents;
        private readonly Mock<IOrganizationRepository> _organizationrepository;
        private readonly Mock<IOrganizationService> _organizationService;
        private readonly OrganizationWriteService _sut;


        public OrganizationWriteServiceTest()
        {
            _authorizationContext = new Mock<IAuthorizationContext>();
            _transactionManager = new Mock<ITransactionManager>();
            _domainEvents = new Mock<IDomainEvents>();  
            _organizationrepository = new Mock<IOrganizationRepository>();
            _organizationService = new Mock<IOrganizationService>();
            _sut = new OrganizationWriteService(_transactionManager.Object,
                _domainEvents.Object,
                _organizationService.Object,
                _authorizationContext.Object,
                _organizationrepository.Object);
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
    }
}
