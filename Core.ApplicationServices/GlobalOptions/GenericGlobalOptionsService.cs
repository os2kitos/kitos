using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.GlobalOptions;

public class GenericGlobalOptionsService<TOptionType, TReferenceType> : IGenericGlobalOptionsService<TOptionType, TReferenceType> 
    where TOptionType : OptionEntity<TReferenceType>, new()
{
    private readonly IGenericRepository<TOptionType> _globalOptionsRepository;
    private readonly IOrganizationalUserContext _activeUserContext;


    public GenericGlobalOptionsService(IGenericRepository<TOptionType> globalOptionsRepository, IOrganizationalUserContext activeUserContext)
    {
        _globalOptionsRepository = globalOptionsRepository;
        _activeUserContext = activeUserContext;
    }
    public Result<IEnumerable<TOptionType>, OperationError> GetGlobalOptions()
    {
        var isGlobalAdmin = _activeUserContext.IsGlobalAdmin();
        if (!isGlobalAdmin) return new OperationError("User is not allowed to read global options", OperationFailure.Forbidden);
        
        var globalOptions = _globalOptionsRepository.AsQueryable().ToList();
        return Result<IEnumerable<TOptionType>, OperationError>.Success(globalOptions);
    }

    public Result<TOptionType, OperationError> CreateGlobalOption(GlobalOptionCreateParameters createParameters)
    {
        var isGlobalAdmin = _activeUserContext.IsGlobalAdmin();
        if (!isGlobalAdmin) return new OperationError("User is not allowed to create global options", OperationFailure.Forbidden);

        var globalOption = new TOptionType()
        {
            Name = createParameters.Name,
            Description = createParameters.Description,
            IsObligatory = createParameters.IsObligatory,
            IsEnabled = false,
        };
        _globalOptionsRepository.Insert(globalOption);
        _globalOptionsRepository.Save();
        return globalOption;
    }

    public Result<TOptionType, OperationError> PatchGlobalOption(GlobalOptionUpdateParameters updateParameters)
    {
        throw new NotImplementedException();
    }
}