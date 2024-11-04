using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;

namespace Core.ApplicationServices.GlobalOptions
{
        public abstract class GlobalOptionsServiceBase<TOptionType, TReferenceType>
            where TOptionType : OptionEntity<TReferenceType>, new()
        {
            protected readonly IGenericRepository<TOptionType> _globalOptionsRepository;
            protected readonly IOrganizationalUserContext _activeUserContext;
            protected readonly IDomainEvents _domainEvents;

            protected GlobalOptionsServiceBase(IGenericRepository<TOptionType> globalOptionsRepository, IOrganizationalUserContext activeUserContext, IDomainEvents domainEvents)
            {
                _globalOptionsRepository = globalOptionsRepository;
                _activeUserContext = activeUserContext;
                _domainEvents = domainEvents;
            }

            protected int GetNextOptionPriority()
            {
                var options = _globalOptionsRepository.AsQueryable();
                return options.Any() ? options.Max(x => x.Priority) + 1 : 1;
            }

            protected Maybe<OperationError> WithGlobalAdminRights(string errorMessage)
            {
                var isGlobalAdmin = _activeUserContext.IsGlobalAdmin();
                return isGlobalAdmin
                    ? Maybe<OperationError>.None
                    : new OperationError(errorMessage, OperationFailure.Forbidden);
            }
    }
    

}
