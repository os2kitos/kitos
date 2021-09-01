using System;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Queries.KLE
{
    public class QueryByDescriptionContent : IDomainQuery<TaskRef>
    {
        private readonly string _content;

        public QueryByDescriptionContent(string content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public IQueryable<TaskRef> Apply(IQueryable<TaskRef> source)
        {
            return source.Where(x => x.Description != null && x.Description.Contains(_content));
        }
    }
}
