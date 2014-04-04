using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class UserService : IUserService
    {
        //TODO: where do these go?
        private const int ResetRequestTTL = 12;

        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<PasswordResetRequest> _passwordResetRequestRepository;
        private readonly IMailClient _mailClient;
        private readonly ICryptoService _cryptoService;

        public UserService(IGenericRepository<User> userRepository, IGenericRepository<PasswordResetRequest> passwordResetRequestRepository, IMailClient mailClient, ICryptoService cryptoService)
        {
            _userRepository = userRepository;
            _passwordResetRequestRepository = passwordResetRequestRepository;
            _mailClient = mailClient;
            _cryptoService = cryptoService;
        }

        public User AddUser(User user)
        {
            //to avoid access to modified closure-warning
            var tmp = user;

            //check if user already exists. If so, just return him
            var existingUser = _userRepository.Get(u => u.Email == tmp.Email).FirstOrDefault();
            if (existingUser != null) return existingUser;

            //otherwise, hash his salt and default password
            user.Salt = _cryptoService.Encrypt(DateTime.Now + " spices");
#if DEBUG
            user.Password = _cryptoService.Encrypt("arne123" + user.Salt); //TODO: Don't use default password
#else
            user.Password = _cryptoService.Encrypt(DateTime.Now + user.Salt);
#endif
            user = _userRepository.Insert(user);
            IssuePasswordReset(user);

            return user;
        }

        public PasswordResetRequest IssuePasswordReset(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var now = DateTime.Now;

            var hash = _cryptoService.Encrypt(now + user.Email);

            var request = new PasswordResetRequest {Id = hash, Time = now, UserId = user.Id};

            _passwordResetRequestRepository.Insert(request);
            _passwordResetRequestRepository.Save();

            var resetLink = "http://kitos.dk/#/reset-password/" + HttpUtility.UrlEncode(hash);

            const string mailSubject = "Nulstilning af dit KITOS password";
            var mailContent = "<a href='" + resetLink + "'>" +
                              "Klik her for at nulstille passwordet for din KITOS bruger</a>. Linket udløber om " +
                              ResetRequestTTL + " timer.";

            var message = new MailMessage()
                {
                    Body = mailContent,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    Subject = mailSubject,
                };
            message.To.Add(user.Email);

            _mailClient.Send(message);

            return request;
        }

        public PasswordResetRequest GetPasswordReset(string requestId)
        {
            var passwordReset = _passwordResetRequestRepository.GetByKey(requestId);

            if (passwordReset == null) return null;

            var timespan = DateTime.Now - passwordReset.Time;
            if (timespan.TotalHours > ResetRequestTTL)
            {
                //the reset request is too old, delete it
                _passwordResetRequestRepository.DeleteByKey(passwordReset.Id);
                _passwordResetRequestRepository.Save();
                return null;
            }

            return passwordReset;
        }

        public void ResetPassword(PasswordResetRequest passwordResetRequest, string newPassword)
        {
            if (passwordResetRequest == null)
                throw new ArgumentNullException("passwordResetRequest");

            if(!IsValidPassword(newPassword))
                throw new ArgumentException("New password is invalid");

            var user = passwordResetRequest.User;

            user.Password = _cryptoService.Encrypt(newPassword + user.Salt);
            _userRepository.Update(user);
            _userRepository.Save();

            _passwordResetRequestRepository.DeleteByKey(passwordResetRequest.Id);
            _passwordResetRequestRepository.Save();
        }


        private bool IsValidPassword(string password)
        {
            return password.Length >= 6;
        }
    }
}