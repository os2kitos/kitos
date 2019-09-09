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
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Core.ApplicationServices
{
    using DomainModel.ItSystemUsage;
    using Ninject.Extensions.Logging;
    using System.Collections.Generic;

    public class AdviceService: IAdviceService
    {
        [Inject]
        public MailClient _mailClient {  get; set; }
        [Inject]
        public IGenericRepository<User> _userRepository { get; set;}
        [Inject]
        public IGenericRepository<Advice> _adviceRepository { get; set; }
        [Inject]
        public IGenericRepository<AdviceSent> _adviceSentRepository { get; set; }
        [Inject]
        public IGenericRepository<ItContractRight> _ItContractRights { get; set; }
        [Inject]
        public IGenericRepository<ItProjectRight> _ItprojectRights { get; set; }
        [Inject]
        public IGenericRepository<ItSystemRight> _ItSystemRights { get; set; }
        [Inject]
        public ILogger Logger { get; set; }
        [Inject]
        public IGenericRepository<ItContract> _itContractRepository { get; set; }
        [Inject]
        public IGenericRepository<ItInterface> _itInterfaceRepository { get; set; }
        [Inject]
        public IGenericRepository<ItProject> _itProjectRepository { get; set; }
        [Inject]
        public IGenericRepository<ItSystemUsage> _itSystemUsageRepository { get; set; }
        public AdviceService() {}

        public bool SendAdvice(int id){

            var advice = _adviceRepository.AsQueryable().FirstOrDefault(a => a.Id == id);

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
                                if (!string.IsNullOrEmpty(r.Name))
                                {
                                    message.To.Add(r.Name);
                                }
                            }
                            if (r.RecieverType == RecieverType.RECIEVER && r.RecpientType == RecieverType.ROLE)
                            {
                                switch (advice.Type)
                                {
                                    case ObjectType.itContract:

                                        var itContractRoles = _ItContractRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                        && I.Role.Name == r.Name);
                                        foreach (var t in itContractRoles) {
                                                    message.To.Add(t.User.Email);
                                        }
                                        break;
                                    case ObjectType.itProject:
                                        var projectRoles = _ItprojectRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                        && I.Role.Name == r.Name);
                                        foreach (var t in projectRoles)
                                        {
                                            message.To.Add(t.User.Email);
                                        }
                                        break;
                                    case ObjectType.itSystemUsage:

                                        var systemRoles = _ItSystemRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                        && I.Role.Name == r.Name);
                                        foreach (var t in systemRoles)
                                        {
                                            message.To.Add(t.User.Email);
                                        }
                                        break;
                                }
                            }

                            //ADD CCs
                            if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.USER)
                            {
                                if (!string.IsNullOrEmpty(r.Name))
                                {
                                    message.CC.Add(r.Name);
                                }
                            }
                            if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.ROLE)
                            {
                                switch (advice.Type)
                                {
                                    case ObjectType.itContract:

                                        var itContractRoles = _ItContractRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                        && I.Role.Name == r.Name);
                                        foreach (var t in itContractRoles)
                                        {
                                            message.To.Add(t.User.Email);
                                        }
                                        break;
                                    case ObjectType.itProject:
                                        var projectRoles = _ItprojectRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                        && I.Role.Name == r.Name);
                                        foreach (var t in projectRoles)
                                        {
                                            message.To.Add(t.User.Email);
                                        }
                                        break;
                                    case ObjectType.itSystemUsage:

                                        var systemRoles = _ItSystemRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                        && I.Role.Name == r.Name);
                                        foreach (var t in systemRoles)
                                        {
                                            message.To.Add(t.User.Email);
                                        }
                                        break;
                                }
                            }
                        }

                        //Send Mail.
                        _mailClient.Send(message);
                        advice.SentDate = DateTime.Now;

                        _adviceRepository.Update(advice);
                        _adviceRepository.Save();


                        _adviceSentRepository.Insert(new AdviceSent()
                        {
                            AdviceId = id,
                            AdviceSentDate = DateTime.Now
                        });

                        _adviceSentRepository.Save();

                        return true;
                    }
                    catch (Exception e)
                    {
                        this.Logger?.Error(e, "Error in Advis service");
                        return false;
                    }
                }
                else
                {
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
                                if (r.RecieverType == RecieverType.RECIEVER && r.RecpientType == RecieverType.USER)
                                {
                                    if (!string.IsNullOrEmpty(r.Name))
                                    {
                                        message.To.Add(r.Name);
                                    }
                                }
                                if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.USER)
                                {
                                        if (!string.IsNullOrEmpty(r.Name))
                                        {
                                            message.CC.Add(r.Name);
                                        }
                                }
                                if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.ROLE)
                                {

                                    switch (advice.Type)
                                    {
                                        case ObjectType.itContract:

                                            var itContractRoles = _ItContractRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                            && I.Role.Name == r.Name);
                                            foreach (var t in itContractRoles)
                                            {
                                                message.To.Add(t.User.Email);
                                            }
                                            break;
                                        case ObjectType.itProject:
                                            var projectRoles = _ItprojectRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                            && I.Role.Name == r.Name);
                                            foreach (var t in projectRoles)
                                            {
                                                message.To.Add(t.User.Email);
                                            }
                                            break;
                                        case ObjectType.itSystemUsage:

                                            var systemRoles = _ItSystemRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                            && I.Role.Name == r.Name);
                                            foreach (var t in systemRoles)
                                            {
                                                message.To.Add(t.User.Email);
                                            }
                                            break;
                                    }


                                }
                                if (r.RecieverType == RecieverType.RECIEVER && r.RecpientType == RecieverType.ROLE)
                                {
                                    switch (advice.Type)
                                    {
                                        case ObjectType.itContract:

                                            var itContractRoles = _ItContractRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                            && I.Role.Name == r.Name);
                                            foreach (var t in itContractRoles)
                                            {
                                                message.To.Add(t.User.Email);
                                            }
                                            break;
                                        case ObjectType.itProject:
                                            var projectRoles = _ItprojectRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                            && I.Role.Name == r.Name);
                                            foreach (var t in projectRoles)
                                            {
                                                message.To.Add(t.User.Email);
                                            }
                                            break;
                                        case ObjectType.itSystemUsage:

                                            var systemRoles = _ItSystemRights.AsQueryable().Where(I => I.ObjectId == advice.RelationId
                                            && I.Role.Name == r.Name);
                                            foreach (var t in systemRoles)
                                            {
                                                message.To.Add(t.User.Email);
                                            }
                                            break;
                                    }
                                }
                            }

                            //Send Mail.
                            _mailClient.Send(message);
                            advice.SentDate = DateTime.Now;

                            _adviceRepository.Update(advice);
                            _adviceRepository.Save();


                            _adviceSentRepository.Insert(new AdviceSent()
                            {
                                AdviceId = id,
                                AdviceSentDate = DateTime.Now
                            });

                            _adviceSentRepository.Save();

                            return true;
                        }
                        catch (Exception e)
                        {
                            this.Logger?.Error(e, "Error in Advis service");
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        public IEnumerable<Advice> GetAdvicesForOrg(int orgKey)
        {
            var result = _adviceRepository.SQL($"SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItContract c on c.Id = a.RelationId Where c.OrganizationId = {orgKey} and a.Type = 0 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItProject p on p.Id = a.RelationId Where p.OrganizationId = {orgKey} and a.Type = 2 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItSystemUsage u on u.Id = a.RelationId Where u.OrganizationId = {orgKey} and a.Type = 1 Union SELECT a.* FROM[kitos].[dbo].[Advice] a Join ItInterface i on i.Id = a.RelationId Where i.OrganizationId = {orgKey} and a.Type = 3");
            return result;
        }
    }
}
