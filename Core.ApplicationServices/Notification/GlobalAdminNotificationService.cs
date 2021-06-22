using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.ScheduledJobs;
using Core.DomainServices;
using Serilog;

namespace Core.ApplicationServices.Notification
{
    public class GlobalAdminNotificationService : IGlobalAdminNotificationService
    {
        private readonly IMailClient _mailClient;
        private readonly IHangfireApi _hangfireApi;
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public GlobalAdminNotificationService(
            IMailClient mailClient,
            IHangfireApi hangfireApi,
            IUserRepository userRepository,
            ILogger logger)
        {
            _mailClient = mailClient;
            _hangfireApi = hangfireApi;
            _userRepository = userRepository;
            _logger = logger;
        }

        public void Submit(GlobalAdminNotification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            _hangfireApi.Schedule(() => SendEmail(notification.Message.IsHtml, notification.Subject, notification.Message.Content, notification.ResponsibleUserId, notification.OccurredAt));
        }

        //NOTE: Public allow Hangfire to invoke it
        public void SendEmail(bool messageIsHtml, string subject, string messageContent, int responsibleUserId, DateTime notificationOccurredAt)
        {
            var globalAdminEmails = GetGlobalAdminEmails();

            var mailMessage = new MailMessage
            {
                IsBodyHtml = messageIsHtml,
                BodyEncoding = Encoding.UTF8,
                Body = messageContent,
                Subject = subject,
            };

            mailMessage.Headers.Add("X-KITOS-RESPONSIBLE-USER-ID", responsibleUserId.ToString("D"));
            mailMessage.Headers.Add("X-KITOS-OCCURRED-AT", notificationOccurredAt.ToString("O"));

            globalAdminEmails.ForEach(mailMessage.To.Add);
            if (!mailMessage.To.Any())
            {
                _logger.Error("No Global admins resolved. Message {subject}:{message} sent by user with id {userId}, could not be dispatched to global admins", subject, messageContent, responsibleUserId);
            }
            _mailClient.Send(mailMessage);
        }

        private List<MailAddress> GetGlobalAdminEmails()
        {
            return _userRepository
                .GetGlobalAdmins()
                .Select(user => user.Email)
                .Distinct()
                .AsEnumerable()
                .Select(address => new MailAddress(address))
                .ToList();
        }
    }
}
