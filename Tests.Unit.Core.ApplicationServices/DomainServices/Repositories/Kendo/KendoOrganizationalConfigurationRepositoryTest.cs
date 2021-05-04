using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public KendoOrganizationalConfigurationRepositoryTest()
        {
            _repository = new Mock<IGenericRepository<KendoOrganizationalConfiguration>>();
            _sut = new KendoOrganizationalConfigurationRepository(_repository.Object);
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

        private void VerifySaved()
        {
            _repository.Verify(x => x.Save(), Times.Once);
        }
    }
}
