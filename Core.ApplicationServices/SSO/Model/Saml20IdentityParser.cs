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

        public static Saml20IdentityParser CreateFromContext()
        {
            return new Saml20IdentityParser(new Saml20IdentityNavigator(Saml20Identity.Current));
        }

        public Maybe<KitosSamlPrivilege> MatchPrivilege(string privilegeId)
        {
            return
                _navigator
                    .MatchPrivilegeNode()
                    .Match(xml => xml.InnerText.EndsWith(privilegeId, StringComparison.OrdinalIgnoreCase)
                            ? new KitosSamlPrivilege(xml.InnerText)
                            : Maybe<KitosSamlPrivilege>.None, onNone: () => Maybe<KitosSamlPrivilege>.None);
        }

        public Maybe<SamlUserUuid> MatchUuid()
        {
            return
                _navigator
                    .MatchSerials()
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
