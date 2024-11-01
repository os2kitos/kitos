using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.GlobalOptions;

public class GenericGlobalOptionsService<TOptionType, TReferenceType> : IGenericGlobalOptionsService<TOptionType, TReferenceType> 
    where TOptionType : OptionEntity<TReferenceType>, new()
{
    private readonly IGenericRepository<TOptionType> _globalOptionsRepository;
    private readonly IOrganizationalUserContext _activeUserContext;
    private readonly IDomainEvents _domainEvents;


    public GenericGlobalOptionsService(IGenericRepository<TOptionType> globalOptionsRepository, IOrganizationalUserContext activeUserContext, IDomainEvents domainEvents)
    {
        _globalOptionsRepository = globalOptionsRepository;
        _activeUserContext = activeUserContext;
        _domainEvents = domainEvents;
    }
    public Result<IEnumerable<TOptionType>, OperationError> GetGlobalOptions()
    {
        return WithGlobalAdminRights("User is not allowed to read global options")
            .Match(error => error,
                () =>
                {
                    var globalOptions = _globalOptionsRepository.AsQueryable().ToList();
                    return Result<IEnumerable<TOptionType>, OperationError>.Success(globalOptions);
                });
    }

    public Result<TOptionType, OperationError> CreateGlobalOption(GlobalOptionCreateParameters createParameters)
    {
        return WithGlobalAdminRights("User is not allowed to create global options")
            .Match(error => error,
                () =>
                {
                    var globalOption = new TOptionType()
                    {
                        Name = createParameters.Name,
                        Description = createParameters.Description,
                        IsObligatory = createParameters.IsObligatory,
                        IsEnabled = false,
                    };
                    _globalOptionsRepository.Insert(globalOption);
                    _globalOptionsRepository.Save();
                    _domainEvents.Raise(new EntityCreatedEvent<TOptionType>(globalOption));

                    return Result<TOptionType, OperationError>.Success(globalOption);
                });
    }

    public Result<TOptionType, OperationError> PatchGlobalOption(GlobalOptionUpdateParameters updateParameters)
    {
        throw new NotImplementedException();
    }

    private Maybe<OperationError> WithGlobalAdminRights(string errorMessage)
    {
        var isGlobalAdmin = _activeUserContext.IsGlobalAdmin();
        return isGlobalAdmin 
            ? Maybe<OperationError>.None 
            : new OperationError(errorMessage, OperationFailure.Forbidden);
    }
}