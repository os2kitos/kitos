using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;

namespace Core.ApplicationServices.GlobalOptions;

public class GlobalRoleOptionsService<TOptionType, TReferenceType> :
    GlobalOptionsServiceBase<TOptionType, TReferenceType>,
    IGlobalRoleOptionsService<TOptionType, TReferenceType>
    where TOptionType : OptionEntity<TReferenceType>, IRoleEntity, new()
{
    public Result<IEnumerable<TOptionType>, OperationError> GetGlobalOptions()
    {
        return Get();
    }

    public Result<TOptionType, OperationError> CreateGlobalOption(GlobalRoleOptionCreateParameters createParameters)
    {
        var newOption = new TOptionType()
        {
            Name = createParameters.Name,
            Description = createParameters.Description,
            IsObligatory = createParameters.IsObligatory,
            IsEnabled = false,
            Priority = GetNextOptionPriority(),
            HasWriteAccess = createParameters.WriteAccess
        };

        return Create(newOption);
    }

    public Result<TOptionType, OperationError> PatchGlobalOption(Guid optionUuid, GlobalRoleOptionUpdateParameters updateParameters)
    {
        throw new NotImplementedException();
    }

    public GlobalRoleOptionsService(IGenericRepository<TOptionType> globalOptionsRepository, IOrganizationalUserContext activeUserContext, IDomainEvents domainEvents) : base(globalOptionsRepository, activeUserContext, domainEvents)
    {
    }
}