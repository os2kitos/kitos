using System.Linq;
using System.Web.Http.Results;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Moq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class OrganizationWriteModelMapperTest: WriteModelMapperTestBase
    {
        private readonly OrganizationWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _httpRequest;

        public OrganizationWriteModelMapperTest()
        {
            _httpRequest = new Mock<ICurrentHttpRequest>();
            _httpRequest.Setup(x => 
                x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<OrganizationMasterDataRequestDTO>());
            _sut = new OrganizationWriteModelMapper(_httpRequest.Object);
        }

        [Fact]
        public void Can_Map_Master_Data_Update_Params()
        {
            var dto = new OrganizationMasterDataRequestDTO()
            {
                Address = A<string>(),
                Cvr = A<string>(),
                Email = A<string>(),
                Phone = A<string>()
            };

            var result = _sut.ToMasterDataUpdateParameters(dto);

            AssertParamHasValidChange(result.Email, dto.Email);
            AssertParamHasValidChange(result.Cvr, dto.Cvr);
            AssertParamHasValidChange(result.Address, dto.Address);
            AssertParamHasValidChange(result.Phone, dto.Phone);
        }

        private void AssertParamHasValidChange(OptionalValueChange<Maybe<string>> parameter, string expected)
        {
            Assert.True(parameter.HasChange && parameter.NewValue.HasValue);
            Assert.Equal(expected, parameter.NewValue.Value);
        }
    }
}
