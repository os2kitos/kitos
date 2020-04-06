using Core.ApplicationServices.SSO;
using Core.ApplicationServices.SSO.Model;
using dk.nita.saml20.identity;
using Moq;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SSO
{
    public class Saml20IdentityNavigatorTest
    {
        [Fact]
        private void GetAttribute_WhenNoSamlPrivilege_ReturnsValidResult()
        {
            var mockIdentity = new Mock<ISaml20Identity>();
            var sut = new Saml20IdentityNavigator(mockIdentity.Object);
            var attribute = sut.GetAttribute(StsAdgangsStyringConstants.Attributes.PrivilegeKey);
            Assert.False(attribute.HasValue);
        }

        [Fact]
        private void GetPrivilegeNode_WhenNoNode_ReturnsValidResult()
        {
            var mockIdentity = new Mock<ISaml20Identity>();
            var sut = new Saml20IdentityNavigator(mockIdentity.Object);
            var privilegeNode = sut.GetPrivilegeNode();
            Assert.False(privilegeNode.HasValue);
        }
    }
}
