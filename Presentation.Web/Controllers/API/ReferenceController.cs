using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.References;
using Core.DomainServices;
using Infrastructure.Services.Types;
using Presentation.Web.Models;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ReferenceController : GenericApiController<ExternalReference, ExternalReferenceDTO>
    {
        private readonly IReferenceService _referenceService;

        public ReferenceController(
            IGenericRepository<ExternalReference> repository,
            IReferenceService referenceService)
            : base(repository)
        {
            _referenceService = referenceService;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            //NOTE: In this case we make sure dependencies are loaded on POST so we CAN use GetOwner
            return new ChildEntityCrudAuthorization<ExternalReference, IEntityWithExternalReferences>(reference => reference.GetOwner(), base.GetCrudAuthorization());
        }

        public override HttpResponseMessage Delete(int id, int organizationId)
        {
            return _referenceService
                .DeleteByReferenceId(id)
                .Match(onSuccess: _ => Ok(), onFailure: FromOperationFailure);
        }

        [NonAction]
        public override HttpResponseMessage Post(int organizationId, ExternalReferenceDTO dto)
        {
            return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
        }

        public HttpResponseMessage Post(ExternalReferenceDTO dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            return
                GetOwnerTypeAndId(dto)
                    .Match
                    (
                        onValue: typeAndId => _referenceService
                            .AddReference
                            (
                                typeAndId.Value,
                                typeAndId.Key,
                                dto.Title,
                                dto.ExternalReferenceId,
                                dto.URL,
                                dto.Display
                            )
                            .Match
                            (
                                onSuccess: NewObjectCreated,
                                onFailure: FromOperationError
                            ),
                        onNone: () => BadRequest("Target owner Id must be defined")
                    );
        }

        private static Maybe<KeyValuePair<ReferenceRootType, int>> GetOwnerTypeAndId(ExternalReferenceDTO dto)
        {
            return GetOwnerTypeAndIdOrFallback(ReferenceRootType.Contract, dto.ItContract_Id,
                fallback: () => GetOwnerTypeAndIdOrFallback(ReferenceRootType.System, dto.ItSystem_Id,
                    fallback: () => GetOwnerTypeAndIdOrFallback(ReferenceRootType.SystemUsage, dto.ItSystemUsage_Id,
                        fallback: () => GetOwnerTypeAndIdOrFallback(ReferenceRootType.Project, dto.ItProject_Id,
                            fallback: () => Maybe<KeyValuePair<ReferenceRootType, int>>.None))));
        }

        private static Maybe<KeyValuePair<ReferenceRootType, int>> GetOwnerTypeAndIdOrFallback(ReferenceRootType ownerType, int? ownerId, Func<Maybe<KeyValuePair<ReferenceRootType, int>>> fallback)
        {
            return ownerId.HasValue ? new KeyValuePair<ReferenceRootType, int>(ownerType, ownerId.Value) : fallback();
        }
    }
}