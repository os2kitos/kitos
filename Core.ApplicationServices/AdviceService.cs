using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices
{
    public class AdviceService: IAdviceService
    {
        MailClient _mailClient;
        private readonly IGenericRepository<User> _userRepository;

        public AdviceService(MailClient mailClient, IGenericRepository<User> userRepository) {
            _mailClient = mailClient;
            _userRepository = userRepository;
        }

        public bool sendAdvice(Advice advice) {
            
            try
            {
                //get user containing the email
                var reciever = _userRepository.GetByKey(advice.ReceiverId);
                
                //Setup mail
                var message = new MailMessage() { 
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
            }
            catch (Exception e) {
                return false;
            }
            return true;
        }
    }
}
