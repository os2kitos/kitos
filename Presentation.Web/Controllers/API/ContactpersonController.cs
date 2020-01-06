using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [ControllerEvaluationCompleted]
    public class ContactpersonController : GenericApiController<ContactPerson, ContactPersonDTO>
    {
        private readonly IGenericRepository<ContactPerson> _repository;
        private readonly IGenericRepository<Organization> _orgRepository;

        public ContactpersonController(
            IGenericRepository<ContactPerson> repository,
            IGenericRepository<Organization> orgRepository,
            IAuthorizationContext authorization)
            : base(repository, authorization)
        {
            _repository = repository;
            _orgRepository = orgRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ContactPerson>(x => _orgRepository.AsQueryable().ById(x.OrganizationId.GetValueOrDefault(-1)), base.GetCrudAuthorization());
        }

        // GET DataProtectionAdvisor by OrganizationId
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<ContactPersonDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
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
                        //TODO: Check what happens if we remove this odd stuff
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