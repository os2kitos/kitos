using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Repositories.SystemUsage;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess.Repositories.SystemUsage
{
    public class ItSystemUsageBatchLoadRepository : IItSystemUsageBatchLoadRepository
    {
        private readonly KitosContext _context;

        public ItSystemUsageBatchLoadRepository(KitosContext context)
        {
            _context = context;
        }

        public IReadOnlyDictionary<int, ItSystemUsage> GetForReadModelRebuild(IEnumerable<int> sourceIds)
        {
            var ids = sourceIds.ToList();
            return _context.ItSystemUsages
                .AsSplitQuery()
                .Include(x => x.ItSystem).ThenInclude(s => s.TaskRefs)
                .Include(x => x.ItSystem).ThenInclude(s => s.BusinessType)
                .Include(x => x.ItSystem).ThenInclude(s => s.BelongsTo)
                .Include(x => x.ItSystem).ThenInclude(s => s.Parent)
                .Include(x => x.Rights).ThenInclude(r => r.Role)
                .Include(x => x.Rights).ThenInclude(r => r.User)
                .Include(x => x.UsedBy).ThenInclude(u => u.OrganizationUnit)
                .Include(x => x.ArchivePeriods)
                .Include(x => x.SensitiveDataLevels)
                .Include(x => x.Contracts).ThenInclude(c => c.ItContract).ThenInclude(ic => ic.Supplier)
                .Include(x => x.MainContract).ThenInclude(mc => mc.ItContract).ThenInclude(ic => ic.Supplier)
                .Include(x => x.AssociatedDataProcessingRegistrations)
                .Include(x => x.UsageRelations).ThenInclude(r => r.RelationInterface)
                .Include(x => x.UsageRelations).ThenInclude(r => r.ToSystemUsage).ThenInclude(s => s.ItSystem)
                .Include(x => x.UsedByRelations).ThenInclude(r => r.FromSystemUsage).ThenInclude(s => s.ItSystem)
                .Include(x => x.ObjectOwner)
                .Include(x => x.LastChangedByUser)
                .Include(x => x.Reference)
                .Include(x => x.ItSystemCategories)
                .Where(x => ids.Contains(x.Id))
                .ToDictionary(x => x.Id);
        }
    }
}
