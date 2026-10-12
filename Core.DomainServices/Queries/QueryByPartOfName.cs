using System;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByPartOfName<T> : IDomainQuery<T>
        where T : class, IHasName
    {

        private readonly string _nameContent;
        private readonly string _nameContentLower;

        public QueryByPartOfName(string nameContent)
        {
            _nameContent = string.IsNullOrWhiteSpace(nameContent) ? throw new ArgumentException(nameof(nameContent) + " must be string containing more than whitespaces") : nameContent;
            _nameContentLower = _nameContent.ToLower();
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.Name.ToLower().Contains(_nameContentLower));
        }
    }
}
