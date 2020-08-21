using System.Collections.Generic;
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

        public IEnumerable<int> GetIdsOfSubTree(int organizationId, int organizationUnitId)
        {
            var orgUnitTreeIds = new List<int>();
            var queue = new Queue<int>();
            queue.Enqueue(organizationUnitId);
            while (queue.Count > 0)
            {
                var orgUnitKey = queue.Dequeue();
                orgUnitTreeIds.Add(orgUnitKey);

                var childIds = _repository.AsQueryable().Where(x => x.OrganizationId == organizationId && x.Id == orgUnitKey)
                    .SelectMany(x => x.Children).Select(x => x.Id).ToList();

                //Add sub tree
                foreach (var childId in childIds)
                {
                    queue.Enqueue(childId);
                }
            }

            return orgUnitTreeIds;
        }
    }
}
