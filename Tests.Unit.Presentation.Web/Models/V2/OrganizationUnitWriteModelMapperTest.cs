using Moq;
using Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using System.Linq;
using Core.ApplicationServices.Model.Organizations.Write;
using Presentation.Web.Models.API.V2.Request.OrganizationUnit;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class OrganizationUnitWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly OrganizationUnitWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;

        public OrganizationUnitWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(GetAllInputPropertyNames<CreateOrganizationUnitRequestDTO>());
            _sut = new OrganizationUnitWriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Fact]
        public void Can_Map_General_Data_Properties()
        {
            //Arrange
            var input = A<CreateOrganizationUnitRequestDTO>();

            //Act
            var output = _sut.FromPOST(input);

            //Assert
            var mappedGeneralSection = AssertPropertyContainsDataChange<OrganizationUnitUpdateParameters>(output);
            Assert.Equal(input.Name, AssertPropertyContainsDataChange(output.Name));
            Assert.Equal(input.Origin.ToOrganizationUnitOrigin(), AssertPropertyContainsDataChange(output.Origin));
            Assert.Equal(((BaseOrganizationUnitRequestDTO)input).ParentUuid, AssertPropertyContainsDataChange(output.ParentUuid));
        }
    }
}
