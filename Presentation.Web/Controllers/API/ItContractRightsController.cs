using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItContractRightsController : GenericRightsController<ItContract, ItContractRight, ItContractRole>
    {
        private readonly IGenericRepository<ItContract> _objectRepository;
        public ItContractRightsController(IGenericRepository<ItContractRight> rightRepository, IGenericRepository<ItContract> objectRepository) : base(rightRepository, objectRepository)
        {
            _objectRepository = objectRepository;
        }
        
        /// <summary>
        /// Returns all ItContractRight for a specific user
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <returns>List of rights</returns>
        public HttpResponseMessage GetRightsForUser(int userId)
        {
            try
            {
                var theRights = new List<ItContractRight>();
                theRights.AddRange(RightRepository.Get(r => r.UserId == userId));

                //Get signed contracts
                var contracts = _objectRepository.Get(c => c.ContractSignerId == userId);

                foreach (var contract in contracts ?? Enumerable.Empty<ItContract>())
                {
                    var signerRole = new ItContractRole() { Name = "Kontraktunderskriver", HasReadAccess = true, HasWriteAccess = false };
                    var signerRight = new ItContractRight() { ObjectId = contract.Id, Object = contract, Role = signerRole };

                    theRights.Add(signerRight);
                }

                var dtos = AutoMapper.Mapper.Map<ICollection<ItContractRight>, ICollection<RightOutputDTO>>(theRights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
