using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.GlobalOptions;

public class GenericGlobalOptionsService<TOptionType, TReferenceType> : IGenericGlobalOptionsService<TOptionType, TReferenceType> 
    where TOptionType : OptionEntity<TReferenceType>
{
    private readonly IGenericRepository<TOptionType> _globalOptionsRepository;
    private readonly IAuthorizationContext _authorizationContext;


    public GenericGlobalOptionsService(IGenericRepository<TOptionType> globalOptionsRepository, IAuthorizationContext authorizationContext)
    {
        _globalOptionsRepository = globalOptionsRepository;
        _authorizationContext = authorizationContext;
    }
    public Result<IEnumerable<TOptionType>, OperationError> GetGlobalOptions()
    {
        var readAccessLevel = _authorizationContext.GetReadAccessLevel<TOptionType>();
        if (readAccessLevel != EntityReadAccessLevel.All) return new OperationError("User is not allowed to read global options", OperationFailure.Forbidden);
        var globalOptions = _globalOptionsRepository.AsQueryable().ToList();
        return Result<IEnumerable<TOptionType>, OperationError>.Success(globalOptions);
    }

    public Result<TOptionType, OperationError> CreateGlobalOption()
    {
        throw new NotImplementedException();
    }

    public Result<TOptionType, OperationError> PatchGlobalOption(GlobalOptionUpdateParameters updateParameters)
    {
        throw new NotImplementedException();
    }
}