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
using Infrastructure.STS.Common.Model;
using Infrastructure.STS.Common.Model.Client;
using Infrastructure.STS.Common.Model.Token;
using Infrastructure.STS.OrganizationSystem.OrganisationSystem;
using Serilog;

namespace Infrastructure.STS.OrganizationSystem.DomainServices
{
    public class StsOrganizationSystemService : IStsOrganizationSystemService
    {
        private readonly IStsOrganizationService _organizationService;
        private readonly ILogger _logger;
        private readonly string _certificateThumbprint;
        private readonly string _endpoint;
        private readonly string _issuer;
        private readonly string _serviceRoot;

        private const string EntityId = "http://stoettesystemerne.dk/service/organisation/3";

        public StsOrganizationSystemService(IStsOrganizationService organizationService, StsOrganisationIntegrationConfiguration configuration, ILogger logger)
        {
            _organizationService = organizationService;
            _logger = logger;
            _certificateThumbprint = configuration.CertificateThumbprint;
            _endpoint = configuration.CertificateEndpoint;
            _issuer = configuration.Issuer;
            _serviceRoot = $"https://organisation.{configuration.EndpointHost}/organisation/organisationsystem/6";
        }

        public Result<ExternalOrganizationUnit, DetailedOperationError<ResolveOrganizationTreeError>> ResolveOrganizationTree(Organization organization)
        {
            //Search for org units by org uuid
            using var clientCertificate = X509CertificateClientCertificateFactory.GetClientCertificate(_certificateThumbprint);

            const int pageSize = 1000;
            int currentPageSize;
            var totalIds = 0;
            var totalResults = new List<(Guid, RegistreringType9)>();

            using var client = CreateClient(BasicHttpBindingFactory.CreateHttpBinding(), _serviceRoot, clientCertificate);
            var token = TokenFetcher.IssueToken(EntityId, organization.Cvr, _certificateThumbprint, _endpoint, _issuer);
            var channel = client.ChannelFactory.CreateChannelWithIssuedToken(token);
            do
            {
                var listRequest = CreateOrgHierarchyRequest(organization.Uuid.ToString(), pageSize, totalIds);
                var listResponse = LoadOrganizationHierarchy(channel, listRequest);

                var listStatusResult = listResponse.FremsoegObjekthierarkiOutput.StandardRetur;
                var listStsError = listStatusResult.StatusKode.ParseStsErrorFromStandardResultCode();
                if (listStsError.HasValue)
                {
                    _logger.Error("Failed to query org units for org with cvr {orgCvr} failed with {code} {message}", organization.Cvr, listStatusResult.StatusKode, listStatusResult.FejlbeskedTekst);
                    return new DetailedOperationError<ResolveOrganizationTreeError>(OperationFailure.UnknownError, ResolveOrganizationTreeError.FailedLoadingOrgUnits);
                }

                var listResponseUnits = listResponse.FremsoegObjekthierarkiOutput.OrganisationEnheder;
                var numberOfReturnedUnits = listResponseUnits.Length;

                totalIds += numberOfReturnedUnits;
                currentPageSize = numberOfReturnedUnits;

                var unitUuidAndDataList = listResponseUnits
                    .Select(snapshot => (new Guid(snapshot.ObjektType.UUIDIdentifikator), snapshot.Registrering.OrderByDescending(x => x.Tidspunkt).FirstOrDefault()))
                    .Where(x => x.Item2 != null);

                totalResults.AddRange(unitUuidAndDataList);

            } while (currentPageSize == pageSize);

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
                var (unitUuid, registreringType5) = unitsByUuid[currentUnitUuid];

                var egenskabType = registreringType5.AttributListe.Egenskab.First(x => string.IsNullOrEmpty(x.EnhedNavn) == false);
                var organizationUnit = new ExternalOrganizationUnit(unitUuid, egenskabType.EnhedNavn, new Dictionary<string, string>() { { "UserFacingKey", egenskabType.BrugervendtNoegleTekst } }, parentIdToConvertedChildren.ContainsKey(unitUuid) ? parentIdToConvertedChildren[unitUuid] : new List<ExternalOrganizationUnit>(0));
                idToConvertedChildren[organizationUnit.Uuid] = organizationUnit;
                var parentUnit = registreringType5.RelationListe.Overordnet;
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

        private static Stack<Guid> CreateOrgUnitConversionStack((Guid, RegistreringType9) root, Dictionary<Guid, List<(Guid, RegistreringType9)>> unitsByParent)
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

        private static IEnumerable<Guid> GetSubTree((Guid, RegistreringType9) currentChild, Dictionary<Guid, List<(Guid, RegistreringType9)>> unitsByParent)
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

        public static fremsoegobjekthierarkiRequest CreateOrgHierarchyRequest(string uuid, int pageSize, int skip = 0)
        {
            var listRequest = new fremsoegobjekthierarkiRequest
            {
                RequestHeader = new RequestHeaderType
                {
                    TransactionUUID = Guid.NewGuid().ToString(),
                },
                FremsoegObjekthierarkiInput = new FremsoegObjekthierarkiInputType()
                {
                    MaksimalAntalKvantitet = pageSize.ToString("D"),
                    FoersteResultatReference = skip.ToString("D"),
                    /*BrugerSoegEgenskab = new EgenskabType3(),
                    InteressefaellesskabSoegEgenskab = new EgenskabType4(),
                    ItSystemSoegEgenskab = new EgenskabType5(),
                    OrganisationEnhedSoegEgenskab = new EgenskabType1(),
                    OrganisationFunktionSoegEgenskab = new EgenskabType2(),
                    OrganisationSoegEgenskab = new EgenskabType(),
                    SoegRegistrering = new SoegRegistreringType(),
                    SoegVirkning = new SoegVirkningType()*/
                }
            };
            return listRequest;
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
