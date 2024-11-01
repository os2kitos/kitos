﻿using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.DomainModel;

namespace Core.ApplicationServices.GlobalOptions
{
    public interface IGenericGlobalOptionsService<TOptionType, TReferenceType>
        where TOptionType : OptionEntity<TReferenceType>
    {

        Result<IEnumerable<TOptionType>, OperationError> GetGlobalOptions();
        Result<TOptionType, OperationError> CreateGlobalOption();

        Result<TOptionType, OperationError> PatchGlobalOption(GlobalOptionUpdateParameters updateParameters);
    }
}
