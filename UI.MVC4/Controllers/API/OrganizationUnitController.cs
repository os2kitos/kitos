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
    public class OrganizationUnitController : GenericApiController<OrganizationUnit, int, OrgUnitDTO>
    {
        public OrganizationUnitController(IGenericRepository<OrganizationUnit> repository) : base(repository)
        {
        }

        public HttpResponseMessage GetByMunicipality(int municipality)
        {
            try
            {
                var orgUnit = Repository.Get(o => o.Municipality_Id == municipality && o.Parent == null).First();

                var item = Map<OrganizationUnit, OrgUnitDTO>(orgUnit);

                return Ok(item);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
