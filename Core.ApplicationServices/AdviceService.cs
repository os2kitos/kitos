using Core.DomainModel.Advice;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Helpers;
using Core.ApplicationServices.ScheduledJobs;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Extensions;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Core.DomainModel.Shared;
using Core.DomainServices.Notifications;
using Core.DomainModel.Notification;
using Core.DomainServices.Advice;
using Serilog;

namespace Core.ApplicationServices
{
    public class AdviceService : IAdviceService
    {
        private readonly IMailClient _mailClient;
        private readonly IGenericRepository<DataProcessingRegistration> _dataProcessingRegistrationRepository;
        private readonly IGenericRepository<ItContract> _contractRepository;
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IGenericRepository<AdviceSent> _adviceSentRepository;
        private readonly IGenericRepository<ItContractRight> _itContractRights;
        private readonly IGenericRepository<ItSystemRight> _itSystemRights;
        private readonly IGenericRepository<DataProcessingRegistrationRight> _dataProcessingRegistrationRights;
        private readonly ILogger _logger;
        private readonly ITransactionManager _transactionManager;
        private readonly IOrganizationalUserContext _organizationalUserContext;
        private readonly IHangfireApi _hangfireApi;
        private readonly IOperationClock _operationClock;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IAdviceRootResolution _adviceRootResolution;

        public AdviceService(
            IMailClient mailClient,
            IGenericRepository<Advice> adviceRepository,
            IGenericRepository<AdviceSent> adviceSentRepository,
            IGenericRepository<ItContractRight> itContractRights,
            IGenericRepository<ItSystemRight> itSystemRights,
            IGenericRepository<DataProcessingRegistrationRight> dataProcessingRegistrationRights,
            ILogger logger,
            ITransactionManager transactionManager,
            IOrganizationalUserContext organizationalUserContext,
            IHangfireApi hangfireApi,
            IOperationClock operationClock,
            IUserNotificationService userNotificationService,
            IAdviceRootResolution adviceRootResolution,
            IGenericRepository<DataProcessingRegistration> dataProcessingRegistrationRepository,
            IGenericRepository<ItContract> contractRepository,
            IGenericRepository<ItSystemUsage> systemUsageRepository)
        {
            _mailClient = mailClient;
            _adviceRepository = adviceRepository;
            _adviceSentRepository = adviceSentRepository;
            _itContractRights = itContractRights;
            _itSystemRights = itSystemRights;
            _dataProcessingRegistrationRights = dataProcessingRegistrationRights;
            _logger = logger;
            _transactionManager = transactionManager;
            _organizationalUserContext = organizationalUserContext;
            _hangfireApi = hangfireApi;
            _operationClock = operationClock;
            _userNotificationService = userNotificationService;
            _adviceRootResolution = adviceRootResolution;
            _dataProcessingRegistrationRepository = dataProcessingRegistrationRepository;
            _contractRepository = contractRepository;
            _systemUsageRepository = systemUsageRepository;
        }

        public void CreateAdvice(Advice advice)
        {
            ScheduleAdvice(advice);
            _adviceRepository.Update(advice);
            _adviceRepository.Save();
        }

        public IQueryable<Advice> GetAdvicesForOrg(int orgKey)
        {
            var dprAdvices = GetAdvices(orgKey, RelatedEntityType.dataProcessingRegistration, _dataProcessingRegistrationRepository);
            var contractAdvices = GetAdvices(orgKey, RelatedEntityType.itContract, _contractRepository);
            var systemAdvices = GetAdvices(orgKey, RelatedEntityType.itSystemUsage, _systemUsageRepository);

            return dprAdvices
                .Union(contractAdvices)
                .Union(systemAdvices);
        }

        private IQueryable<Advice> GetAdvices<T>(int orgKey, RelatedEntityType relatedEntityType, IGenericRepository<T> _entityRepository) where T : class, IHasId, IOwnedByOrganization
        {
            return _adviceRepository
                .AsQueryable()
                .Where(x => x.Type == relatedEntityType)
                .Join(_entityRepository.AsQueryable().ByOrganizationId(orgKey),
                    advice => advice.RelationId,
                    entity => entity.Id,
                    (advice, _) => advice
                );
        }

        public IQueryable<Advice> GetAdvicesAccessibleToCurrentUser()
        {
            return _organizationalUserContext.IsGlobalAdmin()
                ? _adviceRepository.AsQueryable()
                : _organizationalUserContext
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
            using var transaction = _transactionManager.Begin();
            try
            {
                var advice = _adviceRepository.AsQueryable().ById(id);
                if (advice != null)
                {
                    if (IsDeactivated(advice))
                    {
                        _logger.Warning("SendAdvice has been invoked for deactivated Advice with id: {adviceId}. The hangfire jobs should have been deleted during deactivation. Check the logs.", id);
                        DeleteJobFromHangfire(advice);
                    }
                    else
                    {
                        if (advice.AdviceType == AdviceType.Immediate || IsAdviceInScope(advice))
                        {
                            if (DispatchEmails(advice))
                            {
                                _adviceRepository.Update(advice);

                                _adviceSentRepository.Insert(new AdviceSent { AdviceId = id, AdviceSentDate = _operationClock.Now });
                            }
                        }

                        if (advice.AdviceType == AdviceType.Immediate)
                        {
                            advice.IsActive = false;
                        }
                        else if (IsAdviceExpired(advice))
                        {
                            advice.IsActive = false;
                            DeleteJobFromHangfire(advice);
                        }

                        _adviceRepository.Save();
                        _adviceSentRepository.Save();
                        transaction.Commit();
                    }
                }
                else
                {
                    _logger.Warning("Unable to locate advice with id {id}. Removing it from HangFire", id);
                    DeleteJobFromHangfire(id);
                }
                return true;
            }
            catch (Exception e)
            {
                _logger?.Error(e, "General error sending emails in advice service");
                transaction.Rollback();
                throw;
            }
        }

        private static bool IsDeactivated(Advice advice)
        {
            return !advice.IsActive;
        }

        private bool IsAdviceExpired(Advice advice)
        {
            return advice.StopDate != null && advice.StopDate.Value.Date < _operationClock.Now.Date;
        }

        private bool IsAdviceInScope(Advice advice)
        {
            return advice.AlarmDate != null && advice.AlarmDate.Value.Date <= _operationClock.Now.Date && !IsAdviceExpired(advice);
        }

        private bool DispatchEmails(Advice advice)
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
                            case RecipientType.USER:
                                AddRecipientByName(r, message.To);
                                break;
                            case RecipientType.ROLE:
                                AddRecipientByRole(advice, r, message.To);
                                break;
                        }
                        break;
                    case RecieverType.CC:
                        switch (r.RecpientType)
                        {
                            case RecipientType.USER:
                                AddRecipientByName(r, message.CC);
                                break;
                            case RecipientType.ROLE:
                                AddRecipientByRole(advice, r, message.CC);
                                break;
                        }
                        break;
                }
            }

            if (message.To.Any() || message.CC.Any())
            {
                _mailClient.Send(message);
                advice.SentDate = _operationClock.Now;
                return true;
            }
            else
            {
                var organizationIdOfRelatedEntityId = GetRelatedEntityOrganizationId(advice);
                if (organizationIdOfRelatedEntityId.IsNone)
                {
                    _logger?.Error($"Advis doesn't have valid/correct related entity (RelationId and Type mismatch). Advice Id: {advice.Id}, Advice RelationId: {advice.RelationId}, Advice RelatedEntityType: {advice.Type}");
                }
                else
                {
                    if (advice.HasInvalidState())
                    {
                        _logger?.Error($"Advis is missing critical function information. Advice Id: {advice.Id}, Advice RelationId: {advice.RelationId}, Advice RelatedEntityType: {advice.Type}, Advice ownerId: {advice.ObjectOwnerId}");
                    }
                    else
                    {
                        var nameForNotification = advice.Name ?? "Ikke navngivet";
                        _userNotificationService.AddUserNotification(organizationIdOfRelatedEntityId.Value, advice.ObjectOwnerId.Value, nameForNotification, "Advis kunne ikke sendes da der ikke blev fundet nogen gyldig modtager. Dette kan skyldes at der ikke er nogen bruger tilknyttet den/de valgte rolle(r).", advice.RelationId.Value, advice.Type.Value, NotificationType.Advice);
                    }
                }
                return false;
            }
        }

        private static void AddRecipientByName(AdviceUserRelation r, MailAddressCollection mailAddressCollection)
        {
            if (!string.IsNullOrEmpty(r.Email))
            {
                mailAddressCollection.Add(r.Email);
            }
        }

        private void AddRecipientByRole(Advice advice, AdviceUserRelation r, MailAddressCollection mailAddressCollection)
        {
            switch (advice.Type)
            {
                case RelatedEntityType.itContract:
                    var itContractRoles = _itContractRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                        && I.RoleId == r.ItContractRoleId);
                    foreach (var t in itContractRoles)
                    {
                        if (t.User.Deleted) continue;
                        mailAddressCollection.Add(t.User.Email);
                    }

                    break;
                case RelatedEntityType.itSystemUsage:

                    var systemRoles = _itSystemRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                                                              && I.RoleId == r.ItSystemRoleId);
                    foreach (var t in systemRoles)
                    {
                        if (t.User.Deleted) continue;
                        mailAddressCollection.Add(t.User.Email);
                    }

                    break;
                case RelatedEntityType.dataProcessingRegistration:

                    var dpaRoles = _dataProcessingRegistrationRights.AsQueryable().Where(I =>
                        I.ObjectId == advice.RelationId
                        && I.RoleId == r.DataProcessingRegistrationRoleId);
                    foreach (var t in dpaRoles)
                    {
                        if (t.User.Deleted) continue;
                        mailAddressCollection.Add(t.User.Email);
                    }

                    break;
            }
        }

        public void Delete(Advice advice)
        {
            RemoveAdviceAndItsRelatedEntities(advice);
            _adviceRepository.Save();
        }

        private void RemoveAdviceAndItsRelatedEntities(Advice advice)
        {
            _adviceRepository.DeleteByKeyWithReferencePreload(advice.Id);
            DeleteJobFromHangfire(advice);
        }

        private void DeleteJobFromHangfire(Advice advice)
        {
            var adviceEntityId = advice.Id;

            DeleteJobFromHangfire(adviceEntityId);
        }

        private void DeleteJobFromHangfire(int adviceEntityId)
        {
            _hangfireApi.DeleteAdviceFromHangfire(adviceEntityId);
        }

        public void BulkDeleteAdvice(IEnumerable<Advice> toBeDeleted)
        {
            using var transaction = _transactionManager.Begin();
            foreach (var advice in toBeDeleted)
            {
                RemoveAdviceAndItsRelatedEntities(advice);
            }
            _adviceRepository.Save();
            transaction.Commit();
        }

        public void Deactivate(Advice advice)
        {
            DeleteJobFromHangfire(advice);
            advice.IsActive = false;
            _adviceRepository.Update(advice);
            _adviceRepository.Save();
        }

        /// <summary>
        /// Public only to allow HangFire to access it
        /// </summary>
        /// <param name="adviceId"></param>
        public void DeactivateById(int adviceId)
        {
            var advice = _adviceRepository.AsQueryable().ById(adviceId);
            if (advice != null)
            {
                Deactivate(advice);
            }
        }

        private void ScheduleAdvice(Advice advice)
        {
            if (advice.AdviceType == AdviceType.Immediate)
            {
                _hangfireApi.Schedule(() => SendAdvice(advice.Id));
            }
            else if (advice.AdviceType == AdviceType.Repeat)
            {
                var alarmDate = advice.AlarmDate;

                if (alarmDate == null)
                    throw new ArgumentException(nameof(alarmDate) + " must be defined");

                //Only postpone the trigger creation if the alarm date has not been passed yet
                var runAt = _operationClock.Now.Date >= alarmDate.Value.Date ? default(DateTimeOffset?) : new DateTimeOffset(alarmDate.Value.Date);
                _hangfireApi.Schedule(() => CreateOrUpdateJob(advice.Id), runAt);
            }
        }

        public void UpdateSchedule(Advice advice)
        {
            var alarmDate = advice.AlarmDate;

            if (alarmDate == null)
                throw new ArgumentException(nameof(alarmDate) + " must be defined");

            DeleteJobFromHangfire(advice); // Remove existing hangfire job
            ScheduleAdvice(advice);
        }

        /// <summary>
        /// Public only to allow HangFire to access it
        /// </summary>
        /// <param name="adviceId"></param>
        public void CreateOrUpdateJob(int adviceId)
        {
            var advice = _adviceRepository.GetByKey(adviceId);
            if (advice?.Scheduling == null || advice.AlarmDate == null)
            {
                throw new ArgumentException(nameof(adviceId) + " does not point to a valid id or points to an advice without alarm date or scheduling");
            }

            if (IsDeactivated(advice))
            {
                _logger.Warning("Advice with id: {adviceId} will not be scheduled since it has been deactivated.", adviceId);
            }
            else
            {
                var adviceAlarmDate = advice.AlarmDate.Value;
                var adviceScheduling = advice.Scheduling.Value;

                var adviceTriggers = AdviceTriggerFactory.CreateFrom(adviceAlarmDate, adviceScheduling);

                foreach (var adviceTrigger in adviceTriggers)
                {
                    var jobId = adviceTrigger.PartitionId.Match(partitionId => Advice.CreatePartitionJobId(adviceId, partitionId), () => advice.JobId);
                    _hangfireApi.AddOrUpdateRecurringJob(jobId, () => SendAdvice(adviceId), adviceTrigger.Cron);
                }

                if (advice.StopDate.HasValue)
                {
                    //Schedule deactivation to happen the day after the stop date (stop date is "last day alive" for the advice)
                    var deactivateAt = advice.StopDate.Value.Date == DateTime.MaxValue.Date ? DateTime.MaxValue.Date : advice.StopDate.Value.Date.AddDays(1);
                    _hangfireApi.Schedule(() => DeactivateById(advice.Id), new DateTimeOffset(deactivateAt));
                }

                //If time has passed the trigger time, Hangfire will not fire until the next trigger date so we must force it.
                if (adviceAlarmDate.Date.Equals(_operationClock.Now.Date))
                {
                    switch (adviceScheduling)
                    {
                        case Scheduling.Day:
                        case Scheduling.Week:
                        case Scheduling.Month:
                        case Scheduling.Year:
                        case Scheduling.Quarter:
                        case Scheduling.Semiannual:
                            var mustScheduleAdviceToday =
                                advice.AdviceSent.Where(x => x.AdviceSentDate.Date == adviceAlarmDate.Date).Any() == false &&
                                WillTriggerInvokeToday() == false;
                            if (mustScheduleAdviceToday)
                            {
                                //Send the first advice now
                                _hangfireApi.Schedule(() => SendAdvice(adviceId));
                            }
                            break;
                        //Intentional fallthrough - no corrections here
                        case Scheduling.Hour:
                        case Scheduling.Immediate:
                        default:
                            break;
                    }
                }
            }
        }

        private bool WillTriggerInvokeToday()
        {
            var utcNow = _operationClock.Now.ToUniversalTime();
            return
                utcNow.Hour < CronPatternDefaults.TriggerHourUTC ||
                (utcNow.Minute < CronPatternDefaults.TriggerMinute && utcNow.Hour == CronPatternDefaults.TriggerHourUTC);
        }

        public Maybe<Advice> GetAdviceById(int id)
        {
            return _adviceRepository.GetByKey(id);
        }

        private Maybe<int> GetRelatedEntityOrganizationId(Advice advice)
        {
            return _adviceRootResolution.Resolve(advice).Select(x => x.OrganizationId);
        }
    }
}
