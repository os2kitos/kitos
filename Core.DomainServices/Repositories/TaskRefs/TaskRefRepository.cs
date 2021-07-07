using System;
using System.Linq;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Queries;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.TaskRefs
{
    public class TaskRefRepository : ITaskRefRepository
    {
        private readonly IGenericRepository<TaskRef> _repository;

        public TaskRefRepository(IGenericRepository<TaskRef> repository)
        {
            _repository = repository;
        }

        public IQueryable<TaskRef> Query(params IDomainQuery<TaskRef>[] criteria)
        {
            return _repository.AsQueryable().Transform(new IntersectionQuery<TaskRef>(criteria).Apply);
        }

        public Maybe<TaskRef> GetTaskRef(int id)
        {
            return _repository.AsQueryable().ById(id);
        }

        public Maybe<TaskRef> GetTaskRef(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return _repository.AsQueryable().Where(x => x.TaskKey == key).SingleOrDefault();
        }

        public Maybe<TaskRef> GetTaskRef(Guid uuid)
        {
            return _repository.AsQueryable().AsQueryable().ByUuid(uuid);
        }
    }
}
