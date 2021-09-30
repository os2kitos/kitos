using System.Collections.Generic;
using Core.ApplicationServices.Model.System;
using Moq;
using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.System;
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
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties())
                .Returns(GetAllInputPropertyNames<RightsHolderCreateItSystemRequestDTO>());
            _sut = new ItSystemWriteModelMapper(_currentHttpRequestMock.Object);
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

        public static IEnumerable<object[]> GetUndefinedSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(8);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPatch_Ignores_Root_Level_Sections_Not_Present_In_The_Request(
           bool noName,
           bool noDescription,
           bool noFormerName,
           bool noUrlReference,
           bool noParent,
           bool noBusinessType,
           bool noTaskRefKeys,
           bool noTaskRefUuids)
        {
            //Arrange
            var emptyInput = new RightsHolderPartialUpdateSystemPropertiesRequestDTO();
            var definedProperties = GetAllInputPropertyNames<RightsHolderPartialUpdateSystemPropertiesRequestDTO>();
            if (noName) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.Name));
            if (noDescription) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.Description));
            if (noFormerName) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.FormerName));
            if (noUrlReference) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.UrlReference));
            if (noParent) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.ParentUuid));
            if (noBusinessType) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.BusinessTypeUuid));
            if (noTaskRefKeys) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.KLENumbers));
            if (noTaskRefUuids) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.KLEUuids));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties()).Returns(definedProperties);

            //Act
            var output = _sut.FromRightsHolderPATCH(emptyInput);

            //Assert that all sections are mapped as changed - including undefined sections
            Assert.Equal(output.Name.IsUnchanged, noName);
            Assert.Equal(output.Description.IsUnchanged, noDescription);
            Assert.Equal(output.FormerName.IsUnchanged, noFormerName);
            Assert.Equal(output.UrlReference.IsUnchanged, noUrlReference);
            Assert.Equal(output.ParentSystemUuid.IsUnchanged, noParent);
            Assert.Equal(output.BusinessTypeUuid.IsUnchanged, noBusinessType);
            Assert.Equal(output.TaskRefKeys.IsUnchanged, noTaskRefKeys);
            Assert.Equal(output.TaskRefUuids.IsUnchanged, noTaskRefUuids);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPOST_Enforces_All_Properties(
           bool noName,
           bool noDescription,
           bool noFormerName,
           bool noUrlReference,
           bool npParent,
           bool noBusinessType,
           bool noTaskRefKeys,
           bool noTaskRefUuids)
        {
            //Arrange
            var emptyInput = new RightsHolderCreateItSystemRequestDTO();
            var definedProperties = GetAllInputPropertyNames<RightsHolderPartialUpdateSystemPropertiesRequestDTO>();
            if (noName) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.Name));
            if (noDescription) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.Description));
            if (noFormerName) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.FormerName));
            if (noUrlReference) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.UrlReference));
            if (npParent) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.ParentUuid));
            if (noBusinessType) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.BusinessTypeUuid));
            if (noTaskRefKeys) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.KLENumbers));
            if (noTaskRefUuids) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.KLEUuids));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties()).Returns(definedProperties);

            //Act
            var output = _sut.FromRightsHolderPOST(emptyInput);

            //Assert that all sections are mapped as changed - including undefined sections
            Assert.True(output.Name.HasChange);
            Assert.True(output.Description.HasChange);
            Assert.True(output.FormerName.HasChange);
            Assert.True(output.UrlReference.HasChange);
            Assert.True(output.ParentSystemUuid.HasChange);
            Assert.True(output.BusinessTypeUuid.HasChange);
            Assert.True(output.TaskRefKeys.HasChange);
            Assert.True(output.TaskRefUuids.HasChange);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPUT_Enforces_All_Properties(
       bool noName,
       bool noDescription,
       bool noFormerName,
       bool noUrlReference,
       bool npParent,
       bool noBusinessType,
       bool noTaskRefKeys,
       bool noTaskRefUuids)
        {
            //Arrange
            var emptyInput = new RightsHolderWritableITSystemPropertiesDTO();
            var definedProperties = GetAllInputPropertyNames<RightsHolderPartialUpdateSystemPropertiesRequestDTO>();
            if (noName) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.Name));
            if (noDescription) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.Description));
            if (noFormerName) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.FormerName));
            if (noUrlReference) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.UrlReference));
            if (npParent) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.ParentUuid));
            if (noBusinessType) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.BusinessTypeUuid));
            if (noTaskRefKeys) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.KLENumbers));
            if (noTaskRefUuids) definedProperties.Remove(nameof(RightsHolderPartialUpdateSystemPropertiesRequestDTO.KLEUuids));

            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties()).Returns(definedProperties);

            //Act
            var output = _sut.FromRightsHolderPUT(emptyInput);

            //Assert that all sections are mapped as changed - including undefined sections
            Assert.True(output.Name.HasChange);
            Assert.True(output.Description.HasChange);
            Assert.True(output.FormerName.HasChange);
            Assert.True(output.UrlReference.HasChange);
            Assert.True(output.ParentSystemUuid.HasChange);
            Assert.True(output.BusinessTypeUuid.HasChange);
            Assert.True(output.TaskRefKeys.HasChange);
            Assert.True(output.TaskRefUuids.HasChange);
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
