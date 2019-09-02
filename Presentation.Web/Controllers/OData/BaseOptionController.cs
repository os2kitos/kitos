using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public abstract class BaseOptionController<TType, TDomainModelType> : BaseEntityController<TType>
        where TType : OptionEntity<TDomainModelType>
    {

        private readonly IGenericRepository<TType> _repository;
        private readonly IAuthenticationService _authService;
        // GET: BaseRole
        protected BaseOptionController(IGenericRepository<TType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _repository = repository;
            _authService = authService;
        }

        public override IHttpActionResult Patch(int key, Delta<TType> delta)
        {
            if (delta == null)
            {
                return BadRequest();
            }

            foreach (var t in delta.GetChangedPropertyNames())
            {

                if (t.ToLower() == "priority")
                {
                    var initDelta = delta.GetInstance();
                    var entity = _repository.GetByKey(key);


                    if (initDelta.Priority > entity.Priority)
                    {

                        var entityToBeChanged =
                            _repository.Get().FirstOrDefault(x => x.Priority == initDelta.Priority);

                        if (entityToBeChanged != null)
                        {
                            entityToBeChanged.Priority = entityToBeChanged.Priority - 1;
                            _repository.Update(entityToBeChanged);
                            _repository.Save();
                        }
                        else
                        {
                            if (entity.Priority > 0)
                                initDelta.Priority = entity.Priority;
                        }
                    }
                    else
                    {
                        var entityToBeChanged =
                            _repository.Get().FirstOrDefault(x => x.Priority == initDelta.Priority);

                        if (entityToBeChanged != null)
                        {
                            entityToBeChanged.Priority = entityToBeChanged.Priority + 1;
                            _repository.Update(entityToBeChanged);
                            _repository.Save();
                        }
                        else
                        {
                            initDelta.Priority = entity.Priority;
                        }
                    }
                    break;
                }
            }
            return base.Patch(key, delta);
        }

        public override IHttpActionResult Post(TType entity)
        {
            try
            {
                var Entities = _repository.Get();

                if(Entities.Any())
                {
                    entity.Priority = _repository.Get().Max(e => e.Priority) + 1;
                }else
                {
                    entity.Priority = 1;
                }
            }
            catch(Exception e)
            {
                var message = e.Message;
                return InternalServerError(e);
            }

            return base.Post(entity);
        }

        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);
            if (entity == null)
                return NotFound();

            if (!_authService.HasWriteAccess(UserId, entity))
            {
                return Forbidden();
            }

            var liste = _repository.Get().Where(o => o.Id != key).OrderBy(o => o.Priority);
            try
            {
                Repository.DeleteByKey(key);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            try
            {
                int i = 0;
                foreach (var type in liste)
                {
                    type.Priority = i++;
                }
                _repository.Save();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Could not reprioritize!");
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}