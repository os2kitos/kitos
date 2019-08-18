using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;
using System;
using System.Linq;
using System.Net.Http;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ContactpersonController : GenericApiController<ContactPerson, ContactPersonDTO>
    {
        private readonly IAuthenticationService _authService;
        private readonly IGenericRepository<ContactPerson> _repository;
        private readonly IGenericRepository<Organization> _orgRepository;

        public ContactpersonController(IGenericRepository<ContactPerson> repository, IAuthenticationService authService,
            IGenericRepository<Organization> orgRepository)
            : base(repository)
        {
            _authService = authService;
            _repository = repository;
            _orgRepository = orgRepository;
        }

        // GET DataProtectionAdvisor by OrganizationId
        public override HttpResponseMessage GetSingle(int id)
        {
            try
            {
                var organization = _orgRepository.GetByKey(id);

                if (organization == null) return NotFound();

                var item = Repository.AsQueryable().FirstOrDefault(d => d.OrganizationId == organization.Id);

                //create object if it doesnt exsist
                if (item == null)
                {
                    try
                    {
                        _repository.Insert(new ContactPerson
                        {
                            OrganizationId = organization.Id,
                            ObjectOwnerId = organization.ObjectOwnerId,
                            LastChangedByUserId = KitosUser.Id,
                        });

                        _repository.Save();
                        item = Repository.AsQueryable().FirstOrDefault(d => d.OrganizationId == organization.Id);
                    }
                    catch (Exception e)
                    {
                        return LogError(e);
                    }
                };

                if (!AuthenticationService.HasReadAccess(KitosUser.Id, item))
                {
                    return Forbidden();
                }

                var dto = Map(item);

                return Ok(dto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}