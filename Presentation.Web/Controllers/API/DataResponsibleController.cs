using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class DataResponsibleController : GenericApiController<DataResponsible, DataResponsibleDTO>
    {
        IGenericRepository<DataResponsible> _repository;
        IGenericRepository<Organization> _orgRepository;
        public DataResponsibleController(IGenericRepository<DataResponsible> repository,
        IGenericRepository<Organization> orgRepository) : base(repository)
        {
            _repository = repository;
            _orgRepository = orgRepository;
        }
        // GET DataProtectionAdvisor by OrganizationId
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<DataResponsibleDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.InternalServerError)]
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
                        _repository.Insert(new DataResponsible
                        {
                            OrganizationId = organization.Id,
                            ObjectOwnerId = organization.ObjectOwnerId,
                            LastChangedByUserId = KitosUser.Id

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