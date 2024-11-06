using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
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

        protected Result<IEnumerable<TOptionType>, OperationError> Get()
        {
            return WithGlobalAdminRights("User is not allowed to read global options")
                .Match(error => error,
                    () =>
                    {
                        var globalOptions = _globalOptionsRepository.AsQueryable().ToList();
                        return Result<IEnumerable<TOptionType>, OperationError>.Success(globalOptions);
                    });
        }

        protected int GetNextOptionPriority()
        {
            var options = _globalOptionsRepository.AsQueryable();
            return options.Any() ? options.Max(x => x.Priority) + 1 : 1;
        }

        private Maybe<OperationError> WithGlobalAdminRights(string errorMessage)
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

        private TOptionType Patch(TOptionType updatedOption)
        {
            _globalOptionsRepository.Update(updatedOption);
            _globalOptionsRepository.Save();
            _domainEvents.Raise(new EntityUpdatedEvent<TOptionType>(updatedOption));

            return updatedOption;
        }

        protected Result<TOptionType, OperationError> GetOptionWithGlobalAdminRights(Guid optionUuid)
        {
            return WithGlobalAdminRights($"User is not allowed to access global option with uuid {optionUuid}")
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

        protected Result<TOptionType, OperationError> PerformGlobalRegularOptionUpdates(TOptionType option, GlobalRegularOptionUpdateParameters parameters)
        {
            return option
                .WithOptionalUpdate(parameters.Description, (opt, description) => opt.UpdateDescription(description))
                .Bind(opt => opt.WithOptionalUpdate(parameters.Name, (opt, name) => opt.UpdateName(name)))
                .Bind(opt => opt.WithOptionalUpdate(parameters.IsObligatory, (opt, isObligatory) => opt.UpdateIsObligatory(isObligatory)))
                .Bind(opt => opt.WithOptionalUpdate(parameters.IsEnabled, (opt, isEnabled) => opt.UpdateIsEnabled(isEnabled)));
        }

        protected Result<TOptionType, OperationError> PerformGlobalOptionPriorityUpdates(TOptionType updatedOption,
            GlobalRegularOptionUpdateParameters updateParameters)
        {
            if (ShouldNotUpdatePriority(updateParameters))
                return Patch(updatedOption);

            var newPriority = updateParameters.Priority.NewValue.Value;
            return GetOptionToMove(newPriority).Match(optionToMove =>
            {
                var existingPriority = updatedOption.Priority;
                if (newPriority > existingPriority)
                {
                    updatedOption.IncreasePriority();
                    optionToMove.DecreasePriority();
                }
                else
                {
                    updatedOption.DecreasePriority();
                    optionToMove.IncreasePriority();
                }

                Patch(optionToMove);
                return Patch(updatedOption);
            },
                () => Patch(updatedOption));
        }

        private Maybe<TOptionType> GetOptionToMove(int newPriority)
        {
            return _globalOptionsRepository.AsQueryable().FirstOrDefault(o => o.Priority == newPriority).FromNullable();
        }

        private bool ShouldNotUpdatePriority(GlobalRegularOptionUpdateParameters updateParameters)
        {
            return !updateParameters.Priority.HasChange 
                   || !updateParameters.Priority.NewValue.HasValue
                   || updateParameters.Priority.NewValue.Value < 1;
        }
    }
}
