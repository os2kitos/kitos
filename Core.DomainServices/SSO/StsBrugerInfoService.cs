using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Infrastructure.STS.Common.Model;
using Kombit.InfrastructureSamples.BrugerService;
using Kombit.InfrastructureSamples.Token;
using Serilog;
using RegistreringType10 = Kombit.InfrastructureSamples.PersonService.RegistreringType10;
using RegistreringType8 = Kombit.InfrastructureSamples.AdresseService.RegistreringType8;

namespace Core.DomainServices.SSO
{
    public class StsBrugerInfoService : IStsBrugerInfoService
    {
        private readonly ILogger _logger;
        private const string EmailTypeIdentifier = StsOrganisationConstants.UserProperties.Email;
        private readonly StsOrganisationIntegrationConfiguration _configuration;
        private readonly TokenFetcher _tokenFetcher;

        public StsBrugerInfoService(StsOrganisationIntegrationConfiguration configuration, ILogger logger, TokenFetcher tokenFetcher)
        {
            _configuration = configuration;
            _logger = logger;
            _tokenFetcher = tokenFetcher;
        }

        public Result<StsBrugerInfo, string> GetStsBrugerInfo(Guid uuid, string cvrNumber)
        {
            var brugerInfo = CollectStsBrugerInformationFromUuid(uuid, cvrNumber);
            if (brugerInfo.Failed)
            {
                _logger.Error("Failed to resolve user UUID '{error}'", brugerInfo.Error);
                return brugerInfo.Error;
            }

            var (emailAdresseUuid, organisationUuid, personUuid) = brugerInfo.Value;
            var emailsResult = GetStsAdresseEmailFromUuid(emailAdresseUuid, cvrNumber);
            if (emailsResult.Failed)
            {
                _logger.Error("Failed to resolve Email '{error}'", emailsResult.Error);
                return brugerInfo.Error;
            }

            var personData = GetStsPersonFromUuid(personUuid, cvrNumber);
            if (personData.Failed)
            {
                _logger.Error("Failed to resolve Person '{error}'", personData.Error);
                return brugerInfo.Error;
            }

            return new StsBrugerInfo(
                uuid,
                emailsResult.Value,
                Guid.Parse(organisationUuid),
            cvrNumber,
            personData.Value.FirstName,
                personData.Value.LastName);
        }

        private Result<(string emailAdresseUuid, string organisationUuid, string personUuid), string> CollectStsBrugerInformationFromUuid(Guid uuid, string cvrNumber)
        {
            var port = PortFactory.CreateBrugerPort(_tokenFetcher, _configuration, cvrNumber);
            if (port.Failed) return port.Error;
           
            var laesRequest = StsBrugerHelpers.CreateStsBrugerLaesRequest(uuid);
            var laesResponseResult = port.Value.laes(laesRequest);

            if (laesResponseResult == null)
                return $"Failed to fetch data from STS Bruger from uuid {uuid}";

            var stdOutput = laesResponseResult.LaesOutput?.StandardRetur;
            var returnCode = stdOutput?.StatusKode ?? "unknown";
            var errorCode = stdOutput?.FejlbeskedTekst ?? string.Empty;
            var stsError = stdOutput?.StatusKode.ParseStsErrorFromStandardResultCode() ?? Maybe<StsError>.None;
            if (stsError.Select(error => error == StsError.NotFound).GetValueOrDefault())
                return $"Requested user '{uuid}' from cvr '{cvrNumber}' was not found. STS Bruger endpoint returned '{returnCode}:{errorCode}'";

            var registrations =
                laesResponseResult
                    .LaesOutput
                    ?.FiltreretOejebliksbillede
                    ?.Registrering
                    ?.ToList() ?? new List<RegistreringType5>();

            if (registrations.Any() == false)
                return $"No STS Bruger registrations from UUID {uuid}";

            foreach (var registreringType5 in registrations)
            {
                if (registreringType5.IsStsBrugerObsolete())
                {
                    //User info obsolete, go to next registration
                    continue;
                }

                var organizationUuid =
                    registreringType5
                        .RelationListe
                        ?.Tilhoerer
                        ?.ReferenceID
                        ?.Item;

                if (organizationUuid == null)
                {
                    //No organization uuid
                    continue;
                }
                string emailUuid = null;
                var adresses = registreringType5
                    .RelationListe
                    ?.Adresser
                    ?.ToList() ?? new List<AdresseFlerRelationType>();

                foreach (var adresse in adresses)
                {
                    var emailField = adresse.Rolle?.Item ?? string.Empty;

                    if (EmailTypeIdentifier.Equals(emailField))
                    {
                        emailUuid = adresse.ReferenceID?.Item;

                        if (emailUuid == null)
                        {
                            //No uuid provided
                            continue;
                        }
                        break;
                    }
                }

                if (emailUuid == null)
                {
                    //Email UUID could not be found - go to the next
                    continue;
                }

                var personUuid = registreringType5
                    .RelationListe
                    ?.TilknyttedePersoner
                    ?.OrderByDescending(p => p.Virkning.TilTidspunkt.Item is true ? DateTime.MaxValue : p.Virkning.TilTidspunkt.Item)
                    ?.FirstOrDefault()
                    ?.ReferenceID
                    ?.Item;


                if (personUuid == null)
                {
                    //Person UUID could not be found, bail out
                    continue;
                }

                return (emailUuid, organizationUuid, personUuid);
            }
            return $"Unable to resolve email and organization UUID from uuid {uuid}";
        }

        private Result<IEnumerable<string>, string> GetStsAdresseEmailFromUuid(string emailAdresseUuid, string cvrNumber)
        {
            var port = PortFactory.CreateAdressePort(_tokenFetcher, _configuration, cvrNumber);
            if (port.Failed) return port.Error;

            var laesRequest = StsAdresseHelpers.CreateStsAdresseLaesRequest(emailAdresseUuid);
            var laesResponse = port.Value.laes(laesRequest);

            if (laesResponse == null)
                return $"Failed to read STS Adresse from emailAdresseUUID {emailAdresseUuid}";

            var stdOutput = laesResponse.LaesOutput?.StandardRetur;
            var returnCode = stdOutput?.StatusKode ?? "unknown";
            var errorCode = stdOutput?.FejlbeskedTekst ?? string.Empty;
            var stsError = stdOutput?.StatusKode.ParseStsErrorFromStandardResultCode() ?? Maybe<StsError>.None;
            if (stsError.Select(error => error == StsError.NotFound).GetValueOrDefault())
                return $"Requested email address '{emailAdresseUuid}' from cvr '{cvrNumber}' was not found. STS Adresse endpoint returned '{returnCode}:{errorCode}'";

            var registreringType8 =
                laesResponse
                    .LaesOutput
                    ?.FiltreretOejebliksbillede
                    ?.Registrering ?? new RegistreringType8[0];

            if (registreringType8.Any() == false)
                return $"No registrations from laesResponse for emailAdresseUuid:{emailAdresseUuid}";

            var result = new List<string>();
            foreach (var registreringType in registreringType8)
            {
                if (registreringType.IsStsAdresseObsolete())
                    continue;

                var latest = registreringType
                    .AttributListe
                    ?.OrderByDescending(y => y.Virkning.TilTidspunkt)
                    ?.FirstOrDefault()
                    ?.AdresseTekst;

                if (latest == null)
                {
                    //Failed to parse latest adresse
                    continue;
                }
                result.Add(latest);
            }

            if (result.Any())
                return result;

            return $"No email addresses found from emailAdresseUuid {emailAdresseUuid}";
        }

        private Result<StsPersonData, string> GetStsPersonFromUuid(string personUuid, string cvrNumber)
        {
            var port = PortFactory.CreatePersonPort(_tokenFetcher, _configuration, cvrNumber);
            if (port.Failed) return port.Error;

            var laesRequest = StsPersonHelpers.CreateStsPersonLaesRequest(personUuid);
            var laesResponse = port.Value.laes(laesRequest);

            if (laesResponse == null)
                return $"Failed to read from STS Person with personUUID:{personUuid}";

            var stdOutput = laesResponse.LaesOutput?.StandardRetur;
            var returnCode = stdOutput?.StatusKode ?? "unknown";
            var errorCode = stdOutput?.FejlbeskedTekst ?? string.Empty;

            var stsError = stdOutput?.StatusKode.ParseStsErrorFromStandardResultCode() ?? Maybe<StsError>.None;
            if (stsError.Select(error => error == StsError.NotFound).GetValueOrDefault())
                return $"Requested person '{personUuid}' from cvr '{cvrNumber}' was not found. STS Person endpoint returned '{returnCode}:{errorCode}'";

            var registreringType1s =
                laesResponse
                    .LaesOutput
                    ?.FiltreretOejebliksbillede
                    ?.Registrering ?? new RegistreringType10[0];

            if (registreringType1s.Any() == false)
            {
                return $"Failed to parse registrations from STS Person with personUUID:{personUuid}";
            }

            foreach (var registreringType1 in registreringType1s)
            {
                if (registreringType1.IsStsPersonObsolete())
                {
                    continue;
                }

                var latest = registreringType1
                    .AttributListe
                    ?.OrderByDescending(y => y.Virkning.TilTidspunkt)
                    ?.FirstOrDefault()
                    ?.NavnTekst;

                if (latest == null)
                    continue;

                return Result<StsPersonData, string>.Success(new StsPersonData(latest));
            }
            return Result<StsPersonData, string>.Failure($"Unable to resolve person from personuuid:{personUuid}");
        }
    }
}
