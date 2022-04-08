using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Moq;
using Newtonsoft.Json.Linq;
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
            return typeof(RightsHolderCreateItInterfaceRequestDTO).GetProperties().Select(x => x.Name).ToHashSet();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("test")]
        public void Can_Map_Name_From_Post(string name)
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
        public void Can_Map_Name_From_Put(string name)
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
        public void Can_Map_Name_From_Patch(string name)
        {
            //Arrange
            var requestDto = new RightsHolderPartialUpdateItInterfaceRequestDTO() { Name = name };

            //Act
            var modificationParameters = _sut.FromPATCH(requestDto);

            //Assert
            Assert.Equal(requestDto.Name, AssertPropertyContainsDataChange(modificationParameters.Name));
        }

        public static IEnumerable<object[]> GetUndefinedSectionsInput()
        {
            return CreateGetUndefinedSectionsInput(6);
        }

        [Theory]
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPATCH_Ignores_Undefined_Root_Sections(
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
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPUT_Enforces_Undefined_Root_Sections(
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
        [MemberData(nameof(GetUndefinedSectionsInput))]
        public void FromPOST_Enforces_Undefined_Root_Sections(
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
        public void Can_Map_RightsHolderGivenUuid_From_POST()
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
    }
}
