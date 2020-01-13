using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.SystemUsage
{
    public class ItSystemUsageService : IItSystemUsageService
    {
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IAuthorizationContext _authorizationContext;

        public ItSystemUsageService(IGenericRepository<ItSystemUsage> usageRepository, IAuthorizationContext authorizationContext)
        {
            _usageRepository = usageRepository;
            _authorizationContext = authorizationContext;
        }

        public TwoTrackResult<ItSystemUsage, GenericOperationFailure> Add(ItSystemUsage newSystemUsage, User objectOwner)
        {
            // create the system usage
            var existing = GetByOrganizationAndSystemId(newSystemUsage.OrganizationId, newSystemUsage.ItSystemId);
            if (existing != null)
            {
                return TwoTrackResult<ItSystemUsage, GenericOperationFailure>.Failure(GenericOperationFailure.Conflict);
            }

            if (!_authorizationContext.AllowCreate<ItSystemUsage>(newSystemUsage))
            {
                return TwoTrackResult<ItSystemUsage, GenericOperationFailure>.Failure(GenericOperationFailure.Forbidden);
            }

            var usage = _usageRepository.Create();

            usage.ItSystemId = newSystemUsage.ItSystemId;
            usage.OrganizationId = newSystemUsage.OrganizationId;
            usage.ObjectOwner = objectOwner;
            usage.LastChangedByUser = objectOwner;
            usage.DataLevel = newSystemUsage.DataLevel;
            usage.ContainsLegalInfo = newSystemUsage.ContainsLegalInfo;
            usage.AssociatedDataWorkers = newSystemUsage.AssociatedDataWorkers;
            _usageRepository.Insert(usage);
            _usageRepository.Save(); // abuse this as UoW

            return TwoTrackResult<ItSystemUsage, GenericOperationFailure>.Success(usage);
        }

        public TwoTrackResult<ItSystemUsage, GenericOperationFailure> Delete(int id)
        {
            var itSystemUsage = GetById(id);
            if (itSystemUsage == null)
            {
                return TwoTrackResult<ItSystemUsage, GenericOperationFailure>.Failure(GenericOperationFailure.NotFound);
            }
            if (!_authorizationContext.AllowDelete(itSystemUsage))
            {
                return TwoTrackResult<ItSystemUsage, GenericOperationFailure>.Failure(GenericOperationFailure.Forbidden);
            }

            // delete it system usage
            _usageRepository.DeleteByKeyWithReferencePreload(id);
            _usageRepository.Save();
            return TwoTrackResult<ItSystemUsage, GenericOperationFailure>.Success(itSystemUsage);
        }

        public ItSystemUsage GetByOrganizationAndSystemId(int organizationId, int systemId)
        {
            return _usageRepository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .FirstOrDefault(u => u.ItSystemId == systemId);
        }

        public ItSystemUsage GetById(int usageId)
        {
            return _usageRepository.GetByKey(usageId);
        }
    }
}
