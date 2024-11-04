using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;
using Core.DomainServices.Extensions;

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

            protected Result<TOptionType, OperationError> Create(TOptionType option)
            {
                return WithGlobalAdminRights("User is not allowed to create global options")
                    .Match(error => error,
                        () =>
                        {
                            _globalOptionsRepository.Insert(option);
                            _globalOptionsRepository.Save();
                            _domainEvents.Raise(new EntityCreatedEvent<TOptionType>(option));

                            return Result<TOptionType, OperationError>.Success(option);
                        });
            }

            protected TOptionType Patch(TOptionType updatedOption)
            {
            _globalOptionsRepository.Update(updatedOption);
            _globalOptionsRepository.Save();
            _domainEvents.Raise(new EntityUpdatedEvent<TOptionType>(updatedOption));

            return updatedOption;
            }

        protected Result<TOptionType, OperationError> GetOptionWithGlobalAdminRights(Guid optionUuid)
            {
                return WithGlobalAdminRights($"User is not allowed to patch global option with uuid {optionUuid}")
                    .Match(error => error,
                        () =>
                        {
                            var existingOption = _globalOptionsRepository.AsQueryable().ByUuid(optionUuid);
                            return (existingOption != null)
                                ? Result<TOptionType, OperationError>.Success(existingOption)
                                : new OperationError($"Unable to find global option with uuid {optionUuid}",
                                    OperationFailure.NotFound);
                        });
            }
    }
    

}
