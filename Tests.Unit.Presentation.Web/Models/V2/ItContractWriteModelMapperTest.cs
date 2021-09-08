using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Models.API.V2.Request.Contract;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItContractWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly ItContractWriteModelMapper _sut;

        public ItContractWriteModelMapperTest()
        {
            _sut = new ItContractWriteModelMapper();
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
    }
}
