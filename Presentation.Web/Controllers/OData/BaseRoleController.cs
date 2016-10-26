using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.OData;

namespace Presentation.Web.Controllers.OData
{
    public abstract class BaseRoleController<T, Type> : BaseEntityController<T> where T : OptionEntity<Type>
    {

        private IGenericRepository<T> _repository;
        // GET: BaseRole
        public BaseRoleController(IGenericRepository<T> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _repository = repository;
        }
        public override IHttpActionResult Patch(int key, Delta<T> delta)
        {
            
            foreach (var t in delta.GetChangedPropertyNames())
            {

                if (t.ToLower() == "priority")
                {
                    //var testPriorFromDelta = 1;
                    var initDelta = delta.GetEntity();
                    var entity = _repository.GetByKey(key);

                    if (entity.priority != 0)
                    {

                        if (initDelta.priority > entity.priority)
                        {

                            var entityToBeChanged = _repository.Get().FirstOrDefault(x => x.priority == entity.priority + 1);

                            if (entityToBeChanged != null)
                            {
                                entityToBeChanged.priority = entityToBeChanged.priority - 1;
                                _repository.Update(entityToBeChanged);
                                _repository.Save();
                            }
                            else
                            {
                                if (entity.priority > 0)
                                    initDelta.priority = entity.priority;
                            }
                        }
                        else
                        {
                            var entityToBeChanged = _repository.Get().FirstOrDefault(x => x.priority == entity.priority - 1);

                            if (entityToBeChanged != null)
                            {
                                entityToBeChanged.priority = entityToBeChanged.priority + 1;
                                _repository.Update(entityToBeChanged);
                                _repository.Save();
                            }
                            else
                            {
                                initDelta.priority = entity.priority;
                            }
                        }
                        break;
                    }
                    else
                    {
                        if (delta.GetEntity().priority > entity.priority)
                        {
                            var entitiesToBeChanged = _repository.Get(x => x.priority >= initDelta.priority);

                            if (entitiesToBeChanged.Count() > 0)
                            {
                                foreach (var e in entitiesToBeChanged)
                                {
                                    e.priority = e.priority + 1;
                                    _repository.Update(e);
                                    _repository.Save();
                                }
                            }
                            else
                            {
                                if (entity.priority >= 1)
                                {
                                    initDelta.priority = entity.priority;
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