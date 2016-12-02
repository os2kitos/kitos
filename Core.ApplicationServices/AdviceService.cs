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

                    //Add reciver for Email
                    message.To.Add(reciever.Email);

                    if (advice.CarbonCopyReceiver != null)
                    {
                        //Add CC('s)
                        message.CC.Add(reciever.Email);
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
