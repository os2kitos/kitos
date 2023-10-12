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
using Infrastructure.STS.Common.Model.Client;
using Infrastructure.STS.OrganizationSystem.OrganisationSystem;
using Infrastructure.STS.OrganizationUnit.ServiceReference;
using OrganisationSystem;
using Serilog;
using AktoerTypeKodeType = OrganisationSystem.AktoerTypeKodeType;
using GyldighedStatusKodeType = Infrastructure.STS.OrganizationUnit.ServiceReference.GyldighedStatusKodeType;
using GyldighedType = Infrastructure.STS.OrganizationUnit.ServiceReference.GyldighedType;
using ItemChoiceType = Infrastructure.STS.OrganizationUnit.ServiceReference.ItemChoiceType;
using OrganisationRelationType = Infrastructure.STS.OrganizationUnit.ServiceReference.OrganisationRelationType;
using RegistreringType1 = Infrastructure.STS.OrganizationUnit.ServiceReference.RegistreringType1;
using RelationListeType = Infrastructure.STS.OrganizationUnit.ServiceReference.RelationListeType;
using SoegInputType1 = Infrastructure.STS.OrganizationUnit.ServiceReference.SoegInputType1;
using TilstandListeType = Infrastructure.STS.OrganizationUnit.ServiceReference.TilstandListeType;
using UnikIdType = Infrastructure.STS.OrganizationUnit.ServiceReference.UnikIdType;

namespace Infrastructure.STS.OrganizationSystem.DomainServices
{
    public class StsOrganizationSystemService : IStsOrganizationUnitService
    {
        private readonly IStsOrganizationService _organizationService;
        private readonly ILogger _logger;
        private readonly string _certificateThumbprint;
        private readonly string _serviceRoot;

        public StsOrganizationSystemService(IStsOrganizationService organizationService, StsOrganisationIntegrationConfiguration configuration, ILogger logger)
        {
            _organizationService = organizationService;
            _logger = logger;
            _certificateThumbprint = configuration.CertificateThumbprint;
            _serviceRoot = $"https://{configuration.EndpointHost}/service/Organisation/OrganisationSystem/5";
        }

        public Result<ExternalOrganizationUnit, DetailedOperationError<ResolveOrganizationTreeError>> ResolveOrganizationTree(Organization organization)
        {
            //Resolve the org uuid
            var uuid = _organizationService.ResolveStsOrganizationUuid(organization);
            if (uuid.Failed)
            {
                var error = uuid.Error;
                _logger.Error("Loading sts organization uuid from org with id: {id} failed with {detailedError} {errorCode} {errorMessage}", organization.Id, error.Detail, error.FailureType, error.Message.GetValueOrFallback(""));
                return new DetailedOperationError<ResolveOrganizationTreeError>(error.FailureType, ResolveOrganizationTreeError.FailedResolvingUuid, $"{error.Detail}:{error.Message}");
            }

            //Search for org units by org uuid
            using var clientCertificate = X509CertificateClientCertificateFactory.GetClientCertificate(_certificateThumbprint);

            const int pageSize = 500;
            var totalIds = new List<string>();
            var totalResults = new List<(Guid, RegistreringType1)>();
            var currentPage = new List<string>();
            var organizationStsUuid = uuid.Value;

            using var client = CreateClient(BasicHttpBindingFactory.CreateHttpBinding(), _serviceRoot, clientCertificate);
            var channel = client.ChannelFactory.CreateChannel();
            do
            {
                currentPage.Clear();

                totalIds.AddRange(currentPage);
                
                var listRequest = CreateOrgHierarchyRequest(organization.Cvr);
                var listResponse = LoadOrganizationHierarchy(channel, listRequest);
                var res = listResponse.FremsoegObjekthierarkiOutput.Organisationer;
                /*var listStatusResult = listResponse.ListResponse1.ListOutput.StandardRetur;
                var listStsError = listStatusResult.StatusKode.ParseStsErrorFromStandardResultCode();
                if (listStsError.HasValue)
                {
                    _logger.Error("Failed to list units for org units for org with sts uuid: {stsuuid} and unit uuids: {uuids} failed with {code} {message}", organizationStsUuid, string.Join(",", currentPage), listStatusResult.StatusKode, listStatusResult.FejlbeskedTekst);
                    return new DetailedOperationError<ResolveOrganizationTreeError>(OperationFailure.UnknownError, ResolveOrganizationTreeError.FailedLoadingOrgUnits);

                }

                var units = listResponse
                    .ListResponse1
                    .ListOutput
                    .FiltreretOejebliksbillede
                    .Select(snapshot => (new Guid(snapshot.ObjektType.UUIDIdentifikator), snapshot.Registrering.OrderByDescending(x => x.Tidspunkt).FirstOrDefault()))
                    .Where(x => x.Item2 != null);*/

                //totalResults.AddRange(units);

            } while (currentPage.Count == pageSize);

            // Prepare conversion to import tree
            var unitsByUuid = totalResults.ToDictionary(unit => unit.Item1);
            var unitsByParent = totalResults
                .Where(x => x.Item2.RelationListe.Overordnet != null) // exclude the root
                .GroupBy(unit => new Guid(unit.Item2.RelationListe.Overordnet.ReferenceID.Item))
                .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

            var roots = totalResults.Where(x => x.Item2.RelationListe.Overordnet == null).ToList();
            if (roots.Count > 1)
            {
                //More than one root detected
                var rootIdResult = _organizationService.ResolveOrganizationHierarchyRootUuid(organization);
                if (rootIdResult.Failed)
                {
                    var error = rootIdResult.Error;
                    _logger.Error("Failed to resolve root id for org {id}. Failed with {errorCode}:{error}", organization.Id, error.FailureType, error.Message.GetValueOrFallback(""));
                    return new DetailedOperationError<ResolveOrganizationTreeError>(OperationFailure.UnknownError, ResolveOrganizationTreeError.FailedToLookupRootUnit, "Failed to determine root unit");
                }

                var currentRootId = rootIdResult.Value;
                var currentRootResult = roots.Where(root => root.Item1 == currentRootId).ToList();
                if (currentRootResult.Count != 1)
                {
                    _logger.Error("Failed to resolve root for org {id}. Root id was supposed to be {rootId} but that was not found in the collection of root units", organization.Id, currentRootId);
                    return new DetailedOperationError<ResolveOrganizationTreeError>(OperationFailure.UnknownError, ResolveOrganizationTreeError.FailedToLookupRootUnit, "Failed to find root unit from known root unit id");
                }

                var rootsToRemove = roots.Except(currentRootResult).ToList();
                roots = currentRootResult; //remove the other roots

                //Purge the secondary org trees from the known structure
                var idsOfUnitsToPurge = new Queue<Guid>(rootsToRemove.Select(x => x.Item1));
                while (idsOfUnitsToPurge.Count != 0)
                {
                    var current = idsOfUnitsToPurge.Dequeue();
                    if (unitsByParent.TryGetValue(current, out var children))
                    {
                        children.ToList().ForEach(child => idsOfUnitsToPurge.Enqueue(child.Item1));
                        unitsByParent.Remove(current);
                    }
                    unitsByUuid.Remove(current);
                }
            }

            // Process the tree info from sts org in order to generate the import tree
            var parentIdToConvertedChildren = new Dictionary<Guid, List<ExternalOrganizationUnit>>();
            var idToConvertedChildren = new Dictionary<Guid, ExternalOrganizationUnit>();
            var root = roots.Single();

            var processingStack = CreateOrgUnitConversionStack(root, unitsByParent);

            while (processingStack.Any())
            {
                var currentUnitUuid = processingStack.Pop();
                (Guid, RegistreringType1) unit = unitsByUuid[currentUnitUuid];

                var egenskabType = unit.Item2.AttributListe.Egenskab.First(x => string.IsNullOrEmpty(x.EnhedNavn) == false);
                var unitUuid = unit.Item1;
                var organizationUnit = new ExternalOrganizationUnit(unitUuid, egenskabType.EnhedNavn, new Dictionary<string, string>() { { "UserFacingKey", egenskabType.BrugervendtNoegleTekst } }, parentIdToConvertedChildren.ContainsKey(unitUuid) ? parentIdToConvertedChildren[unitUuid] : new List<ExternalOrganizationUnit>(0));
                idToConvertedChildren[organizationUnit.Uuid] = organizationUnit;
                var parentUnit = unit.Item2.RelationListe.Overordnet;
                if (parentUnit != null)
                {
                    var parentId = new Guid(parentUnit.ReferenceID.Item);
                    if (!parentIdToConvertedChildren.TryGetValue(parentId, out var parentToChildrenList))
                    {
                        parentToChildrenList = new List<ExternalOrganizationUnit>();
                        parentIdToConvertedChildren[parentId] = parentToChildrenList;
                    }
                    parentToChildrenList.Add(organizationUnit);
                }
            }

            return idToConvertedChildren[root.Item1];

        }
        
        private static fremsoegobjekthierarkiResponse LoadOrganizationHierarchy(OrganisationSystemPortType channel, fremsoegobjekthierarkiRequest request)
        {
            return new RetriedIntegrationRequest<fremsoegobjekthierarkiResponse>(() => channel.fremsoegobjekthierarkiAsync(request).Result).Execute();
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

        public static fremsoegobjekthierarkiRequest CreateOrgHierarchyRequest(string municipalityCvr)
        {
            var listRequest = new fremsoegobjekthierarkiRequest
            {
                FremsoegobjekthierarkiRequest1 = new FremsoegobjekthierarkiRequestType()
                FremsoegObjekthierarkiInput = new FremsoegObjekthierarkiInputType()
                {
                    OrganisationSoegEgenskab = new OrganisationSystem.EgenskabType()
                    {
                        
                        Virkning = new OrganisationSystem.VirkningType
                        {
                            AktoerTypeKode = AktoerTypeKodeType.Organisation,
                            AktoerRef = new OrganisationSystem.UnikIdType
                            {
                                Item = municipalityCvr,
                                ItemElementName = OrganisationSystem.ItemChoiceType.UUIDIdentifikator //should be CVR?
                            }
                        }
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

        private static OrganisationSystemPortTypeClient CreateClient(BasicHttpBinding binding, string urlServicePlatformService, X509Certificate2 certificate)
        {
            return new OrganisationSystemPortTypeClient(binding, new EndpointAddress(urlServicePlatformService))
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
