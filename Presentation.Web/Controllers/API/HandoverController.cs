using System;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class HandoverController : GenericContextAwareApiController<Handover, HandoverDTO>
    {
        private readonly IGenericRepository<User> _userRepository;

        public HandoverController(IGenericRepository<Handover> repository, IGenericRepository<User> userRepository)
            : base(repository)
        {
            _userRepository = userRepository;
        }

        public virtual HttpResponseMessage PostParticipant(int id, [FromUri] int participantId)
        {
            try
            {
                var handover = Repository.GetByKey(id);
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
