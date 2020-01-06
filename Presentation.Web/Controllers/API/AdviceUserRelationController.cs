using Core.DomainModel.Advice;
using Core.DomainServices;
using Presentation.Web.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    [ControllerEvaluationCompleted]
    [InternalApi]
    public class AdviceUserRelationController : GenericApiController<AdviceUserRelation, AdviceUserRelationDTO>
    {
        IGenericRepository<AdviceUserRelation> _repository;
        public AdviceUserRelationController(IGenericRepository<AdviceUserRelation> repository) : base(repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// Sletter adviser med det specificerede id fra en genereisk advis
        /// </summary>
        /// <param name="adviceId"></param>
        /// <returns></returns>
        [HttpDelete]
        public virtual HttpResponseMessage DeleteByAdviceId(int adviceId)
        {
            try
            {
                foreach (var d in _repository.AsQueryable().Where(d => d.AdviceId == adviceId)) {
                    _repository.Delete(d);
                }
                _repository.Save();
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
    }
}