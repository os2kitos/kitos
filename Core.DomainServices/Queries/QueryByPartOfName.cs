using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByPartOfName<T> : IDomainQuery<T>
        where T : class, IHasName
    {

        private readonly string _nameContent;

        public QueryByPartOfName(string nameContent)
        {
            _nameContent = string.IsNullOrWhiteSpace(nameContent) ? throw new ArgumentException(nameof(nameContent) + " must be string containing more than whitespaces") : nameContent;
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.Name.Contains(_nameContent));
        }
    }
}
