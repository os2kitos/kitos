﻿using System.Linq;
using Core.DomainModel.ItContract;

namespace Core.DomainServices.Repositories.Contract
{
    public interface IItContractRepository
    {
        IQueryable<ItContract> GetBySystemUsageAssociation(int systemUsageId);
        ItContract GetById(int contractId);
    }
}
