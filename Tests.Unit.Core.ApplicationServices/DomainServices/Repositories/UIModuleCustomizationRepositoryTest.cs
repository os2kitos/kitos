using System.Linq;
using System.Collections.Generic;
using Core.DomainModel.Events;
using Core.DomainModel.UIConfiguration;
using Core.DomainServices;
using Core.DomainServices.Repositories.UICustomization;
using Moq;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.DomainServices.Repositories
{
    public class UIModuleCustomizationRepositoryTest
    {
        [Fact]
        public void Get_Filters_By_Organization_And_Module()
        {
            var target = new UIModuleCustomization { OrganizationId = 1, Module = "ItSystemUsages" };
            var modules = new[] { target,
                new UIModuleCustomization { OrganizationId = 2, Module = target.Module },
                new UIModuleCustomization { OrganizationId = 1, Module = "ItContracts" } };
            var repository = new Mock<IGenericRepository<UIModuleCustomization>>();
            repository.Setup(x => x.GetWithReferencePreload(It.IsAny<System.Linq.Expressions.Expression<System.Func<UIModuleCustomization, ICollection<CustomizedUINode>>>>()))
                .Returns(modules.AsQueryable());
            var sut = new UIModuleCustomizationRepository(repository.Object,
                Mock.Of<IGenericRepository<CustomizedUINode>>(), Mock.Of<IDomainEvents>());

            Assert.Same(target, sut.GetByOrganizationAndModule(1, target.Module).Value);
            Assert.True(sut.GetByOrganizationAndModule(3, target.Module).IsNone);
        }

        [Fact]
        public void DeleteNodes_Defers_Save_Until_Module_Update()
        {
            var repository = new Mock<IGenericRepository<UIModuleCustomization>>();
            var nodesRepository = new Mock<IGenericRepository<CustomizedUINode>>();
            var sut = new UIModuleCustomizationRepository(repository.Object, nodesRepository.Object, Mock.Of<IDomainEvents>());
            var nodes = new[] { new CustomizedUINode() };

            sut.DeleteNodes(nodes);

            nodesRepository.Verify(x => x.RemoveRange(nodes), Times.Once);
            repository.Verify(x => x.Save(), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        public void Update_Inserts_New_Modules_And_Updates_Persisted_Modules(int id)
        {
            var repository = new Mock<IGenericRepository<UIModuleCustomization>>();
            var domainEvents = new Mock<IDomainEvents>();
            var sut = new UIModuleCustomizationRepository(repository.Object,
                Mock.Of<IGenericRepository<CustomizedUINode>>(), domainEvents.Object);
            var module = new UIModuleCustomization { Id = id, Module = "ItSystemUsages" };

            sut.Update(module);

            repository.Verify(x => x.Insert(module), id == 0 ? Times.Once() : Times.Never());
            repository.Verify(x => x.Update(module), id == 0 ? Times.Never() : Times.Once());
            repository.Verify(x => x.Save(), Times.Once());
            domainEvents.Verify(x => x.Raise(It.IsAny<EntityUpdatedEvent<UIModuleCustomization>>()), Times.Once());
        }
    }
}
