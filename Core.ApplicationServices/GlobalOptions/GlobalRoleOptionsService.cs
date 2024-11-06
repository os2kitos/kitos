using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
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
        return GetOptionWithGlobalAdminRights(optionUuid)
            .Bind(existingOption => PerformGlobalRoleOptionUpdates(existingOption, updateParameters)
                .Bind(updatedOption =>
                {
                    if (!updateParameters.Priority.HasChange || !updateParameters.Priority.NewValue.HasValue) return Result<TOptionType, OperationError>.Success(Patch(updatedOption));

                    var newPriority = updateParameters.Priority.NewValue.Value;
                    var existingPriority = updatedOption.Priority;
                    var optionToMove = _globalOptionsRepository.AsQueryable().FirstOrDefault(o => o.Priority == newPriority);
                    if (optionToMove == null) return Result<TOptionType, OperationError>.Success(Patch(updatedOption));

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

                    return Result<TOptionType, OperationError>.Success(Patch(updatedOption, optionToMove));
                })
            );
    }

    private Result<TOptionType, OperationError> PerformGlobalRoleOptionUpdates(TOptionType option, GlobalRoleOptionUpdateParameters parameters)
    {
        return PerformGlobalRegularOptionUpdates(option, parameters)
            .Bind(opt => opt.WithOptionalUpdate(parameters.WriteAccess, (opt, writeAccess) => opt.HasWriteAccess = writeAccess.HasValue && writeAccess.Value)); ;
    }

    public GlobalRoleOptionsService(IGenericRepository<TOptionType> globalOptionsRepository, IOrganizationalUserContext activeUserContext, IDomainEvents domainEvents) : base(globalOptionsRepository, activeUserContext, domainEvents)
    {
    }
}