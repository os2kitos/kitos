using System;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectController : GenericApiController<ItProject, int, ItProjectDTO>
    {
        private readonly IItProjectService _itProjectService;
        private readonly IGenericRepository<Organization> _orgRepository;

        public ItProjectController(IGenericRepository<ItProject> repository, IItProjectService itProjectService, IGenericRepository<Organization> orgRepository) 
            : base(repository)
        {
            _itProjectService = itProjectService;
            _orgRepository = orgRepository;
        }

        public HttpResponseMessage GetPrograms(string q, int orgId, bool? programs)
        {
            try
            {
                //TODO: check for user read access rights

                var org = _orgRepository.GetByKey(orgId);

                var thePrograms = _itProjectService.GetPrograms(org, q);

                return Ok(Map(thePrograms));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetNonPrograms(string q, int orgId, bool? nonPrograms)
        {
            try
            {
                //TODO: check for user read access rights

                var org = _orgRepository.GetByKey(orgId);

                var projects = _itProjectService.GetProjects(org, q);

                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

    }
}