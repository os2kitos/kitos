using System;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using System.Collections.Generic;

namespace Presentation.Web.Controllers.API.V1.OData
{
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

        [InternalApi]
        public override IHttpActionResult Patch(int key, Delta<TType> delta)
        {
            if (delta == null)
            {
                return BadRequest();
            }

            var entity = _repository.GetByKey(key);
            var changedPropertyNames = delta.GetChangedPropertyNames().ToHashSet();

            if (AttemptToChangeUuid(delta, entity, changedPropertyNames))
            {
                return BadRequest("Uuid cannot be changed");
            }

            foreach (var t in delta.GetChangedPropertyNames())
            {

                if (t.ToLower() == "priority")
                {
                    var initDelta = delta.GetInstance();

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

        private static bool AttemptToChangeUuid(Delta<TType> delta, TType entity, HashSet<string> changedPropertyNames)
        {
            const string uuidName = nameof(OptionEntity<TDomainModelType>.Uuid);
            return changedPropertyNames.Contains(uuidName) && delta.TryGetPropertyValue(uuidName,
                out var uuid) && ((Guid)uuid) != entity.Uuid;
        }

        [InternalApi]
        public override IHttpActionResult Post(int organizationId, TType entity)
        {
            if (_repository.AsQueryable().Any())
            {
                entity.Priority = _repository.AsQueryable().Max(x => x.Priority) + 1;
            }
            else
            {
                entity.Priority = 1;
            }
            entity.Uuid = Guid.NewGuid();

            return base.Post(organizationId, entity);
        }

        [NonAction]
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();

    }
}