using Moq;
using PubSub.Core.Config;
using PubSub.Core.Services.CallbackAuthenticator;
using PubSub.Test.Base.Tests.Toolkit.Patterns;
using System.Security.Cryptography;
using System.Text;

namespace PubSub.Test.Unit.Core
{
    public class HmacCallbackAuthenticatorTest: WithAutoFixture
    {
        [Fact]
        public void Can_Hash_Based_On_Source()
        {
            var source = A<string>();
            var configKey = A<string>();
            var apiKey = A<string>();
            using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey));
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(source));
            var expected = Convert.ToBase64String(hash);
            var configuration = new Mock<ICallbackAuthenticatorConfig>();
            configuration.Setup(_ => _.ApiKey).Returns(apiKey);
            var sut = new HmacCallbackAuthenticator(configuration.Object);

            var actual = sut.GetAuthentication(source);

            Assert.Equal(expected, actual);

        }
    }
}
