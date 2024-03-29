﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.Events;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class DataRowController : GenericApiController<DataRow, DataRowDTO>
    {
        private readonly IGenericRepository<ItInterface> _interfaceRepository;

        public DataRowController(
            IGenericRepository<DataRow> repository,
            IGenericRepository<ItInterface> interfaceRepository)
            : base(repository)
        {
            _interfaceRepository = interfaceRepository;
        }

        [NonAction]
        public override HttpResponseMessage GetAll(PagingModel<DataRow> paging)
        {
            throw new NotSupportedException();
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<DataRowDTO>>))]
        public virtual HttpResponseMessage GetByInterface(int interfaceId)
        {
            try
            {
                var itInterface = _interfaceRepository.GetByKey(interfaceId);

                if (itInterface == null)
                    return NotFound();

                if (!AllowRead(itInterface))
                    return Forbidden();

                var dataRows = Repository.Get(x => x.ItInterfaceId == interfaceId);

                var dto = Map(dataRows);
                return Ok(dto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<DataRow, ItInterface>(x => _interfaceRepository.GetByKey(x.ItInterfaceId), base.GetCrudAuthorization());
        }

        protected override void RaiseDeleted(DataRow entity)
        {
            RaiseItInterfaceModified(entity);
        }

        protected override void RaiseUpdated(DataRow item)
        {
            RaiseItInterfaceModified(item);
        }

        protected override void RaiseNewObjectCreated(DataRow savedItem)
        {
            RaiseItInterfaceModified(savedItem);
        }

        private void RaiseItInterfaceModified(DataRow entity)
        {
            var itInterface = _interfaceRepository.GetByKey(entity.ItInterfaceId);
            if (itInterface != null)
                DomainEvents.Raise(new EntityUpdatedEvent<ItInterface>(itInterface));
        }
    }
}
