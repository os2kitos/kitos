using System;
using Core.DomainModel.Result;
using dk.nita.saml20.identity;

namespace Core.ApplicationServices.SSO.Model
{
    public class Saml20IdentityParser
    {
        private readonly Saml20IdentityNavigator _navigator;

        private Saml20IdentityParser(Saml20IdentityNavigator navigator)
        {
            _navigator = navigator;
        }

        public static Saml20IdentityParser CreateFrom(ISaml20Identity sourceIdentity)
        {
            return new Saml20IdentityParser(new Saml20IdentityNavigator(sourceIdentity));
        }

        public Maybe<string> MatchCvrNumber()
        {
            var cvrNumberAttributes = _navigator
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
                _navigator
                    .GetPrivilegeNode()
                    .Match(xml => xml.InnerText.EndsWith(privilegeId, StringComparison.OrdinalIgnoreCase)
                            ? new KitosSamlPrivilege(xml.InnerText)
                            : Maybe<KitosSamlPrivilege>.None, onNone: () => Maybe<KitosSamlPrivilege>.None);
        }

        public Maybe<SamlUserUuid> MatchUuid()
        {
            return
                _navigator
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
