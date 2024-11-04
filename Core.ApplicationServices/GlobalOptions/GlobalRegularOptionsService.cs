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

public class GlobalRegularOptionsService<TOptionType, TReferenceType> : 
    GlobalOptionsServiceBase<TOptionType, TReferenceType>,
    IGlobalRegularOptionsService<TOptionType, TReferenceType> 
    where TOptionType : OptionEntity<TReferenceType>, new()
{

    public GlobalRegularOptionsService(IGenericRepository<TOptionType> globalOptionsRepository, IOrganizationalUserContext activeUserContext, IDomainEvents domainEvents) : base(globalOptionsRepository, activeUserContext, domainEvents)
    {}

    public Result<IEnumerable<TOptionType>, OperationError> GetGlobalOptions()
    {
        return Get();
    }

    public Result<TOptionType, OperationError> CreateGlobalOption(GlobalRegularOptionCreateParameters createParameters)
    {
        var newOption = new TOptionType()
        {
            Name = createParameters.Name,
            Description = createParameters.Description,
            IsObligatory = createParameters.IsObligatory,
            IsEnabled = false,
            Priority = GetNextOptionPriority()
        };

        return Create(newOption);
    }

    public Result<TOptionType, OperationError> PatchGlobalOption(Guid optionUuid, GlobalRegularOptionUpdateParameters updateParameters)
    {
        return GetOptionWithGlobalAdminRights(optionUuid)
            .Bind(existingOption => PerformGlobalOptionUpdates(existingOption, updateParameters)
                    .Bind(updatedOption => Result<TOptionType, OperationError>.Success(Patch(updatedOption)))
            );
    }

    private Result<TOptionType, OperationError> PerformGlobalOptionUpdates(TOptionType option, GlobalRegularOptionUpdateParameters parameters)
    {
        return option
            .WithOptionalUpdate(parameters.Description, (opt, description) => opt.UpdateDescription(description))
            .Bind(opt => opt.WithOptionalUpdate(parameters.Name, (opt, name) => opt.UpdateName(name)))
            .Bind(opt => opt.WithOptionalUpdate(parameters.IsObligatory, (opt, isObligatory) => opt.UpdateIsObligatory(isObligatory)))
            .Bind(opt => opt.WithOptionalUpdate(parameters.IsEnabled, (opt, isEnabled) => opt.UpdateIsEnabled(isEnabled)));
    }
}