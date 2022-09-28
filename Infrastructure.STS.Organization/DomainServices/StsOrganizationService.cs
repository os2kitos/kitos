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
                return companyUuid.Value;

            //Search for the organization based on the resolved company (all organizations are tied to a company)
            using var clientCertificate = X509CertificateClientCertificateFactory.GetClientCertificate(_certificateThumbprint);
            using var organizationPortTypeClient = CreateOrganizationPortTypeClient(BasicHttpBindingFactory.CreateHttpBinding(), _serviceRoot, clientCertificate);

            var searchRequest = CreateSearchForOrganizationRequest(organization, companyUuid.Value);
            var channel = organizationPortTypeClient.ChannelFactory.CreateChannel();
            var response = channel.soeg(searchRequest);
            var statusResult = response.SoegResponse1.SoegOutput.StandardRetur;
            var stsError = statusResult.StatusKode.ParseStsErrorFromStandardResultCode();
            if (stsError.HasValue)
            {
                _logger.Error("Failed to search for organization ({id}) by company uuid {uuid}. Failed with {stsError} {code} and {message}", organization.Id, companyUuid.Value, stsError.Value, statusResult.StatusKode, statusResult.FejlbeskedTekst);
                return new DetailedOperationError<ResolveOrganizationUuidError>(OperationFailure.UnknownError, ResolveOrganizationUuidError.FailedToSearchForOrganizationByCompanyUuid);
            }

            var ids = response.SoegResponse1.SoegOutput.IdListe;
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

        private Result<Guid, DetailedOperationError<ResolveOrganizationUuidError>> ResolveExternalUuid(Core.DomainModel.Organization.Organization organization)
        {
            if (organization.IsCvrInvalid())
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
