using System;
using System.Linq;
using System.Net.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemUsageController : GenericApiController<ItSystemUsage, int, ItSystemUsageDTO> 
    {
        public ItSystemUsageController(IGenericRepository<ItSystemUsage> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage GetByOrganization(int organizationId)
        {
            try
            {
                var usages = Repository.Get(u => u.OrganizationId == organizationId);
                
                return Ok(Map(usages));

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetByItSystemAndOrganization(int itSystemId, int organizationId)
        {
            try
            {
                var usage = Repository.Get(u => u.ItSystemId == itSystemId && u.OrganizationId == organizationId).FirstOrDefault();

                return usage == null ? NotFound() : Ok(Map(usage));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public override HttpResponseMessage Post(ItSystemUsageDTO dto)
        {
            try
            {
                if (Repository.Get(usage => usage.ItSystemId == dto.ItSystemId 
                    && usage.OrganizationId == dto.OrganizationId).Any())
                    return Conflict("Usage already exist");

                var item = Map(dto);
                Repository.Insert(item);
                Repository.Save();

                return Created(item, new Uri(Request.RequestUri + "?itSystemId=" + dto.ItSystemId + "&organizationId" + dto.OrganizationId));

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Delete(int itSystemId, int organizationId)
        {
            try
            {
                var usage = Repository.Get(u => u.ItSystemId == itSystemId && u.OrganizationId == organizationId).FirstOrDefault();

                if (usage == null) return NotFound();

                Repository.DeleteByKey(usage.Id);
                Repository.Save();

                return Ok();

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

    }
}
