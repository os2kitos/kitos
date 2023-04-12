using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Interface;
using Moq;
using Newtonsoft.Json.Linq;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.Interface;
using Tests.Toolkit.Extensions;
using Xunit;

namespace Tests.Unit.Presentation.Web.Models.V2
{
    public class ItInterfaceWriteModelMapperTest : WriteModelMapperTestBase
    {
        private readonly ItInterfaceWriteModelMapper _sut;
        private readonly Mock<ICurrentHttpRequest> _currentHttpRequestMock;

        public ItInterfaceWriteModelMapperTest()
        {
            _currentHttpRequestMock = new Mock<ICurrentHttpRequest>();
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch()))
                .Returns(GetRootProperties());
            _currentHttpRequestMock.Setup(x => x.GetObject(It.IsAny<IEnumerable<string>>())).Returns(Maybe<JToken>.None);
            _sut = new ItInterfaceWriteModelMapper(_currentHttpRequestMock.Object);
        }

        private static HashSet<string> GetRootProperties()
        {
            return typeof(RightsHolderCreateItInterfaceRequestDTO)
                .GetProperties()
                .Concat(typeof(CreateItInterfaceRequestDTO).GetProperties())
                .Select(x => x.Name)
                .ToHashSet();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_RightsHolder_Post(string name)
        {
            //Arrange
            var requestDto = new RightsHolderCreateItInterfaceRequestDTO() { Name = name };

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.AdditionalValues.Name));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_RightsHolder_Put(string name)
        {
            //Arrange
            var requestDto = new RightsHolderWritableItInterfacePropertiesDTO() { Name = name };

            //Act
            var modificationParameters = _sut.FromPUT(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
        }
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_RightsHolder_Patch(string name)
        {
            //Arrange
            var requestDto = new RightsHolderPartialUpdateItInterfaceRequestDTO() { Name = name };

            //Act
            var modificationParameters = _sut.FromPATCH(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
        }

        public static IEnumerable<object[]> GetUndefinedRightsHolderSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(6);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedRightsHolderSectionsInput))]
        public void From_RightsHolder_PATCH_Ignores_Undefined_Root_Sections(
            bool noName,
            bool noInterfaceId,
            bool noExposedBySystem,
            bool noVersion,
            bool noDescription,
            bool noUrlReference)
        {
            //Arrange
            var rootProperties = GetRootProperties();
            if (noName) rootProperties.Remove(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.Name));
            if (noInterfaceId) rootProperties.Remove(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.InterfaceId));
            if (noExposedBySystem) rootProperties.Remove(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.ExposedBySystemUuid));
            if (noVersion) rootProperties.Remove(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.Version));
            if (noDescription) rootProperties.Remove(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.Description));
            if (noUrlReference) rootProperties.Remove(nameof(RightsHolderPartialUpdateItInterfaceRequestDTO.UrlReference));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(rootProperties);
            var emptyInput = new RightsHolderPartialUpdateItInterfaceRequestDTO();

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert
            Assert.Equal(noName, output.Name.IsUnchanged);
            Assert.Equal(noInterfaceId, output.InterfaceId.IsUnchanged);
            Assert.Equal(noExposedBySystem, output.ExposingSystemUuid.IsUnchanged);
            Assert.Equal(noVersion, output.Version.IsUnchanged);
            Assert.Equal(noDescription, output.Description.IsUnchanged);
            Assert.Equal(noUrlReference, output.UrlReference.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedRightsHolderSectionsInput))]
        public void From_RightsHolder_PUT_Enforces_Undefined_Root_Sections(
            bool noName,
            bool noInterfaceId,
            bool noExposedBySystem,
            bool noVersion,
            bool noDescription,
            bool noUrlReference)
        {
            //Arrange
            var rootProperties = GetRootProperties();
            if (noName) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.Name));
            if (noInterfaceId) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.InterfaceId));
            if (noExposedBySystem) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.ExposedBySystemUuid));
            if (noVersion) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.Version));
            if (noDescription) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.Description));
            if (noUrlReference) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.UrlReference));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(rootProperties);
            var emptyInput = new RightsHolderWritableItInterfacePropertiesDTO();

            //Act
            var output = _sut.FromPUT(emptyInput);

            //Assert
            Assert.False(output.Name.IsUnchanged);
            Assert.False(output.InterfaceId.IsUnchanged);
            Assert.False(output.ExposingSystemUuid.IsUnchanged);
            Assert.False(output.Version.IsUnchanged);
            Assert.False(output.Description.IsUnchanged);
            Assert.False(output.UrlReference.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedRightsHolderSectionsInput))]
        public void From_RightsHolder_POST_Enforces_Undefined_Root_Sections(
            bool noName,
            bool noInterfaceId,
            bool noExposedBySystem,
            bool noVersion,
            bool noDescription,
            bool noUrlReference)
        {
            //Arrange
            var rootProperties = GetRootProperties();
            if (noName) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.Name));
            if (noInterfaceId) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.InterfaceId));
            if (noExposedBySystem) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.ExposedBySystemUuid));
            if (noVersion) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.Version));
            if (noDescription) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.Description));
            if (noUrlReference) rootProperties.Remove(nameof(RightsHolderWritableItInterfacePropertiesDTO.UrlReference));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(rootProperties);
            var input = new RightsHolderCreateItInterfaceRequestDTO();

            //Act
            var output = _sut.FromPOST(input);

            //Assert
            Assert.Null(output.RightsHolderProvidedUuid);
            Assert.False(output.AdditionalValues.Name.IsUnchanged);
            Assert.False(output.AdditionalValues.InterfaceId.IsUnchanged);
            Assert.False(output.AdditionalValues.ExposingSystemUuid.IsUnchanged);
            Assert.False(output.AdditionalValues.Version.IsUnchanged);
            Assert.False(output.AdditionalValues.Description.IsUnchanged);
            Assert.False(output.AdditionalValues.UrlReference.IsUnchanged);
        }

        [Fact]
        public void Can_Map_RightsHolderGivenUuid_From_RightsHolder_POST()
        {
            //Arrange
            var input = new RightsHolderCreateItInterfaceRequestDTO
            {
                Uuid = A<Guid>()
            };

            //Act
            var output = _sut.FromPOST(input);

            //Assert
            Assert.Equal(input.Uuid, output.RightsHolderProvidedUuid);
        }

        [Fact]
        public void Can_Map_From_Post()
        {
            //Arrange
            var requestDto = A<CreateItInterfaceRequestDTO>();

            //Act
            var modificationParameters = _sut.FromPOST(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
            Assert.Equal(requestDto.ExposedBySystemUuid, AssertPropertyContainsDataChange(modificationParameters.ExposingSystemUuid));
            Assert.Equal(requestDto.Scope.FromChoice(), AssertPropertyContainsDataChange(modificationParameters.Scope));
            Assert.Equal(requestDto.Deactivated, AssertPropertyContainsDataChange(modificationParameters.Deactivated));
            Assert.Equal(requestDto.Description, AssertPropertyContainsDataChange(modificationParameters.Description));
            Assert.Equal(requestDto.Note, AssertPropertyContainsDataChange(modificationParameters.Note));
            Assert.Equal(requestDto.ItInterfaceTypeUuid, AssertPropertyContainsDataChange(modificationParameters.InterfaceTypeUuid));
            Assert.Equal(requestDto.InterfaceId, AssertPropertyContainsDataChange(modificationParameters.InterfaceId));
            Assert.Equal(requestDto.UrlReference, AssertPropertyContainsDataChange(modificationParameters.UrlReference));
            Assert.Equal(requestDto.Version, AssertPropertyContainsDataChange(modificationParameters.Version));
            Assert.Equivalent(requestDto.Data.Select(x => new ItInterfaceDataWriteModel(x.Description, x.DataTypeUuid)), AssertPropertyContainsDataChange(modificationParameters.Data));
        }

        [Fact]
        public void Can_Map_From_Patch()
        {
            //Arrange
            var requestDto = A<UpdateItInterfaceRequestDTO>();

            //Act
            var modificationParameters = _sut.FromPATCH(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
            Assert.Equal(requestDto.ExposedBySystemUuid, AssertPropertyContainsDataChange(modificationParameters.ExposingSystemUuid));
            Assert.Equal(requestDto.Scope.FromChoice(), AssertPropertyContainsDataChange(modificationParameters.Scope));
            Assert.Equal(requestDto.Deactivated, AssertPropertyContainsDataChange(modificationParameters.Deactivated));
            Assert.Equal(requestDto.Description, AssertPropertyContainsDataChange(modificationParameters.Description));
            Assert.Equal(requestDto.Note, AssertPropertyContainsDataChange(modificationParameters.Note));
            Assert.Equal(requestDto.ItInterfaceTypeUuid, AssertPropertyContainsDataChange(modificationParameters.InterfaceTypeUuid));
            Assert.Equal(requestDto.InterfaceId, AssertPropertyContainsDataChange(modificationParameters.InterfaceId));
            Assert.Equal(requestDto.UrlReference, AssertPropertyContainsDataChange(modificationParameters.UrlReference));
            Assert.Equal(requestDto.Version, AssertPropertyContainsDataChange(modificationParameters.Version));
            Assert.Equivalent(requestDto.Data.Select(x => new ItInterfaceDataWriteModel(x.Description, x.DataTypeUuid)), AssertPropertyContainsDataChange(modificationParameters.Data));
        }

        public static IEnumerable<object[]> GetUndefinedSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(11);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void From_PATCH_Ignores_Undefined_Root_Sections(
            bool noName,
            bool noInterfaceId,
            bool noExposedBySystem,
            bool noVersion,
            bool noDescription,
            bool noUrlReference,
            bool noNote,
            bool noInterfaceType,
            bool noData,
            bool noScope,
            bool noDeactivated)
        {
            //Arrange
            var rootProperties = GetRootProperties();
            if (noName) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Name));
            if (noInterfaceId) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.InterfaceId));
            if (noExposedBySystem) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.ExposedBySystemUuid));
            if (noVersion) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Version));
            if (noDescription) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Description));
            if (noUrlReference) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.UrlReference));
            if (noNote) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Note));
            if (noInterfaceType) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.ItInterfaceTypeUuid));
            if (noData) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Data));
            if (noScope) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Scope));
            if (noDeactivated) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Deactivated));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(rootProperties);
            var emptyInput = new UpdateItInterfaceRequestDTO();

            //Act
            var output = _sut.FromPATCH(emptyInput);

            //Assert
            Assert.Equal(noName, output.Name.IsUnchanged);
            Assert.Equal(noInterfaceId, output.InterfaceId.IsUnchanged);
            Assert.Equal(noExposedBySystem, output.ExposingSystemUuid.IsUnchanged);
            Assert.Equal(noVersion, output.Version.IsUnchanged);
            Assert.Equal(noDescription, output.Description.IsUnchanged);
            Assert.Equal(noUrlReference, output.UrlReference.IsUnchanged);
            Assert.Equal(noNote, output.Note.IsUnchanged);
            Assert.Equal(noInterfaceType, output.InterfaceTypeUuid.IsUnchanged);
            Assert.Equal(noData, output.Data.IsUnchanged);
            Assert.Equal(noScope, output.Scope.IsUnchanged);
            Assert.Equal(noDeactivated, output.Deactivated.IsUnchanged);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void From_POST_Enforces_Undefined_Root_Sections(
           bool noName,
           bool noInterfaceId,
           bool noExposedBySystem,
           bool noVersion,
           bool noDescription,
           bool noUrlReference,
           bool noNote,
           bool noInterfaceType,
           bool noData,
           bool noScope,
           bool noDeactivated)
        {
            //Arrange
            var rootProperties = GetRootProperties();
            if (noName) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Name));
            if (noInterfaceId) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.InterfaceId));
            if (noExposedBySystem) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.ExposedBySystemUuid));
            if (noVersion) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Version));
            if (noDescription) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Description));
            if (noUrlReference) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.UrlReference));
            if (noNote) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Note));
            if (noInterfaceType) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.ItInterfaceTypeUuid));
            if (noData) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Data));
            if (noScope) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Scope));
            if (noDeactivated) rootProperties.Remove(nameof(UpdateItInterfaceRequestDTO.Deactivated));
            _currentHttpRequestMock.Setup(x => x.GetDefinedJsonProperties(Enumerable.Empty<string>().AsParameterMatch())).Returns(rootProperties);
            var emptyInput = new CreateItInterfaceRequestDTO();

            //Act
            var output = _sut.FromPOST(emptyInput);

            //Assert
            Assert.True(output.Name.HasChange);
            Assert.True(output.InterfaceId.HasChange);
            Assert.True(output.ExposingSystemUuid.HasChange);
            Assert.True(output.Version.HasChange);
            Assert.True(output.Description.HasChange);
            Assert.True(output.UrlReference.HasChange);
            Assert.True(output.Note.HasChange);
            Assert.True(output.InterfaceTypeUuid.HasChange);
            Assert.True(output.Data.HasChange);
            Assert.True(output.Scope.HasChange);
            Assert.True(output.Deactivated.HasChange);
        }
    }
}
