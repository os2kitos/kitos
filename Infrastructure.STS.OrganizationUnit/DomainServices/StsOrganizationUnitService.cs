using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;
using Core.DomainServices.Organizations;
using Core.DomainServices.SSO;
using Infrastructure.STS.Common.Factories;
using Infrastructure.STS.OrganizationUnit.ServiceReference;

namespace Infrastructure.STS.OrganizationUnit.DomainServices
{

    public class StsOrganizationUnitService : IStsOrganizationUnitService
    {
        private readonly IStsOrganizationService _organizationService;
        private readonly string _certificateThumbprint;
        private readonly string _serviceRoot;

        public StsOrganizationUnitService(IStsOrganizationService organizationService, StsOrganisationIntegrationConfiguration configuration)
        {
            _organizationService = organizationService;
            _certificateThumbprint = configuration.CertificateThumbprint;
            _serviceRoot = $"https://{configuration.EndpointHost}/service/Organisation/OrganisationEnhed/5";
        }

        public Result<StsOrganizationUnit, OperationError> ResolveOrganizationTree(Organization organization)
        {
            //Resolve the org uuid
            var uuid = _organizationService.ResolveStsOrganizationUuid(organization);
            if (uuid.Failed)
            {
                //TODO: Correct error and logging
                return uuid.Error;
            }

            //Search for org units by org uuid
            using var clientCertificate = X509CertificateClientCertificateFactory.GetClientCertificate(_certificateThumbprint);
            using var client = CreateClient(BasicHttpBindingFactory.CreateHttpBinding(), _serviceRoot, clientCertificate);


            var channel = client.ChannelFactory.CreateChannel();

            const int pageSize = 100;
            var totalIds = new List<string>();
            var totalResults = new List<RegistreringType1>();
            var currentPage = new List<string>();
            do
            {
                currentPage.Clear();
                var searchRequest = CreateSearchOrgUnitsByOrgUuidRequest(organization.Cvr, uuid.Value, pageSize, totalIds.Count);
                var searchResponse = channel.soeg(searchRequest);
                
                //TODO: check errors
                
                currentPage = searchResponse.SoegResponse1.SoegOutput.IdListe.ToList();
                totalIds.AddRange(currentPage);

                var listRequest = CreateListOrgUnitsRequest(organization.Cvr, currentPage.ToArray());
                var listResponse = channel.list(listRequest);
                
                //TODO: check errors

                var units = listResponse.ListResponse1.ListOutput.FiltreretOejebliksbillede.SelectMany(filtreretOejebliksbilledeType => filtreretOejebliksbilledeType.Registrering);
                totalResults.AddRange(units);
            } while (currentPage.Count == pageSize);


            throw new NotImplementedException("TODO: convert to result model");

        }

        public static listRequest CreateListOrgUnitsRequest(string municipalityCvr, params string[] currentUnitUuids)
        {
            var listRequest = new listRequest
            {
                ListRequest1 = new ListRequestType
                {
                    AuthorityContext = new AuthorityContextType
                    {
                        MunicipalityCVR = municipalityCvr
                    },
                    ListInput = new ListInputType
                    {
                        UUIDIdentifikator = currentUnitUuids
                    }
                }
            };
            return listRequest;
        }

        public static soegRequest CreateSearchOrgUnitsByOrgUuidRequest(string municipalityCvr, Guid organizationUuid, int pageSize, int skip = 0)
        {
            return new soegRequest
            {
                SoegRequest1 = new SoegRequestType
                {
                    AuthorityContext = new AuthorityContextType
                    {
                        MunicipalityCVR = municipalityCvr
                    },
                    SoegInput = new SoegInputType1
                    {
                        AttributListe = new AttributListeType(), //Required by schema validation
                        TilstandListe = new TilstandListeType(), //Required by schema validation
                        RelationListe = new RelationListeType
                        {
                            Tilhoerer = new OrganisationRelationType
                            {
                                ReferenceID = new UnikIdType
                                {
                                    Item = organizationUuid.ToString("D"),
                                    ItemElementName = ItemChoiceType.UUIDIdentifikator
                                }
                            }
                        },
                        MaksimalAntalKvantitet = pageSize.ToString("D"),
                        FoersteResultatReference = skip.ToString("D")
                    }
                }
            };
        }

        private static OrganisationEnhedPortTypeClient CreateClient(BasicHttpBinding binding, string urlServicePlatformService, X509Certificate2 certificate)
        {
            return new OrganisationEnhedPortTypeClient(binding, new EndpointAddress(urlServicePlatformService))
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
