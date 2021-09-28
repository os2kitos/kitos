using Core.ApplicationServices.Model.System;
using Moq;
using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.System;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItSystemV2WriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;
        private readonly ItSystemV2WriteModelMapper _sut;

        public ItSystemV2WriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonRootProperties())
                .Returns(GetAllInputPropertyNames<RightsHolderCreateItSystemRequestDTO>());
            _sut = new ItSystemV2WriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Fact]
        public void Can_Map_FromRightsHolderPOST()
        {
            //Arrange
            var input = A<RightsHolderCreateItSystemRequestDTO>();

            //Act
            var output = _sut.FromRightsHolderPOST(input);

            //Assert
            Assert.Equal(input.Uuid,output.RightsHolderProvidedUuid);
            AssertUpdateData(input, output);
        }

        [Fact]
        public void Can_Map_FromRightsHolderPUT()
        {
            //Arrange
            var input = A<RightsHolderWritableITSystemPropertiesDTO>();

            //Act
            var output = _sut.FromRightsHolderPUT(input);

            //Assert
            AssertUpdateData(input, output);
        }

        [Fact]
        public void Can_Map_FromRightsHolderPATCH()
        {
            //Arrange
            var input = A<RightsHolderPartialUpdateSystemPropertiesRequestDTO>();

            //Act
            var output = _sut.FromRightsHolderPATCH(input);

            //Assert
            AssertUpdateData(input, output);
        }


        private static void AssertUpdateData(IRightsHolderWritableSystemPropertiesRequestDTO input, RightsHolderSystemUpdateParameters output)
        {
            Assert.Equal(input.Name, AssertPropertyContainsDataChange(output.Name));
            Assert.Equal(input.FormerName, AssertPropertyContainsDataChange(output.FormerName));
            Assert.Equal(input.Description, AssertPropertyContainsDataChange(output.Description));
            Assert.Equal(input.UrlReference, AssertPropertyContainsDataChange(output.UrlReference));
            Assert.Equal(input.BusinessTypeUuid, AssertPropertyContainsDataChange(output.BusinessTypeUuid));
            Assert.Equal(input.KLENumbers, AssertPropertyContainsDataChange(output.TaskRefKeys));
            Assert.Equal(input.KLEUuids, AssertPropertyContainsDataChange(output.TaskRefUuids));
            Assert.Equal(input.ParentUuid, AssertPropertyContainsDataChange(output.ParentSystemUuid));
        }
    }
}
