using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class PasswordResetRequestController : GenericApiController<PasswordResetRequest, string>
    {
        //TODO: where do these go?
        private const int ResetRequestTTL = 12;
        private const string FromAddress = "kitos@it-minds.dk";

        private readonly IGenericRepository<User> _userRepository;
        private readonly IMailClient _mailClient;

        public PasswordResetRequestController(IGenericRepository<PasswordResetRequest> repository, IGenericRepository<User> userRepository, IMailClient mailClient)
            : base(repository)
        {
            _userRepository = userRepository;
            _mailClient = mailClient;
        }

        // POST api/PasswordResetRequest
        public override HttpResponseMessage Post(PasswordResetRequest item)
        {
            try
            {
                var user = _userRepository.GetById(item.User_Id);
                if (user == null)
                    throw new Exception();

                var now = DateTime.Now;

                //TODO: BETTER HASHING???
                var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(now + user.Email, "MD5");

                item.Id = hash;
                item.Time = now;
                item.User = user;

                Repository.Insert(item);
                Repository.Save();

                var resetLink = "http://kitos.dk/Authorize/ResetPassword?Hash=" + hash;
                var mailContent = "<a href='" + resetLink + "'>Klik her for at nulstille passwordet for din KITOS bruger</a>. Linket udløber om " + ResetRequestTTL + " timer.";

                _mailClient.Send(FromAddress, user.Email, "Nulstilning af dit KITOS password", mailContent);

                var msg = new HttpResponseMessage(HttpStatusCode.Created);
                msg.Headers.Location = new Uri(Request.RequestUri + item.Id);
                return msg;
            }
            catch (Exception)
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }
        }

        public override HttpResponseMessage Put(string id, PasswordResetRequest item)
        {
            throw new HttpResponseException(HttpStatusCode.MethodNotAllowed);
        }
    }
}
