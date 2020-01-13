﻿using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.Interface
{
    public interface IItInterfaceService
    {
        Result<ItInterface,OperationFailure> Delete(int id);
    }
}
