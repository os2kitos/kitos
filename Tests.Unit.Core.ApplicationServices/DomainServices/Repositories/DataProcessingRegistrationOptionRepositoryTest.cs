using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.GDPR;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.DomainServices.Repositories
{
    public class DataProcessingRegistrationOptionRepositoryTest : WithAutoFixture
    {

        private readonly DataProcessingRegistrationOptionRepository _sut;
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingCountryOption>> _countryOptionsServiceMock;
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption>> _dataResponsibleOptionsServiceMock;
        private readonly Mock<IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption>> _basisForTransferOptionsServiceMock;

        public DataProcessingRegistrationOptionRepositoryTest()
        {
            _countryOptionsServiceMock = new Mock<IOptionsService<DataProcessingRegistration, DataProcessingCountryOption>>();
            _dataResponsibleOptionsServiceMock = new Mock<IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption>>();
            _basisForTransferOptionsServiceMock = new Mock<IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption>>();
            _sut = new DataProcessingRegistrationOptionRepository(
                _countryOptionsServiceMock.Object, 
                _dataResponsibleOptionsServiceMock.Object,
                _basisForTransferOptionsServiceMock.Object);
        }

        [Fact]
        public void Can_GetAvailableCountryOptions()
        {
            //Arrange
            var organizationId = A<int>(); 
            var countryOptions = new List<OptionDescriptor<DataProcessingCountryOption>>()
            {
                new OptionDescriptor<DataProcessingCountryOption>(new DataProcessingCountryOption(), ""),
            };
            ExpectCountryOptions(organizationId, countryOptions);

            //Act
            var assignableCountryOptions = _sut.GetAvailableCountryOptions(organizationId);

            //Assert
            Assert.Equal(countryOptions, assignableCountryOptions);
        }

        [Fact]
        public void Can_GetAvailableDataResponsibleOptions()
        {
            //Arrange
            var organizationId = A<int>();
            var dataResponsibleOptions = new List<OptionDescriptor<DataProcessingDataResponsibleOption>>()
            {
                new OptionDescriptor<DataProcessingDataResponsibleOption>(new DataProcessingDataResponsibleOption(), ""),
            };
            ExpectDataResponsibleOptions(organizationId, dataResponsibleOptions);

            //Act
            var assignableDataResponsibleOptions = _sut.GetAvailableDataResponsibleOptions(organizationId);

            //Assert
            Assert.Equal(dataResponsibleOptions, assignableDataResponsibleOptions);
        }

        [Fact]
        public void Can_GetAvailableBasisForTransferOptions()
        {
            //Arrange
            var organizationId = A<int>();
            var basisForTransferOptions = new List<OptionDescriptor<DataProcessingBasisForTransferOption>>()
            {
                new OptionDescriptor<DataProcessingBasisForTransferOption>(new DataProcessingBasisForTransferOption(), ""),
            };
            ExpectBasisForTransferOptions(organizationId, basisForTransferOptions);

            //Act
            var assignableBasisForTransferOptions = _sut.GetAvailableBasisForTransferOptions(organizationId);

            //Assert
            Assert.Equal(basisForTransferOptions, assignableBasisForTransferOptions);
        }

        private void ExpectDataResponsibleOptions(int organizationId, IEnumerable<OptionDescriptor<DataProcessingDataResponsibleOption>> dataResponsibleOptions)
        {
            _dataResponsibleOptionsServiceMock.Setup(x => x.GetAvailableOptionsDetails(organizationId)).Returns(dataResponsibleOptions);
        }

        private void ExpectCountryOptions(int organizationId, IEnumerable<OptionDescriptor<DataProcessingCountryOption>> countryOptions)
        {
            _countryOptionsServiceMock.Setup(x => x.GetAvailableOptionsDetails(organizationId)).Returns(countryOptions);
        }

        private void ExpectBasisForTransferOptions(int organizationId, IEnumerable<OptionDescriptor<DataProcessingBasisForTransferOption>> basisForTransferOptions)
        {
            _basisForTransferOptionsServiceMock.Setup(x => x.GetAvailableOptionsDetails(organizationId)).Returns(basisForTransferOptions);
        }
    }
}
