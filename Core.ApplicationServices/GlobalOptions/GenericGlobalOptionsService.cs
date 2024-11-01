using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices.GlobalOptions;

public class GenericGlobalOptionsService<TOptionType, TReferenceType> : IGenericGlobalOptionsService<TOptionType, TReferenceType> 
    where TOptionType : OptionEntity<TReferenceType>
{
    private readonly IGenericRepository<TOptionType> _globalOptionsRepository;

    public GenericGlobalOptionsService(IGenericRepository<TOptionType> globalOptionsRepository)
    {
        _globalOptionsRepository = globalOptionsRepository;
    }
    public Result<IEnumerable<TOptionType>, OperationError> GetGlobalOptions()
    {
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