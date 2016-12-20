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
    using Ninject.Extensions.Logging;

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

        public AdviceService() {}

        public bool sendAdvice(int id){

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

                       // message.From = new MailAddress("no_reply@kitos.dk");

                        //Add recivers for Email
                        foreach (var r in advice.Reciepients)
                        {
                            if (r.RecieverType == RecieverType.RECIEVER && r.RecpientType == RecieverType.USER)
                            {
                                message.To.Add(r.Name);
                            }
                            if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.USER)
                            {
                                message.CC.Add(r.Name);
                            }
                            if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.ROLE)
                            {

                                switch (advice.Type)
                                {
                                    case ObjectType.itContract:

                                        var result = _ItContractRights.AsQueryable().FirstOrDefault(I => I.ObjectId == advice.RelationId
                                       && I.Role.Name == r.Name);
                                        if (result != null)
                                        {
                                            if (result.User != null)
                                            {
                                                message.CC.Add(result.User.Email);
                                            }
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
                        //todo log exception
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
                                    message.To.Add(r.Name);
                                }
                                if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.USER)
                                {
                                    message.CC.Add(r.Name);
                                }
                                if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.ROLE)
                                {

                                    switch (advice.Type)
                                    {
                                        case ObjectType.itContract:

                                            var result = _ItContractRights.AsQueryable().FirstOrDefault(I => I.ObjectId == advice.RelationId
                                           && I.Role.Name == r.Name);
                                            if (result != null)
                                            {
                                                if (result.User != null)
                                                {
                                                    message.CC.Add(result.User.Email);
                                                }
                                            }
                                            break;
                                    }


                                }
                                if (r.RecieverType == RecieverType.RECIEVER && r.RecpientType == RecieverType.ROLE)
                                {
                                    switch (advice.Type)
                                    {
                                        case ObjectType.itContract:

                                            var result = _ItContractRights.AsQueryable().FirstOrDefault(I => I.ObjectId == advice.RelationId
                                            && I.Role.Name == r.Name);
                                            if (result != null)
                                            {
                                                if (result.User != null)
                                                {
                                                    message.To.Add(result.User.Email);
                                                }
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
                            //todo log exception
                            this.Logger?.Error(e, "Error in Advis service");
                            return false;
                        }
                    }
                }
            }

            return false;
        }
    }
}
