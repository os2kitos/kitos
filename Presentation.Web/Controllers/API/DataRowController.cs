using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class DataRowController : GenericContextAwareApiController<DataRow, DataRowDTO>
    {
        private readonly IGenericRepository<ItInterface> _interfaceRepository;

        public DataRowController(
            IGenericRepository<DataRow> repository,
            IGenericRepository<ItInterface> interfaceRepository,
            IAuthorizationContext authorization)
            : base(repository, authorization)
        {
            _interfaceRepository = interfaceRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<DataRowDTO>>))]
        public virtual HttpResponseMessage GetByInterface(int interfaceId)
        {
            try
            {
                var item = Repository.Get(x => x.ItInterfaceId == interfaceId);
                if (item == null) return NotFound();


                var dto = Map(item.Where(AllowRead));
                return Ok(dto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<DataRow>(x => _interfaceRepository.AsQueryable().ById(x.ItInterfaceId), base.GetCrudAuthorization());
        }
    }
}
