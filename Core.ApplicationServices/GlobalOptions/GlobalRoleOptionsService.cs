using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;

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
            IsLocallyAvailable = false,
            Priority = GetNextOptionPriority(),
            HasWriteAccess = createParameters.WriteAccess
        };

        return Create(newOption);
    }

    public Result<TOptionType, OperationError> PatchGlobalOption(Guid optionUuid, GlobalRoleOptionUpdateParameters updateParameters)
    {
        return GetOptionWithGlobalAdminRights(optionUuid)
            .Bind(existingOption => PerformGlobalRoleOptionUpdates(existingOption, updateParameters)
                .Bind(updatedOption => PerformGlobalOptionPriorityUpdates(updatedOption, updateParameters))
            );
    }

    private Result<TOptionType, OperationError> PerformGlobalRoleOptionUpdates(TOptionType option, GlobalRoleOptionUpdateParameters parameters)
    {
        return PerformGlobalRegularOptionUpdates(option, parameters)
            .Bind(opt => opt.WithOptionalUpdate(parameters.WriteAccess, (opt, writeAccess) => opt.HasWriteAccess = writeAccess.HasValue && writeAccess.Value));
    }

    public GlobalRoleOptionsService(IGenericRepository<TOptionType> globalOptionsRepository, IOrganizationalUserContext activeUserContext, IDomainEvents domainEvents, ITransactionManager transactionManager) : base(globalOptionsRepository, activeUserContext, domainEvents, transactionManager)
    {
    }
}