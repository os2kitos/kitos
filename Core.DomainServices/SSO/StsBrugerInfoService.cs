﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace Core.DomainServices.SSO
{
    public class StsBrugerInfoService : IStsBrugerInfoService
    {
        private const string EmailTypeIdentifier = StsOrganisationConstants.UserProperties.Email;

        private readonly string _urlServicePlatformBrugerService;
        private readonly string _urlServicePlatformAdresseService;
        private readonly string _urlServicePlatformOrganisationService;
        private readonly string _urlServicePlatformVirksomhedService;
        private readonly string _certificateThumbprint;
        private readonly string _authorizedMunicipalityCvr;

        public StsBrugerInfoService(StsOrganisationIntegrationConfiguration configuration)
        {
            _certificateThumbprint = configuration.CertificateThumbprint;
            _urlServicePlatformBrugerService = $"https://{configuration.EndpointHost}/service/Organisation/Bruger/5";
            _urlServicePlatformAdresseService = $"https://{configuration.EndpointHost}/service/Organisation/Adresse/5";
            _urlServicePlatformOrganisationService = $"https://{configuration.EndpointHost}/service/Organisation/Organisation/5";
            _urlServicePlatformVirksomhedService = $"https://{configuration.EndpointHost}/service/Organisation/Virksomhed/5";
            _authorizedMunicipalityCvr = configuration.AuthorizedMunicipalityCvr;
        }

        public StsBrugerInfo GetStsBrugerInfo(Guid uuid)
        {
            var (emailAdresseUuid, organisationUuid) = GetStsBrugerEmailAdresseAndOrganizationUuids(uuid);
            var emails = GetStsAdresseEmailFromUuid(emailAdresseUuid);
            var virksomhedUuid = GetStsVirksomhedFromUuid(organisationUuid);
            var municipalityCvr = GetStsBrugerMunicipalityCvrFromUuid(virksomhedUuid);
            return new StsBrugerInfo(emails, organisationUuid, municipalityCvr);
        }

        private (string emailAdresseUuid, string organisationUuid) GetStsBrugerEmailAdresseAndOrganizationUuids(Guid uuid)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsBrugerHelpers.CreateBrugerPortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformBrugerService, clientCertificate);
                var laesRequest = StsBrugerHelpers.CreateStsBrugerLaesRequest(_authorizedMunicipalityCvr, uuid);
                var brugerPortType = client.ChannelFactory.CreateChannel();
                var laesResponse = brugerPortType.laes(laesRequest);
                foreach (var registreringType1 in laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering)
                {
                    if (registreringType1.IsStsBrugerObsolete())
                    {
                        continue;
                    }

                    var organizationUuid = registreringType1.RelationListe.Tilhoerer.ReferenceID.Item;
                    var emailUuid = string.Empty;
                    foreach (var adresse in registreringType1.RelationListe.Adresser)
                    {
                        if (EmailTypeIdentifier.Equals(adresse.Rolle.Item))
                        {
                            emailUuid = adresse.ReferenceID.Item;
                        }
                    }

                    return (emailUuid, organizationUuid);
                }
                return (string.Empty, string.Empty);
            }
        }

        private IEnumerable<string> GetStsAdresseEmailFromUuid(string emailAdresseUuid)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsAdresseHelpers.CreateAdressePortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformAdresseService, clientCertificate);
                var laesRequest = StsAdresseHelpers.CreateStsAdresseLaesRequest(_authorizedMunicipalityCvr, emailAdresseUuid);
                var adressePortType = client.ChannelFactory.CreateChannel();
                var laesResponse = adressePortType.laes(laesRequest);
                var registreringType1s = laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering;
                var result = new List<string>();
                foreach (var registreringType1 in registreringType1s)
                {
                    if (registreringType1.IsStsAdresseObsolete())
                    {
                        continue;
                    }

                    var latest = registreringType1.AttributListe.OrderByDescending(a => a.Virkning.TilTidspunkt).First();
                    result.Add(latest.AdresseTekst);
                }
                return result;
            }
        }

        private string GetStsVirksomhedFromUuid(string organisationUuid)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsOrganisationHelpers.CreateOrganisationPortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformOrganisationService, clientCertificate);
                var laesRequest = StsOrganisationHelpers.CreateStsOrganisationLaesRequest(_authorizedMunicipalityCvr, organisationUuid);
                var organisationPortType = client.ChannelFactory.CreateChannel();
                var laesResponse = organisationPortType.laes(laesRequest);
                var registreringType1s = laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering;
                foreach (var registreringType1 in registreringType1s)
                {
                    if (registreringType1.IsStsOrganisationObsolete())
                    {
                        continue;
                    }

                    return registreringType1.RelationListe.Virksomhed.ReferenceID.Item;
                }
                return string.Empty;
            }
        }

        private string GetStsBrugerMunicipalityCvrFromUuid(string virksomhedUuid)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsVirksomhedHelpers.CreateVirksomhedPortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformVirksomhedService, clientCertificate);
                var laesRequest = StsVirksomhedHelpers.CreateStsVirksomhedLaesRequest(_authorizedMunicipalityCvr, virksomhedUuid);
                var virksomhedPortType = client.ChannelFactory.CreateChannel();
                var laesResponse = virksomhedPortType.laes(laesRequest);
                var registreringType1s = laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering;
                foreach (var registreringType1 in registreringType1s)
                {
                    if (registreringType1.IsStsVirksomhedObsolete())
                    {
                        continue;
                    }

                    var latest = registreringType1.AttributListe.OrderByDescending(a => a.Virkning.TilTidspunkt).First();
                    return latest.CVRNummerTekst;
                }
                return string.Empty;
            }
        }

        private static BasicHttpBinding CreateHttpBinding()
        {
            return new BasicHttpBinding
            {
                Security =
                {
                    Mode = BasicHttpSecurityMode.Transport,
                    Transport = {ClientCredentialType = HttpClientCredentialType.Certificate}
                },
                MaxReceivedMessageSize = int.MaxValue,
                OpenTimeout = new TimeSpan(0, 3, 0),
                CloseTimeout = new TimeSpan(0, 3, 0),
                ReceiveTimeout = new TimeSpan(0, 3, 0),
                SendTimeout = new TimeSpan(0, 3, 0)
            };
        }

        private static X509Certificate2 GetClientCertificate(string thumbprint)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2 result;
                try
                {
                    var results = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                    if (results.Count == 0)
                    {
                        throw new Exception("Unable to find certificate!");
                    }
                    result = results[0];
                }
                finally
                {
                    store.Close();
                }
                return result;
            }
        }
    }
}
