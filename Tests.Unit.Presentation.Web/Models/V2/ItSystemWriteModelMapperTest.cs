using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.System;
using Moq;
using Newtonsoft.Json.Linq;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.System.RightsHolder;
using Presentation.Web.Models.API.V2.Request.System.Shared;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItSystemWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;
        private readonly ItSystemWriteModelMapper _sut;

        public ItSystemWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x
                    .GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetAllInputPropertyNames<RightsHolderFullItSystemRequestDTO>()
                    .Concat(GetAllInputPropertyNames<UpdateItSystemRequestDTO>()).ToHashSet());
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(nameof(UpdateItSystemRequestDTO.RecommendedArchiveDuty).WrapAsEnumerable().AsParameterMatch())).Returns(GetAllInputPropertyNames<RecommendedArchiveDutyRequestDTO>());
            _currentHttpRequestMock.Setup(x => x.GetObject(It.IsAny<IEnumerable<string>>())).Returns(Maybe<JToken>.None);
            _sut = new ItSystemWriteModelMapper(_currentHttpRequestMock.Object);
        }

        [Fact]
        public void Can_Map_FromRightsHolderPOST()
        {
            //Arrange
            var input = A<RightsHolderFullItSystemRequestDTO>();

            //Act
            var output = _sut.FromRightsHolderPOST(input);

            //Assert
            Assert.Equal(input.ExternalUuid, AssertPropertyContainsDataChange(output.ExternalUuid));
            AssertUpdateData(input, output);
        }

        [Fact]
        public void Can_Map_FromRightsHolderPUT()
        {
            //Arrange
            var input = A<RightsHolderFullItSystemRequestDTO>();

            //Act
            var output = _sut.FromRightsHolderPUT(input);

            //Assert
            Assert.Equal(input.ExternalUuid, AssertPropertyContainsDataChange(output.ExternalUuid));
            AssertUpdateData(input, output);
        }

        [Fact]
        public void Can_Map_FromRightsHolderPATCH()
        {
            //Arrange
            var input = A<RightsHolderUpdateSystemPropertiesRequestDTO>();

            //Act
            var output = _sut.FromRightsHolderPATCH(input);

            //Assert
            Assert.Equal(input.ExternalUuid, AssertPropertyContainsDataChange(output.ExternalUuid));
            AssertUpdateData(input, output);
        }

        public static IEnumerable<object[]> GetUndefinedSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(7);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPatch_Ignores_Root_Level_Sections_Not_Present_In_The_Request(
           bool noName,
           bool noDescription,
           bool noFormerName,
           bool noExternalReferences,
           bool noParent,
           bool noBusinessType,
           bool noTaskRefUuids)
        {
            //Arrange
            var emptyInput = new RightsHolderUpdateSystemPropertiesRequestDTO();
            var definedProperties = GetAllInputPropertyNames<RightsHolderUpdateSystemPropertiesRequestDTO>();
            if (noName) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.Name));
            if (noDescription) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.Description));
            if (noFormerName) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.PreviousName));
            if (noExternalReferences) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.ExternalReferences));
            if (noParent) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.ParentUuid));
            if (noBusinessType) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.BusinessTypeUuid));
            if (noTaskRefUuids) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.KLEUuids));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(definedProperties);

            //Act
            var output = _sut.FromRightsHolderPATCH(emptyInput);

            //Assert that all sections are mapped as changed - including undefined sections
            Assert.Equal(output.Name.IsUnchanged, noName);
            Assert.Equal(output.Description.IsUnchanged, noDescription);
            Assert.Equal(output.FormerName.IsUnchanged, noFormerName);
            Assert.Equal(output.ExternalReferences.IsNone, noExternalReferences);
            Assert.Equal(output.ParentSystemUuid.IsUnchanged, noParent);
            Assert.Equal(output.BusinessTypeUuid.IsUnchanged, noBusinessType);
            Assert.Equal(output.TaskRefUuids.IsUnchanged, noTaskRefUuids);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPOST_Enforces_All_Properties(
           bool noName,
           bool noDescription,
           bool noFormerName,
           bool noExternalReferences,
           bool npParent,
           bool noBusinessType,
           bool noTaskRefUuids)
        {
            //Arrange
            var emptyInput = new RightsHolderFullItSystemRequestDTO();
            var definedProperties = GetAllInputPropertyNames<RightsHolderUpdateSystemPropertiesRequestDTO>();
            if (noName) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.Name));
            if (noDescription) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.Description));
            if (noFormerName) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.PreviousName));
            if (noExternalReferences) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.ExternalReferences));
            if (npParent) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.ParentUuid));
            if (noBusinessType) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.BusinessTypeUuid));
            if (noTaskRefUuids) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.KLEUuids));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(definedProperties);

            //Act
            var output = _sut.FromRightsHolderPOST(emptyInput);

            //Assert that all sections are mapped as changed - including undefined sections
            Assert.True(output.Name.HasChange);
            Assert.True(output.Description.HasChange);
            Assert.True(output.FormerName.HasChange);
            Assert.True(output.ExternalReferences.HasValue);
            Assert.True(output.ParentSystemUuid.HasChange);
            Assert.True(output.BusinessTypeUuid.HasChange);
            Assert.True(output.TaskRefUuids.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPUT_Enforces_All_Properties(
       bool noName,
       bool noDescription,
       bool noFormerName,
       bool noExternalReferences,
       bool npParent,
       bool noBusinessType,
       bool noTaskRefUuids)
        {
            //Arrange
            var emptyInput = new RightsHolderFullItSystemRequestDTO();
            var definedProperties = GetAllInputPropertyNames<RightsHolderUpdateSystemPropertiesRequestDTO>();
            if (noName) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.Name));
            if (noDescription) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.Description));
            if (noFormerName) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.PreviousName));
            if (noExternalReferences) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.ExternalReferences));
            if (npParent) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.ParentUuid));
            if (noBusinessType) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.BusinessTypeUuid));
            if (noTaskRefUuids) definedProperties.Remove(nameof(RightsHolderUpdateSystemPropertiesRequestDTO.KLEUuids));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(definedProperties);

            //Act
            var output = _sut.FromRightsHolderPUT(emptyInput);

            //Assert that all sections are mapped as changed - including undefined sections
            Assert.True(output.Name.HasChange);
            Assert.True(output.Description.HasChange);
            Assert.True(output.FormerName.HasChange);
            Assert.True(output.ExternalReferences.HasValue);
            Assert.True(output.ParentSystemUuid.HasChange);
            Assert.True(output.BusinessTypeUuid.HasChange);
            Assert.True(output.TaskRefUuids.HasChange);
        }

        [Fact]
        public void Can_Map_FromPOST()
        {
            //Arrange
            var input = A<CreateItSystemRequestDTO>();

            //Act
            var output = _sut.FromPOST(input);

            //Assert
            AssertUpdateData(input, output);
            AssertExtendedData(input, output);
        }

        [Fact]
        public void Can_Map_FromPATCH()
        {
            //Arrange
            var input = A<UpdateItSystemRequestDTO>();

            //Act
            var output = _sut.FromPATCH(input);

            //Assert
            AssertUpdateData(input, output);
            AssertExtendedData(input, output);
        }

        public static IEnumerable<object[]> GetUndefinedRegularSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(11);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedRegularSectionsInput))]
        public void FromPatch_Regular_Ignores_Root_Level_Sections_Not_Present_In_The_Request(
            bool noName,
            bool noDescription,
            bool noFormerName,
            bool noExternalReferences,
            bool noParent,
            bool noBusinessType,
            bool noTaskRefUuids, 
            bool noRecommendedArchiveDuty, 
            bool noRightsHolderUuid, 
            bool noScope,
            bool noDeactivated)
        {
            //Arrange
            var emptyInput = new UpdateItSystemRequestDTO();
            var definedProperties = GetAllInputPropertyNames<UpdateItSystemRequestDTO>();
            if (noName) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.Name));
            if (noDescription) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.Description));
            if (noFormerName) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.PreviousName));
            if (noExternalReferences) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.ExternalReferences));
            if (noParent) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.ParentUuid));
            if (noBusinessType) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.BusinessTypeUuid));
            if (noTaskRefUuids) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.KLEUuids));
            if (noRecommendedArchiveDuty) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.RecommendedArchiveDuty));
            if (noRightsHolderUuid) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.RightsHolderUuid));
            if (noScope) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.Scope));
            if (noDeactivated) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.Deactivated));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(definedProperties);

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert that all sections are mapped as changed - including undefined sections
            Assert.Equal(output.Name.IsUnchanged, noName);
            Assert.Equal(output.Description.IsUnchanged, noDescription);
            Assert.Equal(output.FormerName.IsUnchanged, noFormerName);
            Assert.Equal(output.ExternalReferences.IsNone, noExternalReferences);
            Assert.Equal(output.ParentSystemUuid.IsUnchanged, noParent);
            Assert.Equal(output.BusinessTypeUuid.IsUnchanged, noBusinessType);
            Assert.Equal(output.TaskRefUuids.IsUnchanged, noTaskRefUuids);
            Assert.Equal(output.ArchivingRecommendation.IsUnchanged, noRecommendedArchiveDuty);
            Assert.Equal(output.RightsHolderUuid.IsUnchanged, noRightsHolderUuid);
            Assert.Equal(output.Scope.IsUnchanged, noScope);
            Assert.Equal(output.Deactivated.IsUnchanged, noDeactivated);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedRegularSectionsInput))]
        public void FromPOST_Regular_Enforces_All_Properties(
            bool noName,
            bool noDescription,
            bool noFormerName,
            bool noExternalReferences,
            bool noParent,
            bool noBusinessType,
            bool noTaskRefUuids,
            bool noRecommendedArchiveDuty,
            bool noRightsHolderUuid,
            bool noScope,
            bool noDeactivated)
        {
            //Arrange
            var emptyInput = new CreateItSystemRequestDTO();
            var definedProperties = GetAllInputPropertyNames<UpdateItSystemRequestDTO>();
            if (noName) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.Name));
            if (noDescription) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.Description));
            if (noFormerName) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.PreviousName));
            if (noExternalReferences) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.ExternalReferences));
            if (noParent) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.ParentUuid));
            if (noBusinessType) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.BusinessTypeUuid));
            if (noTaskRefUuids) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.KLEUuids));
            if (noRecommendedArchiveDuty) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.RecommendedArchiveDuty));
            if (noRightsHolderUuid) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.RightsHolderUuid));
            if (noScope) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.Scope));
            if (noDeactivated) definedProperties.Remove(nameof(UpdateItSystemRequestDTO.Deactivated));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(definedProperties);

            //Act
            var output = _sut.FromPOST(emptyInput);

            //Assert that all sections are mapped as changed - including undefined sections
            Assert.True(output.Name.HasChange);
            Assert.True(output.Description.HasChange);
            Assert.True(output.FormerName.HasChange);
            Assert.True(output.ExternalReferences.HasValue);
            Assert.True(output.ParentSystemUuid.HasChange);
            Assert.True(output.BusinessTypeUuid.HasChange);
            Assert.True(output.TaskRefUuids.HasChange);
            Assert.True(output.Scope.HasChange);
            Assert.True(output.RightsHolderUuid.HasChange);
            Assert.True(output.ArchivingRecommendation.HasChange);
            Assert.True(output.Deactivated.HasChange);
        }

        private static void AssertExtendedData(IItSystemWriteRequestPropertiesDTO input, SystemUpdateParameters output)
        {
            Assert.Equal(input.Deactivated, AssertPropertyContainsDataChange(output.Deactivated));
            Assert.Equal(input.Scope?.FromChoice(), AssertPropertyContainsDataChange(output.Scope));
            Assert.Equal(input.RightsHolderUuid, AssertPropertyContainsDataChange(output.RightsHolderUuid));
            Assert.Equal(input.RecommendedArchiveDuty.Comment,
                AssertPropertyContainsDataChange(AssertPropertyContainsDataChange(output.ArchivingRecommendation).comment));
            Assert.Equal(input.RecommendedArchiveDuty?.Id?.FromChoice(),
                AssertPropertyContainsDataChange(
                    AssertPropertyContainsDataChange(output.ArchivingRecommendation).recommendation));
        }

        private static void AssertUpdateData(IItSystemWriteRequestCommonPropertiesDTO input, SharedSystemUpdateParameters output)
        {
            Assert.Equal(input.Name, AssertPropertyContainsDataChange(output.Name));
            Assert.Equal(input.PreviousName, AssertPropertyContainsDataChange(output.FormerName));
            Assert.Equal(input.Description, AssertPropertyContainsDataChange(output.Description));
            Assert.Equal(input.BusinessTypeUuid, AssertPropertyContainsDataChange(output.BusinessTypeUuid));
            Assert.Equal(input.KLEUuids, AssertPropertyContainsDataChange(output.TaskRefUuids));
            Assert.Equal(input.ParentUuid, AssertPropertyContainsDataChange(output.ParentSystemUuid));
            if (input is IHasExternalReferencesCreation creation)
            {
                Assert.Equivalent(creation.ExternalReferences, AssertPropertyContainsDataChange(output.ExternalReferences));
            }
            else if (input is IHasExternalReferencesUpdate update)
            {
                Assert.Equivalent(update.ExternalReferences, AssertPropertyContainsDataChange(output.ExternalReferences));
            }
        }
    }
}
