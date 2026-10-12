using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.Queries.SystemUsage
{
    public class QueryBySystemNameContent : IDomainQuery<ItSystemUsage>
    {
        private readonly string _nameContent;
        private readonly string _nameContentLower;

        public QueryBySystemNameContent(string nameContent)
        {
            _nameContent = nameContent ?? throw new ArgumentException(nameof(nameContent));
            _nameContentLower = _nameContent.ToLower();
        }

        public IQueryable<ItSystemUsage> Apply(IQueryable<ItSystemUsage> source)
        {
            return source.Where(systemUsage => systemUsage.ItSystem.Name.ToLower().Contains(_nameContentLower));
        }
    }
}
