using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ReferenceController : GenericApiController<ExternalReference, ExternalReferenceDTO>
    {
        private readonly IItProjectRepository _projectRepository;
        private readonly IItContractRepository _contractRepository;
        private readonly IItSystemRepository _systemRepository;
        private readonly IItSystemUsageRepository _systemUsageRepository;

        public ReferenceController(
            IGenericRepository<ExternalReference> repository,
            IItProjectRepository projectRepository,
            IItContractRepository contractRepository,
            IItSystemRepository systemRepository,
            IItSystemUsageRepository systemUsageRepository)
            : base(repository)
        {
            _projectRepository = projectRepository;
            _contractRepository = contractRepository;
            _systemRepository = systemRepository;
            _systemUsageRepository = systemUsageRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            //NOTE: In this case we make sure dependencies are loaded on POST so we CAN use GetOwner
            return new ChildEntityCrudAuthorization<ExternalReference, IEntity>(reference => reference.GetOwner(), base.GetCrudAuthorization());
        }

        public override HttpResponseMessage Post(ExternalReferenceDTO dto)
        {
            if (dto.ItContract_Id.HasValue ||
                dto.ItSystem_Id.HasValue ||
                dto.ItSystemUsage_Id.HasValue ||
                dto.ItProject_Id.HasValue)
            {
                return base.Post(dto);
            }
            return BadRequest("Target object must be specified");
        }

        protected override void PrepareNewObject(ExternalReference item)
        {
            if (item.ItProject_Id.HasValue)
            {
                item.ItProject = _projectRepository.GetById(item.ItProject_Id.Value);
            }
            if (item.Itcontract_Id.HasValue)
            {
                item.ItContract = _contractRepository.GetById(item.Itcontract_Id.Value);
            }
            if (item.ItSystem_Id.HasValue)
            {
                item.ItSystem = _systemRepository.GetSystem(item.ItSystem_Id.Value);
            }
            if (item.ItSystemUsage_Id.HasValue)
            {
                item.ItSystemUsage = _systemUsageRepository.GetSystemUsage(item.ItSystemUsage_Id.Value);
            }
            base.PrepareNewObject(item);
        }
    }
}