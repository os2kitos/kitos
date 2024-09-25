using System;
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

        [Fact]
        public void Can_Map_Master_Data_Roles()
        {
            var orgUuid = A<Guid>();
            var dto = new OrganizationMasterDataRolesRequestDTO()
            {
                ContactPerson = new ContactPersonRequestDTO()
                {
                    Email = A<string>(), LastName = A<string>(), PhoneNumber = A<string>(), Name = A<string>()
                },
                DataResponsible = new DataResponsibleRequestDTO()
                {
                    Address = A<string>(), Cvr = A<string>(), Email = A<string>(), Name = A<string>(),
                    Phone = A<string>()
                },
                DataProtectionAdvisor = new DataProtectionAdvisorRequestDTO()
                {
                    Address = A<string>(),
                    Cvr = A<string>(),
                    Email = A<string>(),
                    Name = A<string>(),
                    Phone = A<string>()
                }
            };

            var result = _sut.ToMasterDataRolesUpdateParameters(orgUuid, dto);

            var cpDto = dto.ContactPerson;
            var cpResult = result.ContactPerson;
            AssertParamHasValidChange(cpResult.Value.Email, cpDto.Email);
            AssertParamHasValidChange(cpResult.Value.LastName, cpDto.LastName);
            AssertParamHasValidChange(cpResult.Value.Name, cpDto.Name);
            AssertParamHasValidChange(cpResult.Value.PhoneNumber, cpDto.PhoneNumber);

            var drDto = dto.DataResponsible;
            var drResult = result.DataResponsible;
            AssertParamHasValidChange(drResult.Value.Email, drDto.Email);
            AssertParamHasValidChange(drResult.Value.Cvr, drDto.Cvr);
            AssertParamHasValidChange(drResult.Value.Name, drDto.Name);
            AssertParamHasValidChange(drResult.Value.Phone, drDto.Phone);
            AssertParamHasValidChange(drResult.Value.Address, drDto.Address);

            var dpaDto = dto.DataProtectionAdvisor;
            var dpaResult = result.DataProtectionAdvisor;
            AssertParamHasValidChange(dpaResult.Value.Email, dpaDto.Email);
            AssertParamHasValidChange(dpaResult.Value.Cvr, dpaDto.Cvr);
            AssertParamHasValidChange(dpaResult.Value.Name, dpaDto.Name);
            AssertParamHasValidChange(dpaResult.Value.Phone, dpaDto.Phone);
            AssertParamHasValidChange(dpaResult.Value.Address, dpaDto.Address);
        }

        private void AssertParamHasValidChange(OptionalValueChange<Maybe<string>> parameter, string expected)
        {
            Assert.True(parameter.HasChange && parameter.NewValue.HasValue);
            Assert.Equal(expected, parameter.NewValue.Value);
        }
    }
}
