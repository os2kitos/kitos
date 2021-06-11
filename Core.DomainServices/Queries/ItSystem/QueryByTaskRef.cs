using System;
using System.Linq;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Queries.ItSystem
{
    public class QueryByTaskRef : IDomainQuery<DomainModel.ItSystem.ItSystem>
    {
        private readonly string _key;
        private readonly Guid? _uuid;

        public QueryByTaskRef(string key = null, Guid? uuid = null)
        {
            _key = key;
            _uuid = uuid;
        }

        public IQueryable<DomainModel.ItSystem.ItSystem> Apply(IQueryable<DomainModel.ItSystem.ItSystem> itSystems)
        {
            var query = _key?.Transform(kle => itSystems.Where(itSystem => itSystem.TaskRefs.Any(taskRef => taskRef.TaskKey == kle))) ?? itSystems;
            return _uuid?.Transform(kle => query.Where(itSystem => itSystem.TaskRefs.Any(taskRef => taskRef.Uuid == kle))) ?? query;
        }
    }
}
