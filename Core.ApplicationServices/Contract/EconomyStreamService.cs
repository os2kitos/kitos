using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using NotImplementedException = System.NotImplementedException;

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

            DeleteEconomyStream(economyStream);
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

        public IEnumerable<EconomyStream> GetEconomyStreams(ItContract contract)
        {
            return GetExternalEconomyStreams(contract)
                .Concat(GetInternalEconomyStreams(contract));
        }

        public IEnumerable<EconomyStream> GetInternalEconomyStreams(ItContract contract)
        {
            return contract
                .ExternEconomyStreams
                .ToList();
        }

        public IEnumerable<EconomyStream> GetExternalEconomyStreams(ItContract contract)
        {
            return contract
                .InternEconomyStreams
                .ToList();
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
