using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class UserController : GenericApiController<User, int>
    {
        public UserController(IGenericRepository<User> repository)
            : base(repository)
        {
        }

        // POST api/T
        //TODO: this authorize attr. is probably being overruled by the authorize attribute on base.Post()
        //but I'm not sure.
        [Authorize]
        public override HttpResponseMessage Post(User item)
        {
            //todo: fix obvious security leak here
            //(probably just create random default password)
            item.Password = "default password";

            return base.Post(item);
        }

        /* TODO: We need to change permissions on create, update functions in here,
         * since all users must be able to add a new user */
    }
}
