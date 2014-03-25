using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class DepartmentRoleController : GenericOptionApiController<DepartmentRole, DepartmentRight, RoleDTO>
    {
        public DepartmentRoleController(IGenericRepository<DepartmentRole> repository) 
            : base(repository)
        {
        }
    }
}
