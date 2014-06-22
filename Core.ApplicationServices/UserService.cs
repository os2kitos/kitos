using System;
using System.Linq;
using System.Net.Mail;
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
            
            if(user.CreatedIn != null) user.DefaultOrganizationUnit = user.CreatedIn.GetRoot();

            user = _userRepository.Insert(user);

            // TODO this repeats in IssuePasswordReset(), so refactor...
            var now = DateTime.Now;
            var hash = _cryptoService.Encrypt(now + user.Email);
            var resetLink = "http://kitos.dk/#/reset-password/" + HttpUtility.UrlEncode(hash);
            const string subject = "Oprettelse af KITOS profil";
            var content = "<h2>Kære " + user.Name + "</h2>" +
                          "<p>Du er blevet oprettet, som bruger i KITOS (Kommunernes IT Overblikssystem) under organisationen " + user.CreatedIn.Name + ".</p>" +
                          "Du bedes klikke <a href='" + resetLink + "'>her</a>, hvor du første gang bliver bedt om at indtaste et nyt password for din KITOS profil" +
                          "<p>Linket udløber om " + ResetRequestTTL + "timer.</p>";

            IssuePasswordReset(user, subject, content);
            
            _userRepository.Save();
            
            return user;
        }

        public PasswordResetRequest IssuePasswordReset(User user, string subject, string content)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var now = DateTime.Now;
            var hash = _cryptoService.Encrypt(now + user.Email);
            var resetLink = "http://kitos.dk/#/reset-password/" + HttpUtility.UrlEncode(hash);

            const string mailSubject = "Nulstilning af dit KITOS password";
            var mailContent = "<p>Du har bedt om at få nulstillet dit password.</p>" +
                              "<p><a href='" + resetLink + "'>Klik her for at nulstille passwordet for din KITOS profil</a>.</p>" +
                              "<p>Linket udløber om " + ResetRequestTTL + "timer.</p>";

            var message = new MailMessage()
                {
                    Subject = subject ?? mailSubject,
                    Body = content ?? mailContent,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                };
            message.To.Add(user.Email);

            _mailClient.Send(message);


            var request = new PasswordResetRequest {Hash = hash, Time = now, UserId = user.Id, ObjectOwner = user};

            _passwordResetRequestRepository.Insert(request);
            _passwordResetRequestRepository.Save();

            return request;
        }

        public PasswordResetRequest GetPasswordReset(string hash)
        {
            var passwordReset = _passwordResetRequestRepository.Get(req => req.Hash == hash).FirstOrDefault();

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