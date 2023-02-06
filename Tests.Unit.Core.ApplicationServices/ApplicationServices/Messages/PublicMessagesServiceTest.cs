using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Messages;
using Core.ApplicationServices.Model.Messages;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;
using Core.DomainServices;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Messages
{
    public class PublicMessagesServiceTest : WithAutoFixture
    {
        private readonly Mock<IGenericRepository<Text>> _repositoryMock;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly PublicMessagesService _sut;

        public PublicMessagesServiceTest()
        {
            _repositoryMock = new Mock<IGenericRepository<Text>>();
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _sut = new PublicMessagesService(_repositoryMock.Object, _userContextMock.Object, Mock.Of<ILogger>());
        }

        protected override void OnFixtureCreated(Fixture fixture)
        {
            base.OnFixtureCreated(fixture);
            fixture.Inject(new User());//Prevents loops
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void Permissions_Returns_Correct_Permissions(bool isGlobalAdmin, bool expectModify)
        {
            //Arrange
            ExpectIsGlobalAdminReturns(isGlobalAdmin);

            //Act
            var result = _sut.GetPermissions();

            //Assert
            Assert.False(result.Delete);
            Assert.True(result.Read);
            Assert.Equal(expectModify, result.Modify);
        }

        [Fact]
        public void Can_Read()
        {
            //Assert
            var texts = ExpectGetMessages();

            //Act
            var publicMessages = _sut.Read();

            //Assert
            var textLookup = texts.ToDictionary(x => x.Id);
            Assert.Equal(textLookup[Text.SectionIds.ContactInfo].Value, publicMessages.ContactInfo);
            Assert.Equal(textLookup[Text.SectionIds.StatusMessages].Value, publicMessages.StatusMessages);
            Assert.Equal(textLookup[Text.SectionIds.Guides].Value, publicMessages.Guides);
            Assert.Equal(textLookup[Text.SectionIds.Misc].Value, publicMessages.Misc);
            Assert.Equal(textLookup[Text.SectionIds.About].Value, publicMessages.About);
        }


        [Fact]
        public void Can_Write()
        {
            //Assert
            ExpectGetMessages();
            ExpectIsGlobalAdminReturns(true);
            var expectedChanges = new WritePublicMessagesParams()
            {
                ContactInfo = A<ChangedValue<string>>(),
                About = A<ChangedValue<string>>(),
                StatusMessages = A<ChangedValue<string>>(),
                Misc = A<ChangedValue<string>>(),
                Guides = A<ChangedValue<string>>()
            };

            //Act
            var result = _sut.Write(expectedChanges);

            //Assert
            Assert.True(result.Ok);
            var publicMessages = result.Value;
            Assert.Equal(expectedChanges.About.NewValue, publicMessages.About);
            Assert.Equal(expectedChanges.ContactInfo.NewValue, publicMessages.ContactInfo);
            Assert.Equal(expectedChanges.Misc.NewValue, publicMessages.Misc);
            Assert.Equal(expectedChanges.StatusMessages.NewValue, publicMessages.StatusMessages);
            Assert.Equal(expectedChanges.Guides.NewValue, publicMessages.Guides);
            _repositoryMock.Verify(x => x.Save());
        }

        [Fact]
        public void Can_Write_Partially()
        {
            //Assert
            var texts = ExpectGetMessages().ToDictionary(x => x.Id);
            ExpectIsGlobalAdminReturns(true);
            var input = new WritePublicMessagesParams()
            {
                ContactInfo = A<ChangedValue<string>>(),
                About = A<ChangedValue<string>>(),
            };
            var expectedResult = new PublicMessages(
                input.About.NewValue,
                texts[Text.SectionIds.Guides].Value,
                texts[Text.SectionIds.StatusMessages].Value,
                texts[Text.SectionIds.Misc].Value,
                input.ContactInfo.NewValue
            );

            //Act
            var result = _sut.Write(input);

            //Assert
            Assert.True(result.Ok);
            var publicMessages = result.Value;
            Assert.Equivalent(expectedResult, publicMessages);
            _repositoryMock.Verify(x => x.Save());
        }

        [Fact]
        public void Cannot_Write_If_Not_Global_Admin()
        {
            //Arrange
            ExpectIsGlobalAdminReturns(false);

            //Act
            var result = _sut.Write(new WritePublicMessagesParams());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        private void ExpectIsGlobalAdminReturns(bool value)
        {
            _userContextMock.Setup(x => x.IsGlobalAdmin()).Returns(value);
        }


        private IEnumerable<Text> ExpectGetMessages()
        {
            const int maxId = 5;
            var texts = Many<Text>(maxId).ToList();
            for (var id = 1; id <= maxId; id++)
            {
                texts[id - 1].Id = id;
            }

            _repositoryMock.Setup(x => x.AsQueryable()).Returns(texts.AsQueryable());
            return texts;
        }
    }
}
