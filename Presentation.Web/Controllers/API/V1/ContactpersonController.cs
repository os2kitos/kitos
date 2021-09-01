using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Core.DomainModel.Constants;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [PublicApi]
    public class ContactpersonController : GenericApiController<ContactPerson, ContactPersonDTO>
    {
        private readonly IGenericRepository<ContactPerson> _repository;
        private readonly IGenericRepository<Organization> _orgRepository;

        public ContactpersonController(
            IGenericRepository<ContactPerson> repository,
            IGenericRepository<Organization> orgRepository)
            : base(repository)
        {
            _repository = repository;
            _orgRepository = orgRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<ContactPerson, Organization>(x => _orgRepository.GetByKey(x.OrganizationId.GetValueOrDefault(EntityConstants.InvalidId)), base.GetCrudAuthorization());
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
                        _repository.Insert(new ContactPerson
                        {
                            OrganizationId = organization.Id,
                        });

                        _repository.Save();
                        item = Repository.AsQueryable().FirstOrDefault(d => d.OrganizationId == organization.Id);
                    }
                    catch (Exception e)
                    {
                        return LogError(e);
                    }
                };

                if (!AllowRead(item))
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

        protected override IQueryable<ContactPerson> GetAllQuery()
        {
            var query = Repository.AsQueryable();
            if (AuthorizationContext.GetCrossOrganizationReadAccess() == CrossOrganizationDataReadAccessLevel.All)
            {
                return query;
            }

            var organizationIds = UserContext.OrganizationIds.ToList();

            return query.Where(x => x.Organization != null && organizationIds.Contains(x.Organization.Id));
        }
    }
}