﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.ItSystemUsageMigration;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    //[RoutePrefix("api/v1/ItSystemUsageMigration")]
    public class ItSystemUsageMigrationController : BaseApiController
    {
        private IItSystemUsageMigrationService _itSystemUsageMigrationService;

        public ItSystemUsageMigrationController(IItSystemUsageMigrationService itSystemUsageMigrationService, IAuthorizationContext authContext) 
            : base(authContext)
        {
            _itSystemUsageMigrationService = itSystemUsageMigrationService;
        }

        [HttpGet]
        //[Route("")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage GetMigrationConflicts([FromUri]int usageId, [FromUri]int toSystemId)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        //[Route("/{usageId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage ExecuteMigration(int usageId, [FromUri]int toSystemId)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        //[Route("UnusedItSystems")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(IEnumerable<ItSystem>))]
        public HttpResponseMessage GetUnusedItSystemsBySearchAndOrganization([FromUri]int organizationId, [FromUri]string nameContent, [FromUri]int limit)
        {
            try
            {
                if (!AllowOrganizationReadAccess(organizationId))
                {
                    return Forbidden();
                }

                var result = _itSystemUsageMigrationService.GetUnusedItSystemsByOrganization(organizationId, nameContent, limit);
                return CreateResponse(result.StatusCode, MapList<ItSystem, ItSystemDTO>(result.ReturnValue));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        private IEnumerable<TOut> MapList<TIn, TOut>(IEnumerable<TIn> input)
        {
            return Map<IEnumerable<TIn>, IEnumerable<TOut>>(input);
        }
    }
}