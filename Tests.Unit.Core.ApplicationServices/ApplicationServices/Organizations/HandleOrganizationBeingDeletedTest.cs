using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Contract.Write;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Organizations.Handlers;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.Organization;
using Core.DomainServices.Context;
using Core.DomainServices.Repositories.Contract;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Organizations
{
    public class HandleOrganizationBeingDeletedTest : WithAutoFixture
    {
        private readonly Mock<IItContractSupplierOverviewReadModelRepository> _repository;
        private readonly HandleOrganizationBeingDeleted _sut;
        private readonly Organization _organization;

        public HandleOrganizationBeingDeletedTest()
        {
            _organization = new Organization { Id = A<int>(), Uuid = A<Guid>(), DataProcessingRegistrations = [] };
            _repository = new Mock<IItContractSupplierOverviewReadModelRepository>();
            var organizationService = new Mock<IOrganizationService>();
            organizationService.Setup(service => service.ComputeOrganizationRemovalConflicts(_organization.Uuid))
                .Returns(new OrganizationRemovalConflicts([], [], [], [], [], [], [], [], []));
            var defaultOrganizationResolver = new Mock<IDefaultOrganizationResolver>();
            defaultOrganizationResolver.Setup(resolver => resolver.Resolve()).Returns(new Organization());

            _sut = new HandleOrganizationBeingDeleted(
                Mock.Of<IItContractWriteService>(),
                Mock.Of<IItSystemUsageWriteService>(),
                Mock.Of<IItSystemService>(),
                Mock.Of<IDataProcessingRegistrationApplicationService>(),
                Mock.Of<IItInterfaceService>(),
                organizationService.Object,
                defaultOrganizationResolver.Object,
                Mock.Of<IDomainEvents>(),
                _repository.Object);
        }

        [Fact]
        public void Handle_Organization_Has_Supplier_Read_Models_Deletes_Only_Owned_Read_Models()
        {
            var ownedReadModels = new[]
            {
                new ItContractSupplierOverviewReadModel { OrganizationId = _organization.Id },
                new ItContractSupplierOverviewReadModel { OrganizationId = _organization.Id }
            };
            var otherOrganizationReadModel = new ItContractSupplierOverviewReadModel { OrganizationId = A<int>() };
            Assert.NotEqual(_organization.Id, otherOrganizationReadModel.OrganizationId);
            var readModels = ownedReadModels.Append(otherOrganizationReadModel).ToList();
            SetupReadModels(readModels);
            _repository.Setup(repository => repository.Delete(It.IsAny<ItContractSupplierOverviewReadModel>()))
                .Callback<ItContractSupplierOverviewReadModel>(model => readModels.Remove(model));

            _sut.Handle(new EntityBeingDeletedEvent<Organization>(_organization));

            Assert.Same(otherOrganizationReadModel, Assert.Single(readModels));
            foreach (var readModel in ownedReadModels)
            {
                _repository.Verify(repository => repository.Delete(readModel), Times.Once);
            }
            _repository.Verify(repository => repository.GetByOrganizationId(_organization.Id), Times.Once);
            _repository.Verify(repository => repository.Delete(otherOrganizationReadModel), Times.Never);
        }

        [Fact]
        public void Handle_Organization_Has_No_Supplier_Read_Models_Does_Not_Delete_Read_Models()
        {
            SetupReadModels([]);

            _sut.Handle(new EntityBeingDeletedEvent<Organization>(_organization));

            _repository.Verify(repository => repository.Delete(It.IsAny<ItContractSupplierOverviewReadModel>()), Times.Never);
        }

        [Fact]
        public void Handle_Supplier_Read_Model_Deletion_Fails_Propagates_Exception()
        {
            var readModel = new ItContractSupplierOverviewReadModel { OrganizationId = _organization.Id };
            var exception = new InvalidOperationException(A<string>());
            SetupReadModels([readModel]);
            _repository.Setup(repository => repository.Delete(readModel)).Throws(exception);

            var actual = Assert.Throws<InvalidOperationException>(() => _sut.Handle(new EntityBeingDeletedEvent<Organization>(_organization)));

            Assert.Same(exception, actual);
        }

        private void SetupReadModels(List<ItContractSupplierOverviewReadModel> readModels)
        {
            _repository.Setup(repository => repository.GetByOrganizationId(It.IsAny<int>()))
                .Returns((int organizationId) => readModels.Where(model => model.OrganizationId == organizationId).AsQueryable());
        }
    }
}
