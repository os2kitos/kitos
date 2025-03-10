using System.Collections.Generic;
using Core.ApplicationServices.SSO.Model;
using dk.nita.saml20.identity;
using dk.nita.saml20.Schema.Core;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SSO
{
    public class Saml20IdentityParserTest
    {
        [Fact]
        private void MatchPrivilege_WhenNoPrivilegeNodeFound_ReturnsValidResult()
        {
            var mockIdentity = new Saml20Identity(string.Empty, new List<SamlAttribute>());
            var sut = Saml20IdentityParser.CreateFrom(mockIdentity);
            var matchPrivilege = sut.MatchPrivilege("dummy");
            Assert.False(matchPrivilege.HasValue);
        }
    }
}
