using System;
using System.Linq;
using dk.nita.saml20.identity;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.SSO.Model
{
    public class Saml20IdentityParser
    {
        private Saml20IdentityParser(Saml20IdentityNavigator navigator)
        {
            Navigator = navigator;
        }

        public Saml20IdentityNavigator Navigator { get; }

        public static Saml20IdentityParser CreateFrom(ISaml20Identity sourceIdentity)
        {
            return new Saml20IdentityParser(new Saml20IdentityNavigator(sourceIdentity));
        }

        public Maybe<string> MatchCvrNumber()
        {
            var cvrNumberAttributes = Navigator
                .GetAttribute(StsAdgangsStyringConstants.Attributes.CvrNumber)
                .Select(x => x.AttributeValue)
                .GetValueOrFallback(new string[0]);

            foreach (var cvrNumber in cvrNumberAttributes)
            {
                if (!string.IsNullOrEmpty(cvrNumber))
                {
                    return cvrNumber;
                }
            }
            return Maybe<string>.None;
        }

        public Maybe<KitosSamlPrivilege> MatchPrivilege(string privilegeId)
        {
            return
                Navigator
                    .GetPrivilegeNodes()
                    .FirstOrDefault(xml => xml.InnerText.EndsWith(privilegeId, StringComparison.OrdinalIgnoreCase))
                    .FromNullable()
                    .Match(xml => new KitosSamlPrivilege(xml.InnerText), onNone: () => Maybe<KitosSamlPrivilege>.None);
        }

        public Maybe<SamlUserUuid> MatchUuid()
        {
            return
                Navigator
                    .GetSerials()
                    .Match(serials =>
                    {
                        foreach (var serial in serials)
                        {
                            if (Guid.TryParse(serial, out var userUuid))
                            {
                                return new SamlUserUuid(userUuid);
                            }
                        }

                        return Maybe<SamlUserUuid>.None;
                    }, () => Maybe<SamlUserUuid>.None);
        }
    }
}
