using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Core.Abstractions.Types;
using Core.DomainServices.Extensions;
using Core.DomainServices.Organizations;
using Core.DomainServices.SSO;
using Infrastructure.STS.Common.Factories;
using Infrastructure.STS.Organization.ServiceReference;
using Newtonsoft.Json;

namespace Infrastructure.STS.Organization.DomainServices
{
    public class StsOrganizationService : IStsOrganizationService
    {
        private readonly string _certificateThumbprint;
        private readonly string _serviceRoot;

        public StsOrganizationService(StsOrganisationIntegrationConfiguration configuration)
        {
            _certificateThumbprint = configuration.CertificateThumbprint;
            _serviceRoot = $"https://{configuration.EndpointHost}/service/Organisation/Organisation/5";
        }

        public Result<Guid, OperationError> ResolveStsOrganizationUuid(Core.DomainModel.Organization.Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            var fkOrgIdentity = organization.SsoIdentities.FirstOrDefault();
            if (fkOrgIdentity != null)
            {
                return fkOrgIdentity.ExternalUuid;
            }

            if (organization.IsCvrInvalid())
            {
                return new OperationError("Organization is missing CVR or has an invalid CVR", OperationFailure.BadInput);
            }

            //TODO: Get the company by cvr first and then filter by virksomhed uuid

            using var clientCertificate = X509CertificateClientCertificateFactory.GetClientCertificate(_certificateThumbprint);
            using var organizationPortTypeClient = CreateOrganizationPortTypeClient(BasicHttpBindingFactory.CreateHttpBinding(), _serviceRoot, clientCertificate);

            var searchRequest = CreateSearchForOrganizationRequest(organization, new Guid("7302f1a5-bbec-4439-a1ec-ea605bdf5ab3")); //TODO: Get the company uuid from a different service "Virksomhed"
            var channel = organizationPortTypeClient.ChannelFactory.CreateChannel();


            var response = channel.soeg(searchRequest);
            var statusResult = response.SoegResponse1.SoegOutput.StandardRetur;
            if (statusResult.StatusKode != "20") //TODO: Create helper
            {
                return new OperationError($"Error resolving the organization from STS:{statusResult.StatusKode}:{statusResult.FejlbeskedTekst}", OperationFailure.UnknownError);
            }

            var ids = response.SoegResponse1.SoegOutput.IdListe;
            if (ids.Length != 1)
            {
                return new OperationError($"Error resolving the organization from STS. Expected a single UUID but got:{string.Join(",", ids)}", OperationFailure.UnknownError);
            }

            //TODO: Remember to save the uuid on the organization!-> need a databasecontrol thing for that
            return new Guid(ids.Single());
        }

        private static soegRequest CreateSearchForOrganizationRequest(Core.DomainModel.Organization.Organization organization, Guid companyUuid)
        {
            return new soegRequest
            {
                SoegRequest1 = new SoegRequestType
                {
                    AuthorityContext = new AuthorityContextType
                    {
                        MunicipalityCVR = organization.Cvr
                    },
                    SoegInput = new SoegInputType1
                    {
                        MaksimalAntalKvantitet = "2", //We expect only one match so get 2 as max to see if something is off
                        AttributListe = new AttributListeType(), //Required by the schema even if it is not used
                        TilstandListe = new TilstandListeType(), //Required by the schema even if it is not used
                        RelationListe = new RelationListeType
                        {
                            Virksomhed = new VirksomhedRelationType
                            {
                                ReferenceID = new UnikIdType
                                {
                                    ItemElementName = ItemChoiceType.UUIDIdentifikator,
                                    Item = companyUuid.ToString("D")
                                }
                            }
                        }
                    }
                }
            };
        }

        private static OrganisationPortTypeClient CreateOrganizationPortTypeClient(BasicHttpBinding binding, string urlServicePlatformService, X509Certificate2 certificate)
        {
            return new OrganisationPortTypeClient(binding, new EndpointAddress(urlServicePlatformService))
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
