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
            var totalResults = new List<(Guid, RegistreringType1)>();
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

                var units = listResponse
                    .ListResponse1
                    .ListOutput
                    .FiltreretOejebliksbillede
                    .Select(snapshot => (new Guid(snapshot.ObjektType.UUIDIdentifikator), snapshot.Registrering.OrderByDescending(x => x.Tidspunkt).FirstOrDefault()))
                    .Where(x => x.Item2 != null);

                totalResults.AddRange(units);

            } while (currentPage.Count == pageSize);

            // Prepare conversion to import tree
            var unitsByUuid = totalResults.ToDictionary(unit => unit.Item1);
            var unitsByParent = totalResults
                .Where(x => x.Item2.RelationListe.Overordnet != null) // exclude the root
                .GroupBy(unit => new Guid(unit.Item2.RelationListe.Overordnet.ReferenceID.Item))
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
            var roots = totalResults.Where(x => x.Item2.RelationListe.Overordnet == null).ToList();
            if (roots.Count != 1)
            {
                //TODO: error
            }

            // Process the tree info from sts org in order to generate the import tree
            var parentIdToConvertedChildren = new Dictionary<Guid, List<StsOrganizationUnit>>();
            var idToConvertedChildren = new Dictionary<Guid, StsOrganizationUnit>();
            var root = roots.Single();

            var processingStack = CreateOrgUnitConversionStack(root, unitsByParent);

            while (processingStack.Any())
            {
                var currentUnitUuid = processingStack.Pop();
                (Guid, RegistreringType1) unit = unitsByUuid[currentUnitUuid];

                var egenskabType = unit.Item2.AttributListe.Egenskab[0];//TODO: Check if we can always depend on this to be there
                var unitUuid = unit.Item1;
                var organizationUnit = new StsOrganizationUnit(unitUuid, egenskabType.EnhedNavn, egenskabType.BrugervendtNoegleTekst, parentIdToConvertedChildren.ContainsKey(unitUuid) ? parentIdToConvertedChildren[unitUuid] : new List<StsOrganizationUnit>(0));
                idToConvertedChildren[organizationUnit.Uuid] = organizationUnit;
                var parentUnit = unit.Item2.RelationListe.Overordnet;
                if (parentUnit != null)
                {
                    var parentId = new Guid(parentUnit.ReferenceID.Item);
                    if (!parentIdToConvertedChildren.TryGetValue(parentId, out var parentToChildrenList))
                    {
                        parentToChildrenList = new List<StsOrganizationUnit>();
                        parentIdToConvertedChildren[parentId] = parentToChildrenList;
                    }
                    parentToChildrenList.Add(organizationUnit);
                }
            }

            return idToConvertedChildren[root.Item1];

        }

        private static Stack<Guid> CreateOrgUnitConversionStack((Guid, RegistreringType1) root, Dictionary<Guid, List<(Guid, RegistreringType1)>> unitsByParent)
        {
            var processingStack = new Stack<Guid>();
            processingStack.Push(root.Item1);

            //Flatten the tree and have the leafs at the top of the stack
            var currentChildren = unitsByParent[processingStack.Peek()];
            foreach (var currentChild in currentChildren)
            {
                foreach (var unitId in GetSubTree(currentChild, unitsByParent))
                {
                    processingStack.Push(unitId);
                }
            }

            return processingStack;
        }

        private static IEnumerable<Guid> GetSubTree((Guid, RegistreringType1) currentChild, Dictionary<Guid, List<(Guid, RegistreringType1)>> unitsByParent)
        {
            var id = currentChild.Item1;

            //Current level
            yield return id;

            //Append the sub tree
            if (unitsByParent.TryGetValue(id, out var children))
            {
                foreach (var child in children)
                {
                    foreach (var uuid in GetSubTree(child, unitsByParent))
                    {
                        yield return uuid;
                    }

                }
            }
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
                        TilstandListe = new TilstandListeType()
                        {
                            Gyldighed = new GyldighedType[]
                            {
                                new()
                                {
                                    GyldighedStatusKode = GyldighedStatusKodeType.Aktiv
                                }
                            }
                        }, //Required by schema validation
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
