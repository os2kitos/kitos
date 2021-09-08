using System.Collections.Generic;
using System.Linq;
using Moq;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Contract;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItContractWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly ItContractWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;

        public ItContractWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties())
                .Returns(GetRootProperties());
            _sut = new ItContractWriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_Post(string name)
        {
            //Arrange
            var requestDto = new ContractWriteRequestDTO() { Name = name };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_Put(string name)
        {
            //Arrange
            var requestDto = new ContractWriteRequestDTO() { Name = name };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromPUT_Ignores_Undefined_Root_Sections(bool noName)
        {
            //Arrange

            var rootProperties = GetRootProperties();
            if (noName) rootProperties.Remove(nameof(ContractWriteRequestDTO.Name));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties()).Returns(rootProperties);
            var emptyInput = new ContractWriteRequestDTO();

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert
            Assert.Equal(noName, output.Name.IsUnchanged);
        }

        private static HashSet<string> GetRootProperties()
        {
            return typeof(ContractWriteRequestDTO).GetProperties().Select(x => x.Name).ToHashSet();
        }
    }
}
