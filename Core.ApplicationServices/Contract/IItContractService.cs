﻿using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItContract;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.Contract
{
    public interface IItContractService
    {
        Result<ItContract, OperationFailure> Delete(int id);
    }
}
