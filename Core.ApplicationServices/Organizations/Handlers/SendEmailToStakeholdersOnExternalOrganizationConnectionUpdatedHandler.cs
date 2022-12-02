using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Serilog;

namespace Core.ApplicationServices.Organizations.Handlers
{
    public class SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandler : IDomainEventHandler<ExternalOrganizationConnectionUpdated>
    {
        private readonly IMailClient _mailClient;
        private readonly ILogger _logger;

        public SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandler(IMailClient mailClient, ILogger logger)
        {
            _mailClient = mailClient;
            _logger = logger;
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
                        var localAdmins = organization.GetUsersWithRole(OrganizationRole.LocalAdmin).ToList();
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

        public MailMessage CreateMessage(IEnumerable<User> receivers, Organization organization)
        {
            var mailContent = "<p>Du har bedt om at få nulstillet dit password.</p>" +
                              "<p><a href='" + resetLink +
                              "'>Klik her for at nulstille passwordet for din KITOS profil</a>.</p>" +
                              "<p>Linket udløber om " + _ttl.TotalDays + " dage.</p>" +
                              "<p><a href='" + KitosManualsLink + "'>Klik her for at få Hjælp til log ind og brugerkonto</a></p>" +
                              "<p>Bemærk at denne mail ikke kan besvares.</p>";
            var mailSubject = $"{organization.Name} i KITOS har modtaget opdateringer fra FK Organisation";

            var message = new MailMessage
            {
                Subject = mailSubject,
                Body = content ?? mailContent,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
            };

            foreach (var receiver in receivers)
            {
                message.To.Add(receiver.Email);
            }

            _mailClient.Send(message);

            return message;
        }
    }
}
