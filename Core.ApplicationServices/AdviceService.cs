using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Helpers;
using Core.ApplicationServices.Jobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;
using Hangfire;
using Infrastructure.Services.DataAccess;
using Ninject.Extensions.Logging;

namespace Core.ApplicationServices
{
    public class AdviceService : IAdviceService
    {
        #region Properties

        [Inject]
        public IMailClient MailClient { get; set; }

        [Inject]
        public IGenericRepository<User> UserRepository { get; set; }

        [Inject]
        public IGenericRepository<DomainModel.Advice.Advice> AdviceRepository { get; set; }

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
        public IAdviceScheduler AdviceScheduler { get; set; }

        [Inject]
        public ITransactionManager TransactionManager { get; set; }

        [Inject]
        public IOrganizationalUserContext OrganizationalUserContext { get; set; }

        #endregion

        public void CreateAdvice(Advice advice)
        {
            ScheduleAdvice(advice);
            AdviceRepository.Update(advice);
            AdviceRepository.Save();
        }

        public IQueryable<Advice> GetAdvicesForOrg(int orgKey)
        {
            var result = AdviceRepository.SQL($"SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItContract c on c.Id = a.RelationId Where c.OrganizationId = {orgKey} and a.Type = 0 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItProject p on p.Id = a.RelationId Where p.OrganizationId = {orgKey} and a.Type = 2 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItSystemUsage u on u.Id = a.RelationId Where u.OrganizationId = {orgKey} and a.Type = 1 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItInterface i on i.Id = a.RelationId Where i.OrganizationId = {orgKey} and a.Type = 3 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join DataProcessingRegistrations i on i.Id = a.RelationId Where i.OrganizationId = {orgKey} and a.Type = 4");
            return result.AsQueryable();
        }

        public IQueryable<Advice> GetAdvicesFromCurrentUsersOrganizationMemberships()
        {
            return OrganizationalUserContext
                .OrganizationIds
                .Select(GetAdvicesForOrg)
                .Aggregate<IQueryable<Advice>, IQueryable<Advice>>
                (
                    null,
                    (acc, next) => acc == null ? next : acc.Concat(next)
                );
        }

        public bool SendAdvice(int id)
        {
            try
            {
                var advice = AdviceRepository.AsQueryable().FirstOrDefault(a => a.Id == id);
                if (advice != null)
                {
                    if (advice.AdviceType == AdviceType.Immediate || IsAdviceInScope(advice))
                    {
                        try
                        {
                            DispatchEmails(advice);

                            AdviceRepository.Update(advice);

                            AdviceSentRepository.Insert(new AdviceSent { AdviceId = id, AdviceSentDate = DateTime.Now });
                            AdviceSentRepository.Save();
                        }
                        catch (Exception e)
                        {
                            Logger?.Error(e, "Error sending emails in advice service");
                        }
                    }

                    if (advice.AdviceType != AdviceType.Immediate || IsAdviceExpired(advice))
                    {
                        advice.IsActive = false;
                        AdviceScheduler.Remove(advice);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logger?.Error(e, "General error sending emails in advice service");
                return false;
            }
        }

        private static bool IsAdviceExpired(Advice advice)
        {
            return advice.StopDate != null && advice.StopDate.Value.Date < DateTime.Now.Date;
        }

        private static bool IsAdviceInScope(Advice advice)
        {
            return advice.AlarmDate != null && advice.AlarmDate.Value.Date <= DateTime.Now.Date && !IsAdviceExpired(advice);
        }

        private void DispatchEmails(Advice advice)
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
                    case RecieverType.RECIEVER:
                        switch (r.RecpientType)
                        {
                            case RecieverType.USER:
                                AddRecipientByName(r, message.To);
                                break;
                            case RecieverType.ROLE:
                                AddRecipientByRole(advice, r, message.To);
                                break;
                        }
                        break;
                    case RecieverType.CC:
                        switch (r.RecpientType)
                        {
                            case RecieverType.USER:
                                AddRecipientByName(r, message.CC);
                                break;
                            case RecieverType.ROLE:
                                AddRecipientByRole(advice, r, message.CC);
                                break;
                        }
                        break;
                }
            }
            MailClient.Send(message);
            advice.SentDate = DateTime.Now;
        }

        private static void AddRecipientByName(AdviceUserRelation r, MailAddressCollection mailAddressCollection)
        {
            if (!string.IsNullOrEmpty(r.Name))
            {
                mailAddressCollection.Add(r.Name);
            }
        }

        private void AddRecipientByRole(Advice advice, AdviceUserRelation r, MailAddressCollection mailAddressCollection)
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

        public void Delete(Advice advice)
        {
            RemoveAdviceAndItsRelatedEntities(advice);
            AdviceRepository.Save();
        }

        private void RemoveAdviceAndItsRelatedEntities(Advice advice)
        {
            DeleteJobFromHangfire(advice);
            AdviceRepository.DeleteByKeyWithReferencePreload(advice.Id);
        }

        private void DeleteJobFromHangfire(Advice advice)
        {
            DeletePostponedRecurringJobFromHangfire(advice.JobId);
            RecurringJob.RemoveIfExists("Advice: " + advice.Id);
        }

        private void DeletePostponedRecurringJobFromHangfire(string jobIdText)
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            var jobsScheduled = monitor.ScheduledJobs(0, int.MaxValue).
                Where(x => x.Value.Job.Method.Name == nameof(CreateDelayedRecurringJob));
            foreach (var j in jobsScheduled)
            {
                var t = j.Value.Job.Args[1].ToString(); // Pick "Advice: nn"
                if (t.Contains(jobIdText))
                {
                    BackgroundJob.Delete(j.Key);
                }
            }
        }

        public void BulkDeleteAdvice(IEnumerable<Advice> toBeDeleted)
        {
            using var transaction = TransactionManager.Begin(IsolationLevel.ReadCommitted);
            foreach (var advice in toBeDeleted)
            {
                RemoveAdviceAndItsRelatedEntities(advice);
            }
            AdviceRepository.Save();
            transaction.Commit();
        }

        public void Deactivate(Advice advice)
        {
            DeleteJobFromHangfire(advice);
            advice.IsActive = false;
            AdviceRepository.Update(advice);
            AdviceRepository.Save();
        }

        private void ScheduleAdvice(Advice advice)
        {
            if (advice.AdviceType == AdviceType.Immediate)
            {
                BackgroundJob.Enqueue(() => SendAdvice(advice.Id));
            }
            else if (advice.AdviceType == AdviceType.Repeat)
            {
                var alarmDate = advice.AlarmDate;

                if (alarmDate == null)
                    throw new ArgumentException(nameof(alarmDate) + " must be defined");

                BackgroundJob.Schedule(
                    () => CreateDelayedRecurringJob(advice.Id, advice.JobId, advice.Scheduling.Value,
                        alarmDate.Value), new DateTimeOffset(alarmDate.Value));
            }
        }

        public void RescheduleRecurringJob(Advice advice)
        {
            var alarmDate = advice.AlarmDate;

            if (alarmDate == null)
                throw new ArgumentException(nameof(alarmDate) + " must be defined");

            DeletePostponedRecurringJobFromHangfire(advice.JobId); // Remove existing hangfire job
            BackgroundJob.Schedule(() => CreateDelayedRecurringJob(advice.Id, advice.JobId, advice.Scheduling.Value, alarmDate.Value), new DateTimeOffset(alarmDate.Value));
        }

        public void CreateDelayedRecurringJob(int entityId, string name, Scheduling schedule, DateTime alarmDate)
        {
            RecurringJob.AddOrUpdate(name, () => SendAdvice(entityId), CronStringHelper.CronPerInterval(schedule, alarmDate));
        }
    }
}
