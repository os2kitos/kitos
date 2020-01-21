using System;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByName<T> : IDomainQuery<T>
        where T : class, IHasName
    {
        private readonly string _name;

        public QueryByName(string name)
        {
            _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException(nameof(name) + " must be string containing more than whitespaces") : name;
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(entity => entity.Name == _name);
        }
    }
}
