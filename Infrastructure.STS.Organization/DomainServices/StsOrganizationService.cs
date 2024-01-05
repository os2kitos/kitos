using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Core.Abstractions.Types;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model.StsOrganization;
using Core.DomainServices.Organizations;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.SSO;
using Infrastructure.STS.Common.Factories;
using Infrastructure.STS.Common.Model;
using Infrastructure.STS.Common.Model.Client;
using Infrastructure.STS.Organization.ServiceReference;
using Serilog;

namespace Infrastructure.STS.Organization.DomainServices
{
    public class StsOrganizationService : IStsOrganizationService
    {
        private readonly IStsOrganizationCompanyLookupService _companyLookupService;
        private readonly IStsOrganizationIdentityRepository _stsOrganizationIdentityRepository;
        private readonly ILogger _logger;
        private readonly string _certificateThumbprint;
        private readonly string _serviceRoot;

        public StsOrganizationService(
            StsOrganisationIntegrationConfiguration configuration,
            IStsOrganizationCompanyLookupService companyLookupService,
            IStsOrganizationIdentityRepository stsOrganizationIdentityRepository,
            ILogger logger)
        {
            _companyLookupService = companyLookupService;
            _stsOrganizationIdentityRepository = stsOrganizationIdentityRepository;
            _logger = logger;
            _certificateThumbprint = configuration.CertificateThumbprint;
            _serviceRoot = $"https://{configuration.EndpointHost}/service/Organisation/Organisation/5";
        }

        public Maybe<DetailedOperationError<CheckConnectionError>> ValidateConnection(Core.DomainModel.Organization.Organization organization)
        {
            return ResolveExternalUuid(organization)
                .Match(_ => Maybe<DetailedOperationError<CheckConnectionError>>.None, error =>
                {
                    var connectionError = error.Detail switch
                    {
                        ResolveOrganizationUuidError.InvalidCvrOnOrganization => CheckConnectionError.InvalidCvrOnOrganization,
                        ResolveOrganizationUuidError.MissingServiceAgreement => CheckConnectionError.MissingServiceAgreement,
                        ResolveOrganizationUuidError.ExistingServiceAgreementIssue => CheckConnectionError.ExistingServiceAgreementIssue,
                        _ => CheckConnectionError.Unknown
                    };
                    return new DetailedOperationError<CheckConnectionError>(error.FailureType, connectionError, error.Message.GetValueOrFallback(string.Empty));
                });
        }

        public Result<Guid, DetailedOperationError<ResolveOrganizationUuidError>> ResolveStsOrganizationUuid(Core.DomainModel.Organization.Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            //If an FK identity already exists, reuse it
            var fkOrgIdentity = organization.StsOrganizationIdentities.FirstOrDefault();
            if (fkOrgIdentity != null)
            {
                return fkOrgIdentity.ExternalUuid;
            }

            var companyUuid = ResolveExternalUuid(organization);

            if (companyUuid.Failed)
                return companyUuid.Error;

            //Search for the organization based on the resolved company (all organizations are tied to a company)
            using var clientCertificate = X509CertificateClientCertificateFactory.GetClientCertificate(_certificateThumbprint);
            using var organizationPortTypeClient = CreateOrganizationPortTypeClient(BasicHttpBindingFactory.CreateHttpBinding(), _serviceRoot, clientCertificate);

            var searchRequest = CreateSearchForOrganizationRequest(organization, companyUuid.Value);
            var channel = organizationPortTypeClient.ChannelFactory.CreateChannel();
            var response = GetSearchResponse(channel, searchRequest);
            var statusResult = response.SoegOutput.StandardRetur;
            var stsError = statusResult.StatusKode.ParseStsErrorFromStandardResultCode();
            if (stsError.HasValue)
            {
                _logger.Error("Failed to search for organization ({id}) by company uuid {uuid}. Failed with {stsError} {code} and {message}", organization.Id, companyUuid.Value, stsError.Value, statusResult.StatusKode, statusResult.FejlbeskedTekst);
                return new DetailedOperationError<ResolveOrganizationUuidError>(OperationFailure.UnknownError, ResolveOrganizationUuidError.FailedToSearchForOrganizationByCompanyUuid);
            }

            var ids = response.SoegOutput.IdListe;
            if (ids.Length != 1)
            {
                _logger.Error("Failed to search for organization ({id}) by company uuid {uuid}. Expected 1 result but got {resultsCsv}", organization.Id, companyUuid.Value, string.Join(",", ids));
                return new DetailedOperationError<ResolveOrganizationUuidError>(OperationFailure.UnknownError, ResolveOrganizationUuidError.DuplicateOrganizationResults);
            }

            var uuid = new Guid(ids.Single());

            var result = _stsOrganizationIdentityRepository.AddNew(organization, uuid);
            if (result.Failed)
            {
                _logger.Error("Failed save uuid for organization ({id}) with uuid {uuid}. Repository responded with {error}", organization.Id, companyUuid.Value, result.Error.ToString());
                return new DetailedOperationError<ResolveOrganizationUuidError>(OperationFailure.UnknownError, ResolveOrganizationUuidError.FailedToSaveUuidOnKitosOrganization);
            }

            return uuid;
        }

        public Result<Guid, OperationError> ResolveOrganizationHierarchyRootUuid(Core.DomainModel.Organization.Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            var organizationUuidResult = ResolveStsOrganizationUuid(organization);
            if (organizationUuidResult.Failed)
            {
                var error = organizationUuidResult.Error;
                _logger.Error("Failed to resilve uuid while looking up hierarchy root for org {ordId}. Failed with error: {code}:{message}", organization.Id, error.Detail, error.Message.GetValueOrFallback(""));
                return error;
            }

            var uuid = organizationUuidResult.Value;
            using var clientCertificate = X509CertificateClientCertificateFactory.GetClientCertificate(_certificateThumbprint);
            using var organizationPortTypeClient = CreateOrganizationPortTypeClient(BasicHttpBindingFactory.CreateHttpBinding(), _serviceRoot, clientCertificate);

            var readRequest = CreateGetOrganizationByUuidRequest(uuid);
            var channel = organizationPortTypeClient.ChannelFactory.CreateChannel();

            var response = GetReadResponse(channel, readRequest);
            var statusResult = response.LaesOutput.StandardRetur;
            var stsError = statusResult.StatusKode.ParseStsErrorFromStandardResultCode();
            if (stsError.HasValue)
            {
                _logger.Error("Failed to read organization ({id}) by uuid {uuid}. Failed with {stsError} {code} and {message}", organization.Id, uuid, stsError.Value, statusResult.StatusKode, statusResult.FejlbeskedTekst);
                return new OperationError("Failed to resolve organization by uuid", OperationFailure.UnknownError);
            }

            var orgResult = response.LaesOutput.FiltreretOejebliksbillede.Registrering.FirstOrDefault();
            if (orgResult == null)
            {
                _logger.Error("Success reading organization ({id}) by uuid {uuid}. But no data was returned", organization.Id, uuid);
                return new OperationError("FK Organisation did not return the organization by uuid", OperationFailure.UnknownError);
            }

            var rootId = orgResult.RelationListe.Overordnet?.ReferenceID?.Item;
            if (rootId == null || !Guid.TryParse(rootId, out var rootIdAsUuid))
            {
                _logger.Error("Failed to read main root from organization ({id}) by uuid {uuid}. Root unit id was provided as {rootid}", organization.Id, uuid, rootId);
                return new OperationError("FK Organisation root was not valid", OperationFailure.UnknownError);
            }

            return rootIdAsUuid;
        }

        private static laesRequest CreateGetOrganizationByUuidRequest(Guid uuid)
        {
            return new laesRequest()
            {
                LaesInput = new LaesInputType()
                {
                    UUIDIdentifikator = uuid.ToString("D")
                }
            };
        }

        private static soegResponse GetSearchResponse(OrganisationPortType channel, soegRequest searchRequest)
        {
            return new RetriedIntegrationRequest<soegResponse>(() => channel.soeg(searchRequest)).Execute();
        }

        private static laesResponse GetReadResponse(OrganisationPortType channel, laesRequest readRequest)
        {
            return new RetriedIntegrationRequest<laesResponse>(() => channel.laes(readRequest)).Execute();
        }

        private Result<Guid, DetailedOperationError<ResolveOrganizationUuidError>> ResolveExternalUuid(Core.DomainModel.Organization.Organization organization)
        {
            if (string.IsNullOrWhiteSpace(organization.Cvr) || organization.IsCvrInvalid())
            {
                return new DetailedOperationError<ResolveOrganizationUuidError>(OperationFailure.BadState, ResolveOrganizationUuidError.InvalidCvrOnOrganization);
            }

            //Resolve the associated company uuid
            var companyUuid = _companyLookupService.ResolveStsOrganizationCompanyUuid(organization);
            if (companyUuid.Failed)
            {
                _logger.Error("Error {error} while resolving company uuid for organization with id {id}",
                    companyUuid.Error.ToString(), organization.Id);

                var detailedError = companyUuid.Error.Detail switch
                {
                    StsError.MissingServiceAgreement => ResolveOrganizationUuidError.MissingServiceAgreement,
                    StsError.ExistingServiceAgreementIssue => ResolveOrganizationUuidError.ExistingServiceAgreementIssue,
                    _ => ResolveOrganizationUuidError.FailedToLookupOrganizationCompany
                };

                var operationFailure = companyUuid.Error.Detail switch
                {
                    StsError.MissingServiceAgreement => companyUuid.Error.FailureType,
                    StsError.ExistingServiceAgreementIssue => companyUuid.Error.FailureType,
                    _ => OperationFailure.UnknownError
                };

                return new DetailedOperationError<ResolveOrganizationUuidError>(operationFailure, detailedError);
            }

            return companyUuid.Value;
        }

        private static soegRequest CreateSearchForOrganizationRequest(Core.DomainModel.Organization.Organization organization, Guid companyUuid)
        {
            return new soegRequest
            {
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