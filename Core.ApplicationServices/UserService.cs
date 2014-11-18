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
        private readonly TimeSpan _ttl;
        private readonly string _baseUrl;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IGenericRepository<PasswordResetRequest> _passwordResetRequestRepository;
        private readonly IMailClient _mailClient;
        private readonly ICryptoService _cryptoService;

        public UserService(TimeSpan ttl,
            string baseUrl,
            IGenericRepository<User> userRepository, 
            IGenericRepository<Organization> orgRepository, 
            IGenericRepository<PasswordResetRequest> passwordResetRequestRepository, 
            IMailClient mailClient, 
            ICryptoService cryptoService)
        {
            _ttl = ttl;
            _baseUrl = baseUrl;
            _userRepository = userRepository;
            _orgRepository = orgRepository;
            _passwordResetRequestRepository = passwordResetRequestRepository;
            _mailClient = mailClient;
            _cryptoService = cryptoService;
        }

        public User AddUser(User user)
        {
            // hash his salt and default password
            user.Salt = _cryptoService.Encrypt(DateTime.Now + " spices");
#if DEBUG
            user.Password = _cryptoService.Encrypt("arne123" + user.Salt); //TODO: Don't use default password
#else
            user.Password = _cryptoService.Encrypt(DateTime.Now + user.Salt);
#endif
            // user isn't an EF proxy class so navigation properites aren't set,
            // so we need to fetch org name ourself
            var org = _orgRepository.GetByKey(user.CreatedInId);

            user.DefaultOrganizationUnitId = org.GetRoot().Id;

            _userRepository.Insert(user);
            _userRepository.Save();

            var reset = GenerateResetRequest(user);
            var resetLink = _baseUrl + "#/reset-password/" + HttpUtility.UrlEncode(reset.Hash);

            const string subject = "Oprettelse af KITOS profil";
            var content = "<h2>Kære " + user.Name + "</h2>" +
                          "<p>Du er blevet oprettet, som bruger i KITOS (Kommunernes IT Overblikssystem) under organisationen " +
                          org.Name + ".</p>" +
                          "<p>Du bedes klikke <a href='" + resetLink +
                          "'>her</a>, hvor du første gang bliver bedt om at indtaste et nyt password for din KITOS profil.</p>" +
                          "<p>Linket udløber om " + _ttl.TotalDays + " dage.</p>";

            IssuePasswordReset(user, subject, content);

            return user;
        }

        public PasswordResetRequest IssuePasswordReset(User user, string subject, string content)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var mailContent = "";
            var reset = new PasswordResetRequest();
            if (content == null)
            {
                reset = GenerateResetRequest(user);
                var resetLink = _baseUrl + "#/reset-password/" + HttpUtility.UrlEncode(reset.Hash);
                mailContent = "<p>Du har bedt om at få nulstillet dit password.</p>" +
                              "<p><a href='" + resetLink +
                              "'>Klik her for at nulstille passwordet for din KITOS profil</a>.</p>" +
                              "<p>Linket udløber om " + _ttl.TotalDays + " dage.</p>";
            }
            const string mailSubject = "Nulstilning af dit KITOS password";

            var message = new MailMessage()
                {
                    Subject = subject ?? mailSubject,
                    Body = content ?? mailContent,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                };
            message.To.Add(user.Email);

            _mailClient.Send(message);

            return reset;
        }

        private PasswordResetRequest GenerateResetRequest(User user)
        {
            var now = DateTime.Now;
            var hash = _cryptoService.Encrypt(now + user.Email);

            var request = new PasswordResetRequest { Hash = hash, Time = now, UserId = user.Id, ObjectOwner = user, LastChangedByUser = user};

            _passwordResetRequestRepository.Insert(request);
            _passwordResetRequestRepository.Save();

            return request;
        }

        public PasswordResetRequest GetPasswordReset(string hash)
        {
            var passwordReset = _passwordResetRequestRepository.Get(req => req.Hash == hash).FirstOrDefault();

            if (passwordReset == null) return null;

            var timespan = DateTime.Now - passwordReset.Time;
            if (timespan > _ttl)
            {
                // the reset request is too old, delete it
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
