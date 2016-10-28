using System.Linq;
using System.Web.Http;
using System.Web.OData;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public abstract class BaseOptionController<TType, TDomainModelType> : BaseEntityController<TType> where TType : OptionEntity<TDomainModelType>
    {

        private IGenericRepository<TType> _repository;
        // GET: BaseRole
        public BaseOptionController(IGenericRepository<TType> repository, IAuthenticationService authService)
            : base(repository, authService)
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
                    //var testPriorFromDelta = 1;
                    var initDelta = delta.GetEntity();
                    var entity = _repository.GetByKey(key);

                    if (entity.Priority != 0)
                    {

                        if (initDelta.Priority > entity.Priority)
                        {

                            var entityToBeChanged =
                                _repository.Get().FirstOrDefault(x => x.Priority == entity.Priority + 1);

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
                                _repository.Get().FirstOrDefault(x => x.Priority == entity.Priority - 1);

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
                    else
                    {
                        if (delta.GetEntity().Priority > entity.Priority)
                        {
                            var entitiesToBeChanged = _repository.Get(x => x.Priority >= initDelta.Priority);

                            if (entitiesToBeChanged.Count() > 0)
                            {
                                foreach (var e in entitiesToBeChanged)
                                {
                                    e.Priority = e.Priority + 1;
                                    _repository.Update(e);
                                    _repository.Save();
                                }
                            }
                            else
                            {
                                if (entity.Priority >= 1)
                                {
                                    initDelta.Priority = entity.Priority;
                                }
                            }
                        }
                    }
                }
            }
            return base.Patch(key, delta);
        }

    }
}