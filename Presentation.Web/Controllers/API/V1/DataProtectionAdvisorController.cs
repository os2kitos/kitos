﻿using Core.DomainModel.Organization;
using Core.DomainServices;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Core.DomainModel.Constants;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class DataProtectionAdvisorController : GenericApiController<DataProtectionAdvisor, DataProtectionAdvisorDTO>
    {
        private readonly IGenericRepository<DataProtectionAdvisor> _repository;
        private readonly IGenericRepository<Organization> _orgRepository;

        public DataProtectionAdvisorController(
            IGenericRepository<DataProtectionAdvisor> repository,
            IGenericRepository<Organization> orgRepository)
            : base(repository)
        {
            _repository = repository;
            _orgRepository = orgRepository;
        }

        protected override IQueryable<DataProtectionAdvisor> GetAllQuery()
        {
            var query = Repository.AsQueryable();
            if (AuthorizationContext.GetCrossOrganizationReadAccess() == CrossOrganizationDataReadAccessLevel.All)
            {
                return query;
            }
            var organizationIds = UserContext.OrganizationIds.ToList();

            return query.Where(x => x.Organization != null && organizationIds.Contains(x.Organization.Id));
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<DataProtectionAdvisor, Organization>(x => _orgRepository.GetByKey(x.OrganizationId.GetValueOrDefault(EntityConstants.InvalidId)), base.GetCrudAuthorization());
        }

        // GET DataProtectionAdvisor by OrganizationId
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<DataProtectionAdvisorDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public override HttpResponseMessage GetSingle(int id)
        {
            try
            {
                var organization = _orgRepository.GetByKey(id);

                if (organization == null)
                {
                    return NotFound();
                }

                var item = Repository.AsQueryable().FirstOrDefault(d => d.OrganizationId == organization.Id);

                //create object if it doesnt exsist
                if (item == null)
                {
                    try
                    {
                        _repository.Insert(new DataProtectionAdvisor
                        {
                            OrganizationId = organization.Id
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
    }
}