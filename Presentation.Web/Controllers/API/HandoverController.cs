using System;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Repositories.Project;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class HandoverController : GenericApiController<Handover, HandoverDTO>
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IItProjectRepository _projectRepository;

        public HandoverController(
            IGenericRepository<Handover> repository, 
            IGenericRepository<User> userRepository,
            IItProjectRepository projectRepository)
            : base(repository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Handover>(goalStatus => _projectRepository.GetById(goalStatus.ItProject.Id), base.GetCrudAuthorization());
        }

        public virtual HttpResponseMessage PostParticipant(int id, [FromUri] int participantId)
        {
            try
            {
                var handover = Repository.GetByKey(id);

                if (!AllowModify(handover))
                {
                    return Forbidden();
                }

                var user = _userRepository.GetByKey(participantId);
                handover.Participants.Add(user);
                handover.LastChanged = DateTime.UtcNow;
                handover.LastChangedByUser = KitosUser;
                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public virtual HttpResponseMessage DeleteParticipant(int id, [FromUri] int participantId)
        {
            try
            {
                var handover = Repository.GetByKey(id);

                if (!AllowModify(handover))
                {
                    return Forbidden();
                }

                var user = _userRepository.GetByKey(participantId);
                handover.Participants.Remove(user);
                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}
