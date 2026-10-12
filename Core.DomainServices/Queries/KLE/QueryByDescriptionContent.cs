using System;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Queries.KLE
{
    public class QueryByDescriptionContent : IDomainQuery<TaskRef>
    {
        private readonly string _content;
        private readonly string _contentLower;

        public QueryByDescriptionContent(string content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _contentLower = _content.ToLower();
        }

        public IQueryable<TaskRef> Apply(IQueryable<TaskRef> source)
        {
            return source.Where(x => x.Description != null && x.Description.ToLower().Contains(_contentLower));
        }
    }
}
