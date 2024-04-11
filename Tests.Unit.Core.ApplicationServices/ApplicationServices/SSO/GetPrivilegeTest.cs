using System.Collections.Generic;
using Core.ApplicationServices.SSO.Model;
using dk.nita.saml20.identity;
using dk.nita.saml20.Schema.Core;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.SSO
{
    public class GetPrivilegeTest: WithAutoFixture
    {
        [Fact]
        public void GetPrivilege_WhenNoPrivilegeNodeFound_ReturnsValidResult()
        {
            var mockIdentity = new Saml20Identity(string.Empty, new List<SamlAttribute>(), string.Empty);
            var sut = Saml20IdentityParser.CreateFrom(mockIdentity);
            var matchPrivilege = sut.MatchPrivilege("dummy");
            Assert.False(matchPrivilege.HasValue);
        }
    }
}
