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
    public class SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandler : IDomainEventHandler<ExternalOrganizationConnectionUpdated>
    {
        private readonly IMailClient _mailClient;
        private readonly ILogger _logger;
        private readonly string _changeLogLink;

        public SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandler(IMailClient mailClient, ILogger logger, KitosUrl baseUrl)
        {
            _mailClient = mailClient;
            _logger = logger;
            _changeLogLink = new Uri(baseUrl.Url, "/#/local-config/import/organization").AbsoluteUri;
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
