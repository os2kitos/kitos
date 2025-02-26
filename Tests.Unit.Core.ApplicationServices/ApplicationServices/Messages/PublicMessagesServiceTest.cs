using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Messages;
using Core.ApplicationServices.Model.Messages;
using Core.DomainModel;
using Core.DomainModel.PublicMessage;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Moq;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.Messages
{
    public class PublicMessagesServiceTest : WithAutoFixture
    {
        private readonly Mock<IGenericRepository<PublicMessage>> _repositoryMock;
        private readonly Mock<IOrganizationalUserContext> _userContextMock;
        private readonly PublicMessagesService _sut;

        public PublicMessagesServiceTest()
        {
            _repositoryMock = new Mock<IGenericRepository<PublicMessage>>();
            _userContextMock = new Mock<IOrganizationalUserContext>();
            _sut = new PublicMessagesService(_repositoryMock.Object, _userContextMock.Object);
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
            var testMessages = ExpectGetMessages().ToList();

            //Act
            var publicMessagesResult = _sut.Read().ToList();

            //Assert
            foreach (var testMessage in testMessages)
            {
                var resultMessage = Assert.Single(publicMessagesResult, x => x.Uuid == testMessage.Uuid);
                Assert.Equal(testMessage.Title, resultMessage.Title);
                Assert.Equal(testMessage.LongDescription, resultMessage.LongDescription);
                Assert.Equal(testMessage.ShortDescription, resultMessage.ShortDescription);
                Assert.Equal(testMessage.Link, resultMessage.Link);
                Assert.Equal(testMessage.Status, resultMessage.Status);
            }
        }

        [Fact]
        public void Can_Create()
        {
            //Assert
            ExpectIsGlobalAdminReturns(true);
            var expectedChanges = new WritePublicMessagesParams
            {
                LongDescription = A<string>().AsChangedValue(),
                ShortDescription = A<string>().AsChangedValue(),
                Link = A<string>().AsChangedValue(),
                Status = A<PublicMessageStatus?>().AsChangedValue()
            };

            //Act
            var result = _sut.Create(expectedChanges);

            //Assert
            AssertMessageChanges(result, expectedChanges);
            _repositoryMock.Verify(x => x.Save());
        }

        [Fact]
        public void Can_Write()
        {
            //Assert
            ExpectGetMessages();
            ExpectIsGlobalAdminReturns(true);
            var messageUuid = A<Guid>();
            var expectedChanges = new WritePublicMessagesParams
            {
                LongDescription = A<string>().AsChangedValue(),
                ShortDescription = A<string>().AsChangedValue(),
                Link = A<string>().AsChangedValue(),
                Status = A<PublicMessageStatus?>().AsChangedValue()
            };

            var message = A<PublicMessage>();
            message.Uuid = messageUuid;

            ExpectGetMessageByUuid(message);

            //Act
            var result = _sut.Patch(messageUuid, expectedChanges);

            //Assert
            AssertMessageChanges(result, expectedChanges);
            _repositoryMock.Verify(x => x.Save());
        }

        [Fact]
        public void Can_Write_Partially()
        {
            //Assert
            ExpectIsGlobalAdminReturns(true);
            var messageUuid = A<Guid>();
            var parameters = new WritePublicMessagesParams()
            {
                LongDescription = A<string>().AsChangedValue(),
                ShortDescription = A<string>().AsChangedValue(),
            };

            var message = A<PublicMessage>();
            message.Uuid = messageUuid;

            ExpectGetMessageByUuid(message);

            //Act
            var result = _sut.Patch(messageUuid, parameters);

            //Assert
            AssertMessageChanges(result, parameters);
            _repositoryMock.Verify(x => x.Save());
        }

        [Fact]
        public void Cannot_Write_If_Not_Global_Admin()
        {
            //Arrange
            ExpectIsGlobalAdminReturns(false);

            //Act
            var result = _sut.Patch(A<Guid>(), new WritePublicMessagesParams());

            //Assert
            Assert.True(result.Failed);
            Assert.Equal(OperationFailure.Forbidden, result.Error.FailureType);
        }

        private void ExpectIsGlobalAdminReturns(bool value)
        {
            _userContextMock.Setup(x => x.IsGlobalAdmin()).Returns(value);
        }


        private void ExpectGetMessageByUuid(PublicMessage message)
        {
            var messageList = new List<PublicMessage>() { message };
            _repositoryMock.Setup(x => x.AsQueryable()).Returns(messageList.AsQueryable());
        }

        private IEnumerable<PublicMessage> ExpectGetMessages()
        {
            const int maxId = 6;
            var messages = Many<PublicMessage>(maxId).ToList();
            for (var id = 1; id <= maxId; id++)
            {
                messages[id - 1].Id = id;
            }

            _repositoryMock.Setup(x => x.AsQueryable()).Returns(messages.AsQueryable());
            return messages;
        }

        private static void AssertMessageChanges(Result<PublicMessage, OperationError> result, WritePublicMessagesParams expectedChanges)
        {
            Assert.True(result.Ok);
            var publicMessage = result.Value;
            if (expectedChanges.Title.HasChange)
            {
                Assert.Equal(expectedChanges.Title.NewValue, publicMessage.Title);
            }
            if (expectedChanges.Link.HasChange)
            {
                Assert.Equal(expectedChanges.Link.NewValue, publicMessage.Link);
            }
            if (expectedChanges.ShortDescription.HasChange)
            {
                Assert.Equal(expectedChanges.ShortDescription.NewValue, publicMessage.ShortDescription);
            }
            if (expectedChanges.LongDescription.HasChange)
            {
                Assert.Equal(expectedChanges.LongDescription.NewValue, publicMessage.LongDescription);
            }
            if (expectedChanges.Status.HasChange)
            {
                Assert.Equal(expectedChanges.Status.NewValue, publicMessage.Status);
            }
        }
    }
}
