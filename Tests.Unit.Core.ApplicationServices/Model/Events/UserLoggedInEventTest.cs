using Core.DomainModel.Events;
using System;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.Model.Events
{
    public class UserLoggedInEventTest: WithAutoFixture
    {

        [Fact]
        public void Should_Expose_Provided_UserUuid()
        {
            var expected = A<Guid>();

            var sut = new UserLoggedInEvent(expected);

            Assert.Equal(expected, sut.UserUuid);
        }
    }
}
