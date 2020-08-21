using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core.DomainModel.Organization;

namespace Core.DomainServices.Repositories.Organization
{
    public class OrganizationUnitRepository : IOrganizationUnitRepository
    {
        private readonly IGenericRepository<OrganizationUnit> _repository;

        public OrganizationUnitRepository(IGenericRepository<OrganizationUnit> repository)
        {
            _repository = repository;
        }

        public IEnumerable<int> GetSubTree(int orgKey, int unitKey)
        {
            var orgUnitTreeIds = new List<int>();
            var queue = new Queue<int>();
            queue.Enqueue(unitKey);
            while (queue.Count > 0)
            {
                var orgUnitKey = queue.Dequeue();
                orgUnitTreeIds.Add(orgUnitKey);
                var orgUnit = _repository.AsQueryable()
                    .Include(x => x.Children)
                    .First(x => x.OrganizationId == orgKey && x.Id == orgUnitKey);

                //Add sub tree
                var childIds = orgUnit.Children.Select(x => x.Id);
                foreach (var childId in childIds)
                {
                    queue.Enqueue(childId);
                }
            }

            return orgUnitTreeIds;
        }
    }
}
