using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;
using dk.nita.saml20.Schema.Core;

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
            IsLocallyAvailable = true,
            Priority = GetNextOptionPriority()
        };

        return Create(newOption);
    }

    public Result<TOptionType, OperationError> PatchGlobalOption(Guid optionUuid, GlobalRegularOptionUpdateParameters updateParameters)
    {
        return GetOptionWithGlobalAdminRights(optionUuid)
            .Bind(existingOption => PerformGlobalRegularOptionUpdates(existingOption, updateParameters)
                    .Bind(updatedOption => PerformGlobalOptionPriorityUpdates(updatedOption, updateParameters))
            );
    }
}