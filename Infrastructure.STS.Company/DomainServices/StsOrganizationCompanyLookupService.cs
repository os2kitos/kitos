using System;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Core.Abstractions.Types;
using Core.DomainServices.Organizations;
using Core.DomainServices.SSO;
using Infrastructure.STS.Common.Model;
using Infrastructure.STS.Common.Model.Client;
using Infrastructure.STS.Common.Model.Token;
using Kombit.InfrastructureSamples.Token;
using Kombit.InfrastructureSamples.VirksomhedService;
using Serilog;
using Organization = Core.DomainModel.Organization.Organization;

namespace Infrastructure.STS.Company.DomainServices;

public class StsOrganizationCompanyLookupService : IStsOrganizationCompanyLookupService
{
    private readonly StsOrganisationIntegrationConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly TokenFetcher _tokenFetcher;

    public StsOrganizationCompanyLookupService(StsOrganisationIntegrationConfiguration configuration,
        TokenFetcher tokenFetcher, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
        _tokenFetcher = tokenFetcher;
    }

    public Result<Guid, DetailedOperationError<StsError>> ResolveStsOrganizationCompanyUuid(Organization organization)
    {
        if (organization == null) throw new ArgumentNullException(nameof(organization));

        try
        {
            var response = GetSearchResponse(CreatePort(organization.Cvr), CreateSearchByCvrRequest(organization));
            var statusResult = response.SoegOutput.StandardRetur;
            var stsError = statusResult.StatusKode.ParseStsErrorFromStandardResultCode();
            if (stsError.HasValue)
                return new DetailedOperationError<StsError>(OperationFailure.UnknownError, stsError.Value,
                    $"Error resolving the organization company from STS:{statusResult.StatusKode}:{statusResult.FejlbeskedTekst}");

            var ids = response.SoegOutput.IdListe;
            if (ids.Length != 1)
                return new DetailedOperationError<StsError>(OperationFailure.UnknownError, StsError.Unknown,
                    $"Error resolving the organization company from STS. Expected a single UUID but got:{string.Join(",", ids)}");

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

            _logger.Error(spFault,
                "Service platform exception while finding company uuid from cvr {cvr} for organization with id {organizationId}",
                organization.Cvr, organization.Id);
            return new DetailedOperationError<StsError>(operationFailure, stsError,
                $"STS Organisation threw and exception while searching for uuid by cvr:{organization.Cvr} for organization with id:{organization.Id}");
        }
        catch (FaultException e)
        {
            if (e.Code.Name.Equals("5015"))
            {
                _logger.Error(e, "The received user context does not exist while finding company uuid from cvr {cvr} with id {organizationId}",
                    organization.Cvr, organization.Id);
                return new DetailedOperationError<StsError>(OperationFailure.Forbidden, StsError.ReceivedUserContextDoesNotExistOnSystem,
                    e.Message + $" ({organization.Cvr} for organization with id:{organization.Id})");
            }
            _logger.Error(e,
                "Unknown exception while finding company uuid from cvr {cvr} for organization with id {organizationId}",
                organization.Cvr, organization.Id);
            return new DetailedOperationError<StsError>(OperationFailure.UnknownError, StsError.Unknown,
                $"STS Organisation threw and unknown exception while searching for uuid by cvr:{organization.Cvr} for organization with id:{organization.Id}");
        }
    }

    private VirksomhedPortType CreatePort(string cvr)
    {
        var token = _tokenFetcher.IssueToken(_configuration.OrgService6EntityId, cvr);
        var client = new VirksomhedPortTypeClient();

        var identity = EndpointIdentity.CreateDnsIdentity(_configuration.ServiceCertificateAliasOrg);
        var endpointAddress = new EndpointAddress(client.Endpoint.ListenUri, identity);
        client.Endpoint.Address = endpointAddress;
        var certificate = CertificateLoader.LoadCertificate(
            StoreName.My,
            StoreLocation.LocalMachine,
            _configuration.ClientCertificateThumbprint
        );
        client.ClientCredentials.ClientCertificate.Certificate = certificate;
        client.Endpoint.Contract.ProtectionLevel = ProtectionLevel.None;

        return client.ChannelFactory.CreateChannelWithIssuedToken(token);
    }

    private static soegRequest CreateSearchByCvrRequest(Organization organization)
    {
        return new soegRequest(
            new RequestHeaderType
            {
                TransactionUUID = Guid.NewGuid().ToString()
            },
            new SoegInputType1
            {
                AttributListe = new[]
                {
                    new EgenskabType
                    {
                        CVRNummerTekst = organization.Cvr
                    }
                },
                TilstandListe = new TilstandListeType(),
                RelationListe = new RelationListeType()
            });
    }

    private static soegResponse GetSearchResponse(VirksomhedPortType channel, soegRequest request)
    {
        return new RetriedIntegrationRequest<soegResponse>(() => channel.soeg(request)).Execute();
    }
}