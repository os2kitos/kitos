using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.AdviceSent;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Ninject;
using System;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Core.ApplicationServices.Helpers;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;
using Ninject.Extensions.Logging;

namespace Core.ApplicationServices
{
    public class AdviceService : IAdviceService
    {
        [Inject]
        public IMailClient MailClient { get; set; }

        [Inject]
        public IGenericRepository<User> UserRepository { get; set; }

        [Inject]
        public IGenericRepository<Advice> AdviceRepository { get; set; }

        [Inject]
        public IGenericRepository<AdviceSent> AdviceSentRepository { get; set; }

        [Inject]
        public IGenericRepository<ItContractRight> ItContractRights { get; set; }

        [Inject]
        public IGenericRepository<ItProjectRight> ItprojectRights { get; set; }

        [Inject]
        public IGenericRepository<ItSystemRight> ItSystemRights { get; set; }

        [Inject]
        public IGenericRepository<DataProcessingRegistrationRight> DataProcessingRegistrationRights { get; set; }

        [Inject]
        public ILogger Logger { get; set; }

        [Inject]
        public IGenericRepository<ItContract> ItContractRepository { get; set; }

        [Inject]
        public IGenericRepository<ItInterface> ItInterfaceRepository { get; set; }

        [Inject]
        public IGenericRepository<ItProject> ItProjectRepository { get; set; }

        [Inject]
        public IGenericRepository<ItSystemUsage> ItSystemUsageRepository { get; set; }

        [Inject]
        public IGenericRepository<DataProcessingRegistration> DataProcessingRegistrations { get; set; }

        [Inject]
        public IHangfireHelper HangfireHelper { get; set; }

        public bool SendAdvice(int id)
        {
            try
            {
                var advice = AdviceRepository.AsQueryable().FirstOrDefault(a => a.Id == id);
                if (advice != null)
                {
                    if (advice.Scheduling == Scheduling.Immediate || 
                        advice.AlarmDate != null && advice.AlarmDate.Value.Date <= DateTime.Now.Date)
                    {
                        return DispatchEmails(id, advice);
                    }

                    if (advice.StopDate < DateTime.Now)
                    {
                        HangfireHelper.RemoveFromHangfire(advice);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Logger?.Error(e, "Error sending emails in advice service");
                return false;
            }
        }

        private bool DispatchEmails(int id, Advice advice)
        {
            var message = new MailMessage
            {
                Body = advice.Body,
                Subject = (advice.Subject).Replace('\r', ' ').Replace('\n', ' '),
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            foreach (var r in advice.Reciepients)
            {
                switch (r.RecieverType)
                {
                    case RecieverType.RECIEVER when r.RecpientType == RecieverType.USER:
                        AddRecipientByName(r, message, message.To);
                        break;
                    case RecieverType.CC when r.RecpientType == RecieverType.USER:
                        AddRecipientByName(r, message, message.CC);
                        break;
                }

                switch (r.RecieverType)
                {
                    case RecieverType.RECIEVER when r.RecpientType == RecieverType.ROLE:
                        AddRecipientByRole(advice, r, message, message.To);
                        break;
                    case RecieverType.CC when r.RecpientType == RecieverType.ROLE:
                        AddRecipientByRole(advice, r, message, message.CC);
                        break;
                }
            }

            MailClient.Send(message);
            advice.SentDate = DateTime.Now;
            
            AdviceRepository.Update(advice);
            AdviceRepository.Save();

            AdviceSentRepository.Insert(new AdviceSent
            {
                AdviceId = id,
                AdviceSentDate = DateTime.Now
            });
            AdviceSentRepository.Save();

            return true;
        }

        public IQueryable<Advice> GetAdvicesForOrg(int orgKey)
        {
            var result = AdviceRepository.SQL($"SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItContract c on c.Id = a.RelationId Where c.OrganizationId = {orgKey} and a.Type = 0 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItProject p on p.Id = a.RelationId Where p.OrganizationId = {orgKey} and a.Type = 2 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItSystemUsage u on u.Id = a.RelationId Where u.OrganizationId = {orgKey} and a.Type = 1 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItInterface i on i.Id = a.RelationId Where i.OrganizationId = {orgKey} and a.Type = 3 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join DataProcessingRegistrations i on i.Id = a.RelationId Where i.OrganizationId = {orgKey} and a.Type = 4");
            return result.AsQueryable();
        }

        private static void AddRecipientByName(AdviceUserRelation r, MailMessage message, MailAddressCollection mailAddressCollection)
        {
            if (!string.IsNullOrEmpty(r.Name))
            {
                mailAddressCollection.Add(r.Name);
            }
        }

        private void AddRecipientByRole(Advice advice, AdviceUserRelation r, MailMessage message, MailAddressCollection mailAddressCollection)
        {
            switch (advice.Type)
            {
                case ObjectType.itContract:

                    var itContractRoles = ItContractRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                        && I.Role.Name == r.Name);
                    foreach (var t in itContractRoles)
                    {
                        mailAddressCollection.Add(t.User.Email);
                    }

                    break;
                case ObjectType.itProject:
                    var projectRoles = ItprojectRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                        && I.Role.Name == r.Name);
                    foreach (var t in projectRoles)
                    {
                        mailAddressCollection.Add(t.User.Email);
                    }

                    break;
                case ObjectType.itSystemUsage:

                    var systemRoles = ItSystemRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                                                              && I.Role.Name == r.Name);
                    foreach (var t in systemRoles)
                    {
                        mailAddressCollection.Add(t.User.Email);
                    }

                    break;
                case ObjectType.dataProcessingRegistration:

                    var dpaRoles = DataProcessingRegistrationRights.AsQueryable().Where(I =>
                        I.ObjectId == advice.RelationId
                        && I.Role.Name == r.Name);
                    foreach (var t in dpaRoles)
                    {
                        mailAddressCollection.Add(t.User.Email);
                    }

                    break;
            }
        }
    }
}
