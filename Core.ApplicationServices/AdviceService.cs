using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.AdviceSent;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Hangfire;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Core.DomainModel.GDPR;
using Core.DomainServices.Extensions;

namespace Core.ApplicationServices
{
    using DomainModel.ItSystemUsage;
    using Ninject.Extensions.Logging;

    public class AdviceService : IAdviceService
    {
        [Inject]
        public MailClient MailClient { get; set; }

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

        public AdviceService() { }

        public bool SendAdvice(int id)
        {

            var advice = AdviceRepository.AsQueryable().FirstOrDefault(a => a.Id == id);

            if (advice != null)
            {
                if (advice.Scheduling == Scheduling.Immediate)
                {
                    try
                    {
                        //Setup mail
                        MailMessage message = new MailMessage()
                        {
                            Body = advice.Body,
                            Subject = (advice.Subject).Replace('\r', ' ').Replace('\n', ' '),
                            BodyEncoding = Encoding.UTF8,
                            IsBodyHtml = true
                        };



                        //Add recivers for Email
                        foreach (var r in advice.Reciepients)
                        {
                            //add To's
                            if (r.RecieverType == RecieverType.RECIEVER && r.RecpientType == RecieverType.USER)
                            {
                                AddRecipientByName(r, message, message.To);
                            }
                            if (r.RecieverType == RecieverType.RECIEVER && r.RecpientType == RecieverType.ROLE)
                            {
                                AddRecipientByRole(advice, r, message, message.To);
                            }

                            //ADD Role receivers
                            if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.USER)
                            {
                                AddRecipientByName(r, message, message.CC);
                            }
                            if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.ROLE)
                            {
                                AddRecipientByRole(advice, r, message, message.CC);
                            }
                        }

                        //Send Mail.
                        MailClient.Send(message);
                        advice.SentDate = DateTime.Now;

                        AdviceRepository.Update(advice);
                        AdviceRepository.Save();


                        AdviceSentRepository.Insert(new AdviceSent()
                        {
                            AdviceId = id,
                            AdviceSentDate = DateTime.Now
                        });

                        AdviceSentRepository.Save();

                        return true;
                    }
                    catch (Exception e)
                    {
                        this.Logger?.Error(e, "Error in Advis service");
                        return false;
                    }
                }

                if (advice.StopDate < DateTime.Now)
                {
                    RecurringJob.RemoveIfExists(advice.JobId);
                    return false;
                }
                if (advice.AlarmDate.Value.Date <= DateTime.Now.Date)
                {
                    try
                    {
                        //Setup mail//Setup mail
                        var message = new MailMessage()
                        {
                            Body = advice.Body,
                            Subject = (advice.Subject).Replace('\r', ' ').Replace('\n', ' '),
                            BodyEncoding = Encoding.UTF8,
                            IsBodyHtml = true
                        };
                        //  message.From = new MailAddress("no_reply@kitos.dk");

                        //Add recivers for Email
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

                        //Send Mail.
                        MailClient.Send(message);
                        advice.SentDate = DateTime.Now;

                        AdviceRepository.Update(advice);
                        AdviceRepository.Save();


                        AdviceSentRepository.Insert(new AdviceSent()
                        {
                            AdviceId = id,
                            AdviceSentDate = DateTime.Now
                        });

                        AdviceSentRepository.Save();

                        return true;
                    }
                    catch (Exception e)
                    {
                        this.Logger?.Error(e, "Error in Advis service");
                        return false;
                    }
                }
            }

            return false;
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
