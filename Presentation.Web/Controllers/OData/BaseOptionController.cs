using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [MigratedToNewAuthorizationContext]
    public abstract class BaseOptionController<TType, TDomainModelType> : BaseEntityController<TType>
        where TType : OptionEntity<TDomainModelType>
    {

        private readonly IGenericRepository<TType> _repository;
        // GET: BaseRole
        protected BaseOptionController(IGenericRepository<TType> repository)
            : base(repository)
        {
            _repository = repository;
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
            if (_repository.AsQueryable().Any())
            {
                entity.Priority = _repository.AsQueryable().Max(x => x.Priority) + 1;
            }
            else
            {
                entity.Priority = 1;
            }

            return base.Post(entity);
        }

        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.GetByKey(key);
            if (entity == null)
                return NotFound();

            if (!AllowDelete(entity))
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