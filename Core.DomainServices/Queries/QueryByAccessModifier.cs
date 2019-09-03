using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByAccessModifier<T> : IDomainQuery<T>
        where T : class, IHasAccessModifier
    {
        private readonly AccessModifier _expectedValue;

        public QueryByAccessModifier(AccessModifier expectedValue)
        {
            _expectedValue = expectedValue;
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.AccessModifier == _expectedValue);
        }
    }
}
