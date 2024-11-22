using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
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
        private readonly Mock<IEntityIdentityResolver> _identityResolver;

        public OrganizationWriteModelMapperTest()
        {
            _httpRequest = new Mock<ICurrentHttpRequest>();
            _identityResolver = new Mock<IEntityIdentityResolver>();
            _httpRequest.Setup(x => 
                x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<OrganizationMasterDataRequestDTO>());
            _sut = new OrganizationWriteModelMapper(_httpRequest.Object, _identityResolver.Object);
        }

        [Fact]
        public void Can_Map_UI_Root_Config_Update_Params()
        {
            ExpectHttpRequestPropertyNames<UIRootConfigUpdateRequestDTO>();
            var dto = new UIRootConfigUpdateRequestDTO()
            {
                ShowItSystemModule = A<bool>(),
                ShowDataProcessing = A<bool>(),
                ShowItContractModule = A<bool>()
            };

            var result = _sut.ToUIRootConfigUpdateParameters(dto);

            Assert.Equal(dto.ShowItSystemModule, result.ShowItSystemModule.NewValue.Value);
            Assert.Equal(dto.ShowDataProcessing, result.ShowDataProcessing.NewValue.Value);
            Assert.Equal(dto.ShowItContractModule, result.ShowItContractModule.NewValue.Value);
        }

        [Fact]
        public void Can_Map_Organization_Update_Params()
        {
            ExpectHttpRequestPropertyNames<OrganizationUpdateRequestDTO>();
            var dto = A<OrganizationUpdateRequestDTO>();
            var result = _sut.ToOrganizationUpdateParameters(dto);
            AssertParamHasValidChange(result.Cvr, dto.Cvr);
            AssertParamHasValidChange(result.Name, dto.Name);
            Assert.Equal(result.TypeId.NewValue, (int)dto.Type);
            var foreignCountryCodeUuid = dto.ForeignCountryCodeUuid;
            Assert.NotNull(foreignCountryCodeUuid);
            Assert.Equal(foreignCountryCodeUuid, result.ForeignCountryCodeUuid.NewValue);
        }

        [Fact]
        public void Can_Map_Null_Country_Code_If_Requesting_Update()
        {
            var dto = new OrganizationUpdateRequestDTO()
            {
                UpdateForeignCountryCode = true,
                ForeignCountryCodeUuid = null,
            };

            var result = _sut.ToOrganizationUpdateParameters(dto);
            Assert.True(result.ForeignCountryCodeUuid.HasChange);
            Assert.Null(result.ForeignCountryCodeUuid.NewValue);
        }

        [Fact]
        public void Does_Not_Map_Null_Country_Code_If_Not_Requested()
        {

            //todo afte this check if the change in Org.cs is needed

            var dto = new OrganizationUpdateRequestDTO()
            {
                UpdateForeignCountryCode = false,
                ForeignCountryCodeUuid = null,
            };

            var result = _sut.ToOrganizationUpdateParameters(dto);
            Assert.False(result.ForeignCountryCodeUuid.HasChange);
        }

        [Fact]
        public void Can_Map_Organization_Create_Params()
        {
            ExpectHttpRequestPropertyNames<OrganizationCreateRequestDTO>();
            var dto = A<OrganizationCreateRequestDTO>();
            var result = _sut.ToOrganizationCreateParameters(dto);
            AssertParamHasValidChange(result.Cvr, dto.Cvr);
            AssertParamHasValidChange(result.Name, dto.Name);
            Assert.Equal(result.TypeId.NewValue, (int)dto.Type);
            var foreignCountryCodeUuid = dto.ForeignCountryCodeUuid;
            Assert.NotNull(foreignCountryCodeUuid);
            Assert.Equal(foreignCountryCodeUuid, result.ForeignCountryCodeUuid.NewValue);
        }

        [Fact]
        public void Can_Map_Master_Data_Update_Params()
        {
            ExpectHttpRequestPropertyNames<OrganizationMasterDataRequestDTO>();
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
        public void Can_Map_UI_Module_Customization_Params()
        {
            var orgUuid = A<Guid>();
            var expectedDbId = A<int>();
            var moduleName = A<string>();
            var dto = new UIModuleCustomizationRequestDTO()
            {
                Nodes = PrepareTestNodes(5, A<string>())
            };
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(expectedDbId);

            var result = _sut.ToUIModuleCustomizationParameters(orgUuid, moduleName, dto);

            Assert.True(result.Ok);
            var parameters = result.Value;
            Assert.Equal(moduleName, parameters.Module);
            Assert.Equal(expectedDbId, parameters.OrganizationId);
            Assert.Equal(dto.Nodes.Count(), parameters.Nodes.Count());
            foreach (var expectedNode in dto.Nodes)
            {
                var actual = parameters.Nodes.FirstOrDefault(nodeDto => nodeDto.Key == expectedNode.Key);

                Assert.NotNull(actual);
                Assert.Equal(expectedNode.Enabled, actual.Enabled);
            }
        }

        [Fact]
        public void Map_UI_Module_Customization_Params_Returns_Not_Found_If_Cannot_Resolve_Org_Id()
        {
            var orgUuid = A<Guid>();
            var moduleName = A<string>();
            var dto = new UIModuleCustomizationRequestDTO()
            {
                Nodes = PrepareTestNodes(5, A<string>())
            };
            _identityResolver.Setup(_ => _.ResolveDbId<Organization>(orgUuid)).Returns(Maybe<int>.None);

            var result = _sut.ToUIModuleCustomizationParameters(orgUuid, moduleName, dto);

            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.NotFound, result.Error.FailureType);
        }

        private List<CustomizedUINodeRequestDTO> PrepareTestNodes(int numberOfElements = 1, string key = "", bool isEnabled = false)
        {
            var nodes = new List<CustomizedUINodeRequestDTO>();
            for (var i = 0; i < numberOfElements; i++)
            {
                key = string.IsNullOrEmpty(key) ? GenerateKey() : key;
                nodes.Add(new CustomizedUINodeRequestDTO { Key = key, Enabled = isEnabled });
            }

            return nodes;
        }

        private string GenerateKey()
        {
            return Regex.Replace(A<string>(), "[0-9-]", "a");
        }

        [Fact]
        public void Can_Map_Master_Data_Roles()
        {
            ExpectHttpRequestPropertyNames<OrganizationMasterDataRolesRequestDTO>();
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

        private void AssertParamHasValidChange<T>(OptionalValueChange<Maybe<T>> parameter, T expected)
        {
            Assert.True(parameter.HasChange && parameter.NewValue.HasValue);
            Assert.Equal(expected, parameter.NewValue.Value);
        }

        private void ExpectHttpRequestPropertyNames<T>()
        {
            _httpRequest.Setup(x =>
                    x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<T>());
        }
    }
}
