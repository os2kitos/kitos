using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.AdviceSent;
using Core.DomainServices;
using Ninject;
using System;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Core.ApplicationServices
{
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

      public AdviceService() {}

        public bool sendAdvice(int id) {

            var advice = _adviceRepository.AsQueryable().FirstOrDefault(a => a.Id == id);

            if (advice != null)
            {

                try
                {
                    //get user containing the email
                    var reciever = _userRepository.GetByKey(advice.ReceiverId);

                    //Setup mail
                    var message = new MailMessage()
                    {
                        Body = advice.Body,
                        Subject = (advice.Subject).Replace('\r', ' ').Replace('\n', ' '),
                        BodyEncoding = Encoding.UTF8
                    };

                    //Add recivers for Email
                    foreach (var r in advice.Reciepients) {
                        if (r.RecieverType == RecieverType.RECIEVER && r.RecpientType == RecieverType.USER) { 
                        message.To.Add(r.Name);
                        }
                        if (r.RecieverType == RecieverType.CC && r.RecpientType == RecieverType.USER) {
                        message.CC.Add(r.Name);
                        }
                    }

                    //Send Mail.
                    _mailClient.Send(message);

                    _adviceSentRepository.Insert(new AdviceSent() {
                        AdviceId = id,
                        AdviceSentDate = DateTime.Now
                    });

                    _adviceSentRepository.Save();

                    return true;
                }
                catch (Exception e)
                {
                    //todo log exception
                    return false;
                }
            }

            return false;
        }
    }
}
