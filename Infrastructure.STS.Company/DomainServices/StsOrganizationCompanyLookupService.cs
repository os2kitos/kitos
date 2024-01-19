using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Core.DomainServices.Organizations;
using Core.DomainServices.SSO;
using Infrastructure.STS.Common.Factories;
using Infrastructure.STS.Common.Model;
using Infrastructure.STS.Common.Model.Client;
using Infrastructure.STS.Common.Model.Token;
using Infrastructure.STS.Company.ServiceReference;
using Serilog;
using System;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Infrastructure.STS.Company.DomainServices
{
    public class StsOrganizationCompanyLookupService : IStsOrganizationCompanyLookupService
    {
        private readonly ILogger _logger;
        private readonly string _certificateThumbprint;
        private readonly string _issuer;

        private const string EntityId = "http://stoettesystemerne.dk/service/organisation/3";

        public StsOrganizationCompanyLookupService(StsOrganisationIntegrationConfiguration configuration, ILogger logger)
        {
            _logger = logger;
            _certificateThumbprint = configuration.CertificateThumbprint;
            _issuer = configuration.Issuer;
        }

        public Result<Guid, DetailedOperationError<StsError>> ResolveStsOrganizationCompanyUuid(Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            var token = TokenFetcher.IssueToken(EntityId, organization.Cvr, _certificateThumbprint, _issuer);
            var clientCertificate = X509CertificateClientCertificateFactory.GetClientCertificate(_certificateThumbprint);

            using var virksomhedPortTypeClient = CreateClient(BasicHttpBindingFactory.CreateHttpBinding(), "https://organisation.eksterntest-stoettesystemerne.dk/organisation/virksomhed/6", clientCertificate);
            var identity = EndpointIdentity.CreateDnsIdentity("ORG_EXTTEST_Organisation_1");
            var endpointAddress = new EndpointAddress(virksomhedPortTypeClient.Endpoint.ListenUri, identity);
            virksomhedPortTypeClient.Endpoint.Address = endpointAddress;
            virksomhedPortTypeClient.Endpoint.Contract.ProtectionLevel = ProtectionLevel.None;
            var channel = virksomhedPortTypeClient.ChannelFactory.CreateChannelWithIssuedToken(token);
            var request = CreateSearchByCvrRequest(organization);
            try
            {
                var response = GetSearchResponse(channel, request);
                var statusResult = response.SoegOutput.StandardRetur;
                var stsError = statusResult.StatusKode.ParseStsErrorFromStandardResultCode();
                if (stsError.HasValue)
                {
                    return new DetailedOperationError<StsError>(OperationFailure.UnknownError, stsError.Value, $"Error resolving the organization company from STS:{statusResult.StatusKode}:{statusResult.FejlbeskedTekst}");
                }

                var ids = response.SoegOutput.IdListe;
                if (ids.Length != 1)
                {
                    return new DetailedOperationError<StsError>(OperationFailure.UnknownError, StsError.Unknown, $"Error resolving the organization company from STS. Expected a single UUID but got:{string.Join(",", ids)}");
                }

                return new Guid(ids.Single());
            }
            catch (FaultException<MessageFault> spFault)
            {
                var knownStsError = spFault.Detail.Reason.ToString().ParseStsFromErrorCode();
                var stsError = knownStsError.GetValueOrFallback(StsError.Unknown);
                var operationFailure =
                    stsError is StsError.MissingServiceAgreement or StsError.ExistingServiceAgreementIssue
                        ? OperationFailure.Forbidden
                        : OperationFailure.UnknownError;

                _logger.Error(spFault, "Service platform exception while finding company uuid from cvr {cvr} for organization with id {organizationId}", organization.Cvr, organization.Id);
                return new DetailedOperationError<StsError>(operationFailure, stsError, $"STS Organisation threw and exception while searching for uuid by cvr:{organization.Cvr} for organization with id:{organization.Id}");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unknown Exception while finding company uuid from cvr {cvr} for organization with id {organizationId}", organization.Cvr, organization.Id);
                return new DetailedOperationError<StsError>(OperationFailure.UnknownError, StsError.Unknown, $"STS Organisation threw and unknown exception while searching for uuid by cvr:{organization.Cvr} for organization with id:{organization.Id}");
            }
        }

        private static soegRequest CreateSearchByCvrRequest(Organization organization)
        {
            return new soegRequest(
                new RequestHeaderType
                {
                    TransactionUUID = Guid.NewGuid().ToString()
                },
                new SoegInputType1()
                {
                    AttributListe = new[]{
                            new EgenskabType
                            {
                                CVRNummerTekst = organization.Cvr
                            }},
                    TilstandListe = new TilstandListeType(),
                    RelationListe = new RelationListeType(),
                });
        }

        private static soegResponse GetSearchResponse(VirksomhedPortType channel, soegRequest request)
        {
            return new RetriedIntegrationRequest<soegResponse>(() => channel.soeg(request)).Execute();
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
