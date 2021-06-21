using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Repositories.Interface;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Repositories
{
    public class ItInterfaceRepositoryTest : WithAutoFixture
    {

        private readonly InterfaceRepository _sut;
        private readonly Mock<IGenericRepository<ItInterface>> _repository;

        public ItInterfaceRepositoryTest()
        {
            _repository = new Mock<IGenericRepository<ItInterface>>();
            _sut = new InterfaceRepository(_repository.Object);
        }

        [Fact]
        public void Can_Add()
        {
            //Arrange
            var itInterface = CreateInterface();
            _repository.Setup(x => x.Insert(itInterface)).Returns<ItInterface>(x => x);

            //Act
            _sut.Add(itInterface);

            //Assert
            VerifySaved(Times.Once());
            _repository.Verify(x => x.Insert(itInterface), Times.Once);
        }

        [Fact]
        public void Cannot_Add_If_Null()
        {
            //Arrange
            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => _sut.Add(null));
            VerifySaved(Times.Never());
            _repository.Verify(x => x.Insert(It.IsAny<ItInterface>()), Times.Never);
        }

        [Fact]
        public void Can_Update()
        {
            //Arrange
            var itInterface = CreateInterface();

            //Act
            _sut.Update(itInterface);

            //Assert
            VerifySaved(Times.Once());
            _repository.Verify(x => x.Update(itInterface), Times.Once);
        }

        [Fact]
        public void Cannot_Update_If_Null()
        {
            //Arrange
            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => _sut.Update(null));
            VerifySaved(Times.Never());
            _repository.Verify(x => x.Update(It.IsAny<ItInterface>()), Times.Never);
        }

        [Fact]
        public void Can_Delete()
        {
            //Arrange
            var itInterface = CreateInterface();

            //Act
            _sut.Delete(itInterface);

            //Assert
            VerifySaved(Times.Once());
            _repository.Verify(x => x.DeleteWithReferencePreload(itInterface), Times.Once);
        }

        [Fact]
        public void Cannot_Delete_If_Null()
        {
            //Arrange
            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => _sut.Update(null));
            VerifySaved(Times.Never());
            _repository.Verify(x => x.DeleteByKeyWithReferencePreload(It.IsAny<ItInterface>()), Times.Never);
        }

        [Fact]
        public void Can_GetInterface_By_Id_Returns_Single_ItInterface()
        {
            //Arrange
            var id = A<int>();
            var itInterface1 = new ItInterface()
            {
                Id = id
            };
            var itInterface2 = CreateInterface();
            _repository.Setup(x => x.AsQueryable()).Returns(new List<ItInterface>() { itInterface1, itInterface2 }.AsQueryable());

            //Act
            var result = _sut.GetInterface(id);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(itInterface1, result.Value);
        }

        [Fact]
        public void GetInterface_By_Id_Returns_None_If_No_ItInterface_Found()
        {
            //Arrange
            var id = A<int>();
            var itInterface1 = CreateInterface();
            var itInterface2 = CreateInterface();
            _repository.Setup(x => x.AsQueryable()).Returns(new List<ItInterface>() { itInterface1, itInterface2 }.AsQueryable());

            //Act
            var result = _sut.GetInterface(id);

            //Assert
            Assert.True(result.IsNone);
        }

        [Fact]
        public void Can_GetInterface_By_Uuid_Returns_Single_ItInterface()
        {
            //Arrange
            var uuid = A<Guid>();
            var itInterface1 = new ItInterface()
            {
                Uuid = uuid
            };
            var itInterface2 = CreateInterface();
            _repository.Setup(x => x.AsQueryable()).Returns(new List<ItInterface>() { itInterface1, itInterface2 }.AsQueryable());

            //Act
            var result = _sut.GetInterface(uuid);

            //Assert
            Assert.True(result.HasValue);
            Assert.Equal(itInterface1, result.Value);
        }

        [Fact]
        public void GetInterface_By_Uuid_Returns_None_If_No_ItInterface_Found()
        {
            //Arrange
            var uuid = A<Guid>();
            var itInterface1 = CreateInterface();
            var itInterface2 = CreateInterface();
            _repository.Setup(x => x.AsQueryable()).Returns(new List<ItInterface>() { itInterface1, itInterface2 }.AsQueryable());

            //Act
            var result = _sut.GetInterface(uuid);

            //Assert
            Assert.True(result.IsNone);
        }

        [Fact]
        public void Can_GetInterfaces()
        {
            //Arrange
            var numberOfInterfaces = Math.Abs(A<int>() % 10);
            var itInterfaces = new List<ItInterface>();
            for (int i = 0; i < numberOfInterfaces; i++)
            {
                itInterfaces.Add(CreateInterface());
            }
            _repository.Setup(x => x.AsQueryable()).Returns(itInterfaces.AsQueryable());

            //Act
            var result = _sut.GetInterfaces();

            //Assert
            Assert.Equal(numberOfInterfaces, result.Count());
        }

        [Fact]
        public void Can_GetInterfacesWithExternalReferenceDefined()
        {
            //Arrange
            var itInterfacesWithoutExternalReference = CreateInterface();
            var itInterfaceWithExternalReference = CreateInterface();
            itInterfaceWithExternalReference.Url = A<string>();
            _repository.Setup(x => x.AsQueryable()).Returns(new List<ItInterface>() { itInterfacesWithoutExternalReference, itInterfaceWithExternalReference }.AsQueryable());

            //Act
            var result = _sut.GetInterfacesWithExternalReferenceDefined();

            //Assert
            var itInterfaceResult = Assert.Single(result);
            Assert.Same(itInterfaceWithExternalReference, itInterfaceResult);
        }

        private ItInterface CreateInterface()
        {
            return new ItInterface()
            {
                Id = A<int>(),
                Uuid = A<Guid>()
            };
        }

        private void VerifySaved(Times numberOfTimes)
        {
            _repository.Verify(x => x.Save(), numberOfTimes);
        }
    }
}
