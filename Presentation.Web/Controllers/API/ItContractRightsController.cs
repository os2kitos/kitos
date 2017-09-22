using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItContractRightController : GenericRightsController<ItContract, ItContractRight, ItContractRole>
    {
        private readonly IGenericRepository<ItContract> _objectRepository;
        public ItContractRightController(IGenericRepository<ItContractRight> rightRepository, IGenericRepository<ItContract> objectRepository) : base(rightRepository, objectRepository)
        {
            _objectRepository = objectRepository;
        }
    }
}
