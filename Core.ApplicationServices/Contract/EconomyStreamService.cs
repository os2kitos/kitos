using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Core.ApplicationServices.Contract
{
    public class EconomyStreamService : IEconomyStreamService
    {
        private readonly IGenericRepository<EconomyStream> _economyRepository;
        private readonly IDomainEvents _domainEvents;
        private readonly IAuthorizationContext _authorizationContext;

        public EconomyStreamService(IGenericRepository<EconomyStream> economyRepository, 
            IDomainEvents domainEvents, 
            IAuthorizationContext authorizationContext)
        {
            _economyRepository = economyRepository;
            _domainEvents = domainEvents;
            _authorizationContext = authorizationContext;
        }

        public Maybe<OperationError> Delete(int id)
        {
            var economyStream = _economyRepository.GetByKey(id);

            if (economyStream == null)
            {
                return new OperationError(OperationFailure.NotFound);
            }
            if (!_authorizationContext.AllowDelete(economyStream))
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            _economyRepository.DeleteWithReferencePreload(economyStream);
            _economyRepository.Save();

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> TransferRange(OrganizationUnit targetUnit, IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                var economyStream = _economyRepository.GetByKey(id);
                if (economyStream == null)
                {
                    return new OperationError(OperationFailure.NotFound);
                }
                if (!_authorizationContext.AllowModify(economyStream))
                {
                    return new OperationError(OperationFailure.Forbidden);
                }

                economyStream.OrganizationUnit = targetUnit;
                economyStream.OrganizationUnitId = targetUnit.Id;
                _economyRepository.Update(economyStream);
            }
            _economyRepository.Save();

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> DeleteRange(IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                var economyStream = _economyRepository.GetByKey(id);
                var result = DeleteEconomyStream(economyStream);
                if(result.IsNone)
                    continue;
                
                return result;
            }
            _economyRepository.Save();

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> DeleteRange(IEnumerable<EconomyStream> entities)
        {
            foreach (var entity in entities)
            {
                var result = DeleteEconomyStream(entity);
                if(result.IsNone)
                    continue;
                
                return result;
            }
            _economyRepository.Save();

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> DeleteEconomyStream(EconomyStream entity)
        {
            if (entity == null)
            {
                return new OperationError(OperationFailure.NotFound);
            }
            if (!_authorizationContext.AllowDelete(entity))
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            
            _economyRepository.DeleteWithReferencePreload(entity);
            return Maybe<OperationError>.None;
        }
    }
}
