﻿using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.References;
using Core.DomainServices;
using Core.DomainServices.Repositories.Reference;

using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices
{
    public class ReferenceRepositoryTest : WithAutoFixture
    {
        private readonly ReferenceRepository _sut;
        private readonly Mock<IGenericRepository<ItContract>> _contractRepository;
        private readonly Mock<IGenericRepository<ItSystem>> _systemRepository;
        private readonly Mock<IGenericRepository<ItSystemUsage>> _systemUsageRepository;
        private readonly Mock<IGenericRepository<DataProcessingRegistration>> _dpaRepository;

        public ReferenceRepositoryTest()
        {
            _contractRepository = new Mock<IGenericRepository<ItContract>>();
            _systemRepository = new Mock<IGenericRepository<ItSystem>>();
            _systemUsageRepository = new Mock<IGenericRepository<ItSystemUsage>>();
            _dpaRepository = new Mock<IGenericRepository<DataProcessingRegistration>>();
            _sut = new ReferenceRepository
            (
                new Mock<IGenericRepository<ExternalReference>>().Object,
                _contractRepository.Object,
                _systemRepository.Object,
                _systemUsageRepository.Object,
                _dpaRepository.Object
            );
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetRootEntity_Returns_System(bool returnsSome)
        {
            //Arrange
            var id = A<int>();
            var expected = returnsSome ? new ItSystem() : null;
            _systemRepository.Setup(x => x.GetByKey(id)).Returns(expected);

            //Act
            var actual = _sut.GetRootEntity(id, ReferenceRootType.System);

            //Assert
            Assert.Equal(expected.FromNullable<IEntityWithExternalReferences>(), actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetRootEntity_Returns_SystemUsage(bool returnsSome)
        {
            //Arrange
            var id = A<int>();
            var expected = returnsSome ? new ItSystemUsage() : null;
            _systemUsageRepository.Setup(x => x.GetByKey(id)).Returns(expected);

            //Act
            var actual = _sut.GetRootEntity(id, ReferenceRootType.SystemUsage);

            //Assert
            Assert.Equal(expected.FromNullable<IEntityWithExternalReferences>(), actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetRootEntity_Returns_Contract(bool returnsSome)
        {
            //Arrange
            var id = A<int>();
            var expected = returnsSome ? new ItContract() : null;
            _contractRepository.Setup(x => x.GetByKey(id)).Returns(expected);

            //Act
            var actual = _sut.GetRootEntity(id, ReferenceRootType.Contract);

            //Assert
            Assert.Equal(expected.FromNullable<IEntityWithExternalReferences>(), actual);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetRootEntity_Returns_DataProcessingRegistration(bool returnsSome)
        {
            //Arrange
            var id = A<int>();
            var expected = returnsSome ? new DataProcessingRegistration() : null;
            _dpaRepository.Setup(x => x.GetByKey(id)).Returns(expected);

            //Act
            var actual = _sut.GetRootEntity(id, ReferenceRootType.DataProcessingRegistration);

            //Assert
            Assert.Equal(expected.FromNullable<IEntityWithExternalReferences>(), actual);
        }

        [Fact]
        public void SaveRootEntity_With_System()
        {
            //Act
            _sut.SaveRootEntity(new ItSystem());

            //Assert
            _systemRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveRootEntity_With_SystemUsage()
        {
            //Act
            _sut.SaveRootEntity(new ItSystemUsage());

            //Assert
            _systemUsageRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveRootEntity_With_Contract()
        {
            //Act
            _sut.SaveRootEntity(new ItContract());

            //Assert
            _contractRepository.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public void SaveRootEntity_With_DataProcessingRegistration()
        {
            //Act
            _sut.SaveRootEntity(new DataProcessingRegistration());

            //Assert
            _dpaRepository.Verify(x => x.Save(), Times.Once);
        }
    }
}
