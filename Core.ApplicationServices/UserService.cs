﻿using System;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;
using System.Security.Cryptography;

namespace Core.ApplicationServices
{
    public class UserService : IUserService
    {
        private readonly TimeSpan _ttl;
        private readonly string _baseUrl;
        private readonly string _mailSuffix;
        private readonly string _defaultUserPassword;
        private readonly bool _useDefaultUserPassword;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IGenericRepository<PasswordResetRequest> _passwordResetRequestRepository;
        private readonly IMailClient _mailClient;
        private readonly ICryptoService _cryptoService;
        private readonly SHA256Managed _crypt;
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        public UserService(TimeSpan ttl,
            string baseUrl,
            string mailSuffix,
            string defaultUserPassword,
            bool useDefaultUserPassword,
            IGenericRepository<User> userRepository,
            IGenericRepository<Organization> orgRepository,
            IGenericRepository<PasswordResetRequest> passwordResetRequestRepository,
            IMailClient mailClient,
            ICryptoService cryptoService)
        {
            _ttl = ttl;
            _baseUrl = baseUrl;
            _mailSuffix = mailSuffix;
            _defaultUserPassword = defaultUserPassword;
            _useDefaultUserPassword = useDefaultUserPassword;
            _userRepository = userRepository;
            _orgRepository = orgRepository;
            _passwordResetRequestRepository = passwordResetRequestRepository;
            _mailClient = mailClient;
            _cryptoService = cryptoService;
            _crypt = new SHA256Managed();
            if (useDefaultUserPassword && string.IsNullOrWhiteSpace(defaultUserPassword))
            {
                throw new ArgumentException(nameof(defaultUserPassword) + " must be defined, when it must be used.");
            }
        }

        public User AddUser(User user, bool sendMailOnCreation, int orgId)
        {
            // hash his salt and default password
            var utcNow = DateTime.UtcNow;
            user.Salt = _cryptoService.Encrypt(utcNow + " spices");

            user.Password = _useDefaultUserPassword
                ? _cryptoService.Encrypt(_defaultUserPassword + user.Salt)
                : _cryptoService.Encrypt(utcNow + user.Salt);

            user.LastChanged = utcNow;
            user.DefaultOrganizationId = orgId;

            _userRepository.Insert(user);
            _userRepository.Save();
            var savedUser = _userRepository.Get(u => u.Id == user.Id).FirstOrDefault();

            if (sendMailOnCreation)
                IssueAdvisMail(savedUser, false, orgId);

            return savedUser;
        }

        public void IssueAdvisMail(User user, bool reminder, int orgId)
        {
            if (user == null || _userRepository.GetByKey(user.Id) == null)
                throw new ArgumentNullException(nameof(user));

            var org = _orgRepository.GetByKey(orgId);

            var reset = GenerateResetRequest(user);
            var resetLink = _baseUrl + "#/reset-password/" + HttpUtility.UrlEncode(reset.Hash);

            var subject = (reminder ? "Påmindelse: " : string.Empty) + "Oprettelse som ny bruger i KITOS " + _mailSuffix;
            var content = "<h2>Kære " + user.Name + "</h2>" +
                          "<p>Du er blevet oprettet, som bruger i KITOS (Kommunernes IT Overblikssystem) under organisationen " +
                          org.Name + ".</p>" +
                          "<p>Du bedes klikke <a href='" + resetLink +
                          "'>her</a>, hvor du første gang bliver bedt om at indtaste et nyt password for din KITOS profil.</p>" +
                          "<p>Linket udløber om " + _ttl.TotalDays + " dage. <a href='" + resetLink + "'>Klik her</a>, " +
                          "hvis dit link er udløbet og du vil blive ledt til 'Glemt password' proceduren.</p>" +
                          "<p><a href='https://os2.eu/sites/default/files/documents/generelt_-_login_i_kitos_som_ny_bruger.pdf'>Klik her for at få Hjælp til log ind og brugerkonto</a></p>" +
                          "<p>Bemærk at denne mail ikke kan besvares.</p>";

            IssuePasswordReset(user, subject, content);

            user.LastAdvisDate = DateTime.UtcNow.Date;
            _userRepository.Update(user);
            _userRepository.Save();
        }

        public PasswordResetRequest IssuePasswordReset(User user, string subject, string content)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var mailContent = "";
            var reset = new PasswordResetRequest();
            if (content == null)
            {
                reset = GenerateResetRequest(user);
                var resetLink = _baseUrl + "#/reset-password/" + HttpUtility.UrlEncode(reset.Hash);
                mailContent = "<p>Du har bedt om at få nulstillet dit password.</p>" +
                              "<p><a href='" + resetLink +
                              "'>Klik her for at nulstille passwordet for din KITOS profil</a>.</p>" +
                              "<p>Linket udløber om " + _ttl.TotalDays + " dage.</p>" +
                              "<p><a href='https://os2.eu/sites/default/files/documents/generelt_-_login_i_kitos_som_ny_bruger.pdf'>Klik her for at få Hjælp til log ind og brugerkonto</a></p>" +
                              "<p>Bemærk at denne mail ikke kan besvares.</p>";
            }
            var mailSubject = "Nulstilning af dit KITOS password" + _mailSuffix;

            var message = new MailMessage()
                {
                    Subject = (subject ?? mailSubject).Replace('\r', ' ').Replace('\n', ' '),
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
            var now = DateTime.UtcNow;

            byte[] randomNumber = new byte[8];
            rngCsp.GetBytes(randomNumber);

            byte[] encrypted = _crypt.ComputeHash(randomNumber);

            var hash = HttpServerUtility.UrlTokenEncode(encrypted);

            var request = new PasswordResetRequest { Hash = hash, Time = now, UserId = user.Id, ObjectOwner = user, LastChangedByUser = user };

            _passwordResetRequestRepository.Insert(request);
            _passwordResetRequestRepository.Save();

            return request;
        }

        public PasswordResetRequest GetPasswordReset(string hash)
        {
            var passwordReset = _passwordResetRequestRepository.Get(req => req.Hash == hash).FirstOrDefault();

            if (passwordReset == null) return null;

            var timespan = DateTime.UtcNow - passwordReset.Time;
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
                throw new ArgumentNullException(nameof(passwordResetRequest));

            if (!IsValidPassword(newPassword))
                throw new ArgumentException("New password is invalid");

            var user = passwordResetRequest.User;

            user.Password = _cryptoService.Encrypt(newPassword + user.Salt);
            _userRepository.Update(user);
            _userRepository.Save();

            _passwordResetRequestRepository.DeleteByKey(passwordResetRequest.Id);
            _passwordResetRequestRepository.Save();
        }


        private static bool IsValidPassword(string password)
        {
            return password.Length >= 6;
        }

        public User GetUserById(int id)
        {
            return _userRepository.GetByKey(id);
        }

        public void Dispose()
        {
            _crypt?.Dispose();
        }
    }
}
