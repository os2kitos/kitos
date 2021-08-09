using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Repositories.Kendo;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Repositories.Kendo
{
    public class KendoOrganizationalConfigurationRepositoryTest : WithAutoFixture
    {
        private readonly KendoOrganizationalConfigurationRepository _sut;
        private readonly Mock<IGenericRepository<KendoOrganizationalConfiguration>> _repository;
        private readonly Mock<IGenericRepository<KendoColumnConfiguration>> _columnRepository;

        public KendoOrganizationalConfigurationRepositoryTest()
        {
            _repository = new Mock<IGenericRepository<KendoOrganizationalConfiguration>>();
            _columnRepository = new Mock<IGenericRepository<KendoColumnConfiguration>>();
            _sut = new KendoOrganizationalConfigurationRepository(_repository.Object, _columnRepository.Object);
        }

        [Fact]
        public void Can_Add()
        {
            //Arrange
            var config = new KendoOrganizationalConfiguration();
            _repository.Setup(x => x.Insert(config)).Returns<KendoOrganizationalConfiguration>(x => x);

            //Act
            var configuration = _sut.Add(config);

            //Assert
            Assert.Same(config, configuration);
            VerifySaved();
        }

        [Fact]
        public void Can_Update()
        {
            //Arrange
            var config = new KendoOrganizationalConfiguration();

            //Act
            _sut.Update(config);

            //Assert
            VerifySaved();
            _repository.Verify(x => x.Update(config), Times.Once);
        }

        [Fact]
        public void Can_DeleteChilds()
        {
            //Arrange
            var config = new KendoOrganizationalConfiguration();

            //Act
            _sut.DeleteColumns(config);

            //Assert
            _columnRepository.Verify(x => x.Save(), Times.Once);
            _columnRepository.Verify(x => x.RemoveRange(config.VisibleColumns), Times.Once);
        }

        private void VerifySaved()
        {
            _repository.Verify(x => x.Save(), Times.Once);
        }
    }
}
