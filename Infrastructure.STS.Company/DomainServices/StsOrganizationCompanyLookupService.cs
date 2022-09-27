using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Core.DomainServices.Organizations;
using Core.DomainServices.SSO;
using Infrastructure.STS.Common.Factories;
using Infrastructure.STS.Common.Model;
using Infrastructure.STS.Company.ServiceReference;

namespace Infrastructure.STS.Company.DomainServices
{
    public class StsOrganizationCompanyLookupService : IStsOrganizationCompanyLookupService
    {
        private readonly string _certificateThumbprint;
        private readonly string _serviceRoot;

        public StsOrganizationCompanyLookupService(StsOrganisationIntegrationConfiguration configuration)
        {
            _certificateThumbprint = configuration.CertificateThumbprint;
            _serviceRoot = $"https://{configuration.EndpointHost}/service/Organisation/Virksomhed/5";
        }

        //TODO: More detailed response - we need the info for the validation endpoint!
        public Result<Guid, OperationError> ResolveStsOrganizationCompanyUuid(Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }
            using var clientCertificate = X509CertificateClientCertificateFactory.GetClientCertificate(_certificateThumbprint);
            using var organizationPortTypeClient = CreateClient(BasicHttpBindingFactory.CreateHttpBinding(), _serviceRoot, clientCertificate);

            var channel = organizationPortTypeClient.ChannelFactory.CreateChannel();
            var request = CreateSearchByCvrRequest(organization);
            var response = channel.soeg(request);

            var statusResult = response.SoegResponse1.SoegOutput.StandardRetur;
            //TODO: Extend with service agreement related - test by calling into places we dont have an agreement: https://www.serviceplatformen.dk/administration/errorcodes-doc/errorcodes/4afb35be-7b7a-45b3-ab01-bd5017a8b182_errorcodes.html
            var stsError = statusResult.StatusKode.ParseStsError(); 
            if (stsError.HasValue)
            {
                return new OperationError($"Error resolving the organization company from STS:{statusResult.StatusKode}:{statusResult.FejlbeskedTekst}", OperationFailure.UnknownError);
            }

            var ids = response.SoegResponse1.SoegOutput.IdListe;
            if (ids.Length != 1)
            {
                return new OperationError($"Error resolving the organization company from STS. Expected a single UUID but got:{string.Join(",", ids)}", OperationFailure.UnknownError);
            }

            return new Guid(ids.Single());
        }

        private static soegRequest CreateSearchByCvrRequest(Organization organization)
        {
            return new soegRequest
            {
                SoegRequest1 = new SoegRequestType
                {
                    AuthorityContext = new AuthorityContextType
                    {
                        MunicipalityCVR = organization.Cvr
                    },
                    SoegInput = new SoegInputType1()
                    {
                        RelationListe = new RelationListeType(),
                        FoersteResultatReference = "0",
                        MaksimalAntalKvantitet = "2",
                        SoegRegistrering = new SoegRegistreringType(),
                        TilstandListe = new TilstandListeType(),
                        AttributListe = new[]{new EgenskabType
                        {
                            CVRNummerTekst = organization.Cvr
                        }}
                    }
                }
            };
        }

        private static VirksomhedPortTypeClient CreateClient(BasicHttpBinding binding, string urlServicePlatformService, X509Certificate2 certificate)
        {
            return new VirksomhedPortTypeClient(binding, new EndpointAddress(urlServicePlatformService))
            {
                ClientCredentials =
                {
                    ClientCertificate =
                    {
                        Certificate = certificate
                    }
                }
            };
        }
    }
}
