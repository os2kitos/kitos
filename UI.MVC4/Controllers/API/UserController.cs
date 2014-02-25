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

        /* TODO: We need to change permissions on create, update functions in here,
         * since all users must be able to add a new user */
    }
}
