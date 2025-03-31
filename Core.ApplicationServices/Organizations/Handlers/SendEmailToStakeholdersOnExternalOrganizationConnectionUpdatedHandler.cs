using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Infrastructure.Services.Configuration;
using Serilog;

namespace Core.ApplicationServices.Organizations.Handlers
{
    public class SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandler :
        IDomainEventHandler<ExternalOrganizationConnectionUpdated>,
        IDomainEventHandler<PendingExternalOrganizationUpdatesResolved>
    {
        private readonly IMailClient _mailClient;
        private readonly ILogger _logger;
        private readonly string _changeLogLink;

        public SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandler(IMailClient mailClient, ILogger logger, KitosUrl baseUrl)
        {
            _mailClient = mailClient;
            _logger = logger;
            _changeLogLink = new Uri(baseUrl.Url, "/ui/local-admin/import").AbsoluteUri;
        }

        public void Handle(ExternalOrganizationConnectionUpdated domainEvent)
        {
            var organization = domainEvent.Entity;
            try
            {
                var logEntries = domainEvent.Changes.Entries.ToList();
                var shouldHandle =
                    domainEvent.Changes.ResponsibleType == ExternalOrganizationChangeLogResponsible.Background &&
                    domainEvent.Connection.Origin == OrganizationUnitOrigin.STS_Organisation &&
                    logEntries.Any();
                if (shouldHandle)
                {
                    if (logEntries.Any())
                    {
                        var localAdmins = CollectLocalAdmins(organization);
                        if (localAdmins.Any())
                        {
                            var message = CreateMessage(localAdmins, organization);
                            _mailClient.Send(message);
                        }
                        else
                        {
                            _logger.Warning("No local admins in organization with id {uuid}, so no email can be sent as result of background update through an external connection", organization.Uuid);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed while sending background update email to local admins in {orgName} ({uuid})", organization.Name, organization.Uuid);
            }
        }

        private MailMessage CreateMessage(IEnumerable<User> receivers, Organization organization)
        {
            var mailContent = $"<p>Din organisation '{organization.Name}' har automatisk indlæst opdateringer fra FK Organisation.</p>" +
                              "<p>" +
                                $"<a href='{_changeLogLink}' target='_blank'>" +
                                    "Klik her for at se ændringsloggen" +
                                "</a>." +
                              "</p>" +
                              "<p>" +
                                "Bemærk at denne mail ikke kan besvares." +
                              "</p>";
            var mailSubject = $"{organization.Name} i KITOS har automatisk indlæst opdateringer fra FK Organisation";

            return CreateMessage(receivers, mailSubject, mailContent);
        }

        public void Handle(PendingExternalOrganizationUpdatesResolved domainEvent)
        {
            var changes = domainEvent.Changes;
            var changeDetails = changes.Entries.ToList();
            if (changeDetails.Any())
            {
                var organization = domainEvent.Organization;
                var localAdmins = CollectLocalAdmins(organization);
                if (localAdmins.Any())
                {
                    var summary = CollectChangeSummary(changeDetails);

                    var mailSubject = $"Der er '{changeDetails.Count}' ventende opdateringer fra FK Organisation til din organisation '{organization.Name}' i KITOS";
                    var mailContent = $"<p>Din organisation '{organization.Name}' er forbundet til FK Organisation, men indeholder ikke det seneste data. Der er {changeDetails.Count} ventende opdateringer.</p>" +
                                      "<p>" +
                                      string.Join(", ", summary) +
                                      "</p>" +
                                      $"<p><a href='{_changeLogLink}' target='_blank'>" +
                                      "Klik her for at gennemse (klik 'Rediger') og indlæse de ventende opdateringer fra FK Organisation" +
                                      "</a>." +
                                      "</p>" +
                                      "<p>" +
                                      "Bemærk at denne mail ikke kan besvares." +
                                      "</p>";

                    var mailMessage = CreateMessage(localAdmins, mailSubject, mailContent);
                    _mailClient.Send(mailMessage);
                }
                else
                {
                    _logger.Warning("No local admins in organization with id {uuid}, so no email can be sent to notify of pending changes", organization.Uuid);
                }
            }
        }

        private static List<User> CollectLocalAdmins(Organization organization)
        {
            return organization.GetUsersWithRole(OrganizationRole.LocalAdmin, false).ToList();
        }

        private static IEnumerable<string> CollectChangeSummary(List<ExternalConnectionAddNewLogEntryInput> changeDetails)
        {
            var changeSummaries = new List<string>();
            foreach (var detailsByType in changeDetails.GroupBy(x => x.Type).OrderBy(x=>x.Key).ToList())
            {
                var category = detailsByType.Key switch
                {
                    ConnectionUpdateOrganizationUnitChangeType.Added => "Tilføjelser",
                    ConnectionUpdateOrganizationUnitChangeType.Renamed => "Omdøbninger",
                    ConnectionUpdateOrganizationUnitChangeType.Moved => "Flytninger",
                    ConnectionUpdateOrganizationUnitChangeType.Deleted => "Sletninger",
                    ConnectionUpdateOrganizationUnitChangeType.Converted => "Konverteringer",
                    ConnectionUpdateOrganizationUnitChangeType.RootChanged => "Organisationsrod udskiftet",
                    _ => "Ukendt"
                };
                changeSummaries.Add($"{category}: {detailsByType.Count()}");
            }

            return changeSummaries;
        }

        private static MailMessage CreateMessage(IEnumerable<User> receivers, string mailSubject, string mailContent)
        {
            var message = new MailMessage
            {
                Subject = mailSubject,
                Body = mailContent,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
            };

            foreach (var receiver in receivers)
            {
                message.To.Add(receiver.Email);
            }

            return message;
        }
    }
}
